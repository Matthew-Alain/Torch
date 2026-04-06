using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Debug;

public abstract class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] protected SpriteRenderer rend;     //Derived tiles can now access
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject validHighlight;
    [SerializeField] private GameObject invalidHighlight;
    public bool isWalkable;
    public bool isDifficult;
    public bool isSwimmable;
    public bool isClimbable;
    public string TileName;
    public int tileEncounter, tileID, tileX, tileY;

    public BaseUnit OccupiedUnit;

    //This logic runs on all tiles, but each tile has the chance to override it
    public virtual void Init(int encounterID, int id, int x, int y)
    {
        tileEncounter = encounterID;
        tileID = id;
        tileX = x;
        tileY = y;
    }

    public void ShowValidTargeting(TargetType type)
    {
        switch (type)
        {
            case TargetType.Unit:
                if (OccupiedUnit != null)
                {
                    validHighlight.SetActive(true);
                }
                else
                {
                    invalidHighlight.SetActive(true);
                }
                break;
            case TargetType.Monster:
                if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.Monster)
                {
                    validHighlight.SetActive(true);
                }
                else
                {
                    invalidHighlight.SetActive(true);
                }
                break;
            case TargetType.PC:
                if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.PC)
                {
                    validHighlight.SetActive(true);
                }
                else
                {
                    invalidHighlight.SetActive(true);
                }
                break;
            case TargetType.AnyTile:
                validHighlight.SetActive(true);
                break;
            case TargetType.EmptyTile:
                if (OccupiedUnit == null)
                {
                    validHighlight.SetActive(true);
                }
                else
                {
                    invalidHighlight.SetActive(true);
                }
                break;
        }
    }

    public void HideValidTargeting()
    {
        validHighlight.SetActive(false);
        invalidHighlight.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.SetActive(true);
        CombatMenuManager.Instance.ShowTileInfo(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.SetActive(false);
        CombatMenuManager.Instance.ShowTileInfo(null);
    }

    public IEnumerator MoveUnit(BaseUnit movingUnit, bool forced)
    {
        bool moving = true;
        if (!forced)
        {
            var context = new MoveContext
            {
                TriggeringUnit = movingUnit,
                originTile = movingUnit.occupiedTile,
                destinationTile = this
            };

            // Log("About to check for reactions");

            yield return StartCoroutine(ReactionManager.Instance.CheckForReactions(
                ReactionTrigger.UnitMoves,
                context
            ));
            // Log("returned from checking for reactions");

            // Log("Should stop? "+TurnUtility.ShouldStop(movingUnit));
            if (TurnUtility.ShouldStop(movingUnit)) yield break;

            int unitSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT current_speed FROM unit_resources WHERE id = {movingUnit.UnitID}"
            ));

            //If tile is difficult terrain, multiply by 10 instead
            int movementCost = 0;
            if (CostsExtra(movingUnit))
            {
                movementCost = CheckDistanceInTiles(movingUnit.occupiedTile) * 10;
            }
            else
            {
                movementCost = CheckDistanceInTiles(movingUnit.occupiedTile) * 5;
            }

            if (movementCost > unitSpeed || unitSpeed <= 0)
            {
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{movingUnit.UnitName} does not have enough movement to move to this tile"));
                moving = false;
            }
            else
            {
                int newSpeed = unitSpeed - movementCost;

                DatabaseManager.Instance.ExecuteNonQuery(
                    $"UPDATE unit_resources SET current_speed = {newSpeed} WHERE id = {movingUnit.UnitID}"
                );
                // yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{movingUnit.UnitName} has {newSpeed} feet of movement left"));
                // Log("Unit has " + newSpeed + " feet of movement left.");
            }

            CombatMenuManager.Instance.ReRenderMenu();
        }

        if (moving)
        {
            yield return StartCoroutine(SetUnit(movingUnit)); //Set this tile's unit as the selected unit
            if(movingUnit.Faction == Faction.Monster)
                yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator MoveUnit(BaseUnit movingUnit)
    {
        yield return StartCoroutine(MoveUnit(movingUnit, false));
    }

    public IEnumerator SetUnit(BaseUnit unit)
    {
        if(unit != null)
        {
            if (unit.occupiedTile != null)
            {
                // Go to unit's occupied tile, and set it's occupied unit to null (for when this unit is moving from a previous tile to this one)
                int oldX = unit.occupiedTile.tileX;
                int oldY = unit.occupiedTile.tileY;

                DatabaseManager.Instance.ExecuteNonQuery(
                    $"UPDATE grid_contents SET unit_id = NULL WHERE encounter_id = {tileEncounter} AND x = {oldX} AND y = {oldY}"
                );
                unit.occupiedTile.OccupiedUnit = null;
            }
            unit.transform.position = transform.position;
            OccupiedUnit = unit;
            unit.occupiedTile = this;

            DatabaseManager.Instance.ExecuteNonQuery(
                $"UPDATE grid_contents SET unit_id = {unit.UnitID} WHERE encounter_id = {tileEncounter} AND x = {tileX} AND y = {tileY}"
            );
            
            yield return null;
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(CombatUnitManager.Instance.SelectedPC == null)
            return;

        CombatMenuManager menu = CombatMenuManager.Instance;

        bool leftClick = eventData.button == PointerEventData.InputButton.Left;
        GameState currentState = CombatStateManager.Instance.GameState;

        if (currentState == GameState.GenerateGrid ||
            currentState == GameState.SpawnPCs ||
            currentState == GameState.SpawnMonsters ||
            // currentState == GameState.Precombat ||
            currentState == GameState.RollInitiative ||
            currentState == GameState.MonsterTurn
        ) return; //If it's not your turn, you can't click

        if (!leftClick)
        {
            //Only close this if the stack has more than one layer
            // menu.CloseMenu();
            return;
        }

        switch (currentState)
        {
            case GameState.PlayerTurn:
                if (OccupiedUnit != null)
                {
                    if(OccupiedUnit == InitiativeTracker.Instance.currentTurnUnit)
                    {
                        if(OccupiedUnit.GetCondition("dying") || OccupiedUnit.GetCondition("unconscious") || OccupiedUnit.GetCondition("dead"))
                        {
                            StartCoroutine(menu.DisplayText($"{OccupiedUnit.UnitName} is unable to act this turn."));
                            return;
                        }

                        SelectPC(); //Select that PC
                    }
                    
                }
                else
                {
                    ClearUnitSelection();
                }
                break;

            case GameState.MovingPC:
                if (isWalkable && OccupiedUnit == null)
                {
                    int distance = CheckDistanceInTiles(CombatUnitManager.Instance.SelectedPC.occupiedTile);
                    if(distance == 1)
                    {
                        StartCoroutine(MoveUnit(CombatUnitManager.Instance.SelectedPC));
                    }
                    else
                    {
                        // Log("Please move one tile at a time");
                    }
                }
                else
                {
                    StartCoroutine(menu.DisplayText("That tile is not walkable"));
                }
                break;

            //These handle clicking tiles
            case GameState.SelectTargetMonster:
                if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.Monster)
                {
                    // StartCoroutine(CombatStateManager.Instance.ConfirmTarget(OccupiedUnit));
                    StartCoroutine(CombatStateManager.Instance.ConfirmTileTargetSelection(this));
                }
                else
                {
                    StartCoroutine(menu.DisplayText("You are only allowed to select monsters"));
                }
                break;
            
            case GameState.SelectTargetPC:
                if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.PC)
                {
                    // StartCoroutine(CombatStateManager.Instance.ConfirmTarget(OccupiedUnit));
                    StartCoroutine(CombatStateManager.Instance.ConfirmTileTargetSelection(this));
                }
                else
                {
                    StartCoroutine(menu.DisplayText("You are only allowed to select PCs"));
                }
                break;
            
            case GameState.SelectTargetUnit:
                if (OccupiedUnit != null)
                {
                    // StartCoroutine(CombatStateManager.Instance.ConfirmTarget(OccupiedUnit));
                    StartCoroutine(CombatStateManager.Instance.ConfirmTileTargetSelection(this));
                }
                else
                {
                    StartCoroutine(menu.DisplayText("There's no unit there that tile"));
                }
                break;
            
            case GameState.SelectTargetTile:
                // StartCoroutine(CombatStateManager.Instance.ConfirmTile(this));
                StartCoroutine(CombatStateManager.Instance.ConfirmTileTargetSelection(this));
                break;

            case GameState.SelectTargetEmptyTile:
                if (OccupiedUnit == null)
                {
                    // StartCoroutine(CombatStateManager.Instance.ConfirmTile(this));
                    StartCoroutine(CombatStateManager.Instance.ConfirmTileTargetSelection(this));
                }
                else
                {
                    StartCoroutine(menu.DisplayText("You must select an empty tile"));
                }
                break;
            
            default:
                Log("No click action for the current game state: " + currentState);
                break;
        }
    }
    
    private void ClearUnitSelection()
    {
        CombatMenuManager.Instance.CloseAllMenus();
        // CombatUnitManager.Instance.SetSelectedPC(null);
        // StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));
    }

    private void SelectPC()
    {
        CombatMenuManager.Instance.CloseAllMenus(); //Close any existing menu
        // CombatUnitManager.Instance.SetSelectedPC((BasePC)OccupiedUnit); //Select the newly clicked PC
        CombatMenuManager.Instance.OpenRootMenu(); //Open the action menu
    }

    //Returns the number of tiles between two tiles
    public int CheckDistanceInTiles(Tile targetTile)
    {
        int xDifference = Mathf.Abs(tileX - targetTile.tileX);
        int yDifference = Mathf.Abs(tileY - targetTile.tileY);

        return Mathf.Max(xDifference, yDifference);
    }

    // private bool CheckWithinUnitSpeed(int numberOfTiles, BaseUnit movingUnit)
    // {
    //     decimal unitSpeed = Convert.ToDecimal(DatabaseManager.Instance.ExecuteScalar(
    //         $"SELECT current_speed FROM unit_resources WHERE id = {movingUnit.UnitID}"
    //     )) / 5;

    //     int speedInTiles = (int)Math.Floor(unitSpeed);

    //     return numberOfTiles <= speedInTiles;
    // }

    public void EmptyTile()
    {
        if (OccupiedUnit != null)
        {
            Destroy(OccupiedUnit.gameObject);
            OccupiedUnit = null;
        }
        else
        {
            LogWarning("This tile was already empty");
        }
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE grid_contents SET unit_id = NULL WHERE x = {tileX} AND y = {tileY}");
    }
    
    public bool CostsExtra(BaseUnit movingUnit)
    {
        if (isDifficult)
        {
            if ((isSwimmable && movingUnit.CanSwim()) || (isClimbable && movingUnit.CanClimb()))
            {
                return false;
            }
            return true;
        }

        return false;
    }
}

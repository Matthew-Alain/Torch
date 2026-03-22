using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Debug;

public abstract class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] protected SpriteRenderer rend;     //Derived tiles can now access
    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isWalkable;
    public string TileName;
    public int tileEncounter, tileID, tileX, tileY;

    public BaseUnit OccupiedUnit;
    public bool Walkable => isWalkable && OccupiedUnit == null;

    //This logic runs on all tiles, but each tile has the chance to override it
    public virtual void Init(int encounterID, int id, int x, int y)
    {
        tileEncounter = encounterID;
        tileID = id;
        tileX = x;
        tileY = y;
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

    public void SetUnit(BaseUnit unit)
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
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CombatMenuManager menu = CombatMenuManager.Instance;

        bool leftClick = eventData.button == PointerEventData.InputButton.Left;
        GameState currentState = CombatStateManager.Instance.GameState;

        if (currentState == GameState.GenerateGrid ||
            currentState == GameState.SpawnHeroes ||
            currentState == GameState.SpawnMonsters ||
            currentState == GameState.Precombat ||
            currentState == GameState.RollInitiative ||
            currentState == GameState.MonsterTurn
        ) return; //If it's not your turn, you can't click

        if (!leftClick)
        {
            menu.CloseMenu();
            return;
        }

        switch (currentState)
        {
            case GameState.PlayerTurn:
                if (OccupiedUnit != null)
                {
                    if(OccupiedUnit.Faction == Faction.PC)
                    {
                        if(OccupiedUnit.GetCondition("dying") || OccupiedUnit.GetCondition("unconscious") || OccupiedUnit.GetCondition("dead"))
                        {
                            menu.DisplayText($"{OccupiedUnit.UnitName} is unable to act this turn.");
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
                if (isWalkable)
                {
                    int distance = CheckDistanceInTiles(CombatUnitManager.Instance.SelectedPC.occupiedTile);
                    bool hasEnoughSpeed = CheckWithinUnitSpeed(distance, CombatUnitManager.Instance.SelectedPC);
                    if (hasEnoughSpeed)
                    {
                        if(distance == 1)
                        {
                            MoveUnit(CombatUnitManager.Instance.SelectedPC);
                        }
                        else
                        {
                            // Log("Please move one tile at a time");
                        }
                    }
                    else
                    {
                        // Log("Tile out of range");
                    }
                }
                else
                {
                    menu.DisplayText("That tile is not walkable");
                }
                break;
            
            case GameState.SelectAttackTarget:
                if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.Monster)
                {
                    int melee_range = 0;
                    int normal_range = 0;
                    int long_range = 0;

                    DatabaseManager.Instance.ExecuteReader(
                        $"SELECT melee_range, normal_range, long_range FROM weapons WHERE id = {CombatStateManager.Instance.declaredWeapon}",
                        reader =>
                        {
                            while (reader.Read())
                            {
                                if (reader["melee_range"] != DBNull.Value) melee_range = Convert.ToInt32(reader["melee_range"]);
                                if (reader["normal_range"] != DBNull.Value) normal_range = Convert.ToInt32(reader["normal_range"]);
                                if (reader["long_range"] != DBNull.Value) long_range = Convert.ToInt32(reader["long_range"]);
                            }
                        }
                    );

                    int distance = CheckDistanceInTiles(CombatUnitManager.Instance.SelectedPC.occupiedTile) * 5;
                    if (distance <= melee_range)
                    {
                        CombatActions.MeleeWeaponAttack(CombatUnitManager.Instance.SelectedPC, CombatStateManager.Instance.declaredWeapon, OccupiedUnit);
                        
                    }
                    else if (distance <= normal_range)
                    {
                        CombatActions.RangedWeaponAttack(CombatUnitManager.Instance.SelectedPC, OccupiedUnit);
                    }
                    else if (distance <= long_range)
                    {
                        CombatActions.LongRangeWeaponAttack(CombatUnitManager.Instance.SelectedPC, OccupiedUnit);
                    }
                    else
                    {
                        menu.DisplayText("The target is out of range");
                        // Log("The target is out of range");
                    }
                }
                else
                {
                    menu.DisplayText("That's an invalid target");
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
        CombatUnitManager.Instance.SetSelectedPC(null);
        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    private void SelectPC()
    {
        CombatMenuManager.Instance.CloseAllMenus(); //Close any existing menu
        CombatUnitManager.Instance.SetSelectedPC((BasePC)OccupiedUnit); //Select the newly clicked PC
        CombatMenuManager.Instance.OpenRootMenu(); //Open the action menu
    }

    //Returns the number of tiles between two tiles
    public int CheckDistanceInTiles(Tile targetTile)
    {
        int xDifference = Mathf.Abs(tileX - targetTile.tileX);
        int yDifference = Mathf.Abs(tileY - targetTile.tileY);

        return Mathf.Max(xDifference, yDifference);
    }

    private bool CheckWithinUnitSpeed(int numberOfTiles, BaseUnit movingUnit)
    {
        decimal unitSpeed = Convert.ToDecimal(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT current_speed FROM unit_resources WHERE id = {movingUnit.UnitID}"
        )) / 5;

        int speedInTiles = (int)Math.Floor(unitSpeed);

        return numberOfTiles <= speedInTiles;
    }

    public void MoveUnit(BaseUnit movingUnit)
    {
        int unitSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT current_speed FROM unit_resources WHERE id = {movingUnit.UnitID}"
        ));

        //If tile is difficult terrain, multiply by 10 instead
        int amountMoved = CheckDistanceInTiles(movingUnit.occupiedTile) * 5;

        int newSpeed = unitSpeed - amountMoved;

        SetUnit(movingUnit); //Set this tile's unit as the selected unit

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE unit_resources SET current_speed = {newSpeed} WHERE id = {movingUnit.UnitID}"
        );

        CombatMenuManager.Instance.DisplayText($"{OccupiedUnit.UnitName} has {newSpeed} feet of movement left");
        // Log("Unit has " + newSpeed + " feet of movement left.");

        if (newSpeed == 0)
        {
            // CombatUnitManager.Instance.SetSelectedPC(null); //And deselect the PC
            CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
        }
    }
    
    public void EmptyTile()
    {
        if(OccupiedUnit != null)
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
}

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
    public int TileID;
    public int tileEncounter, tileX, tileY;

    public BaseUnit OccupiedUnit;
    public bool Walkable => isWalkable && OccupiedUnit == null;

    //This logic runs on all tiles, but each tile has the chance to override it
    public virtual void Init(int encounterID, int x, int y)
    {
        tileEncounter = encounterID;
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
                "UPDATE grid_contents SET content = NULL WHERE encounter_id = @encounterID AND x = @x AND y = @y",
                ("@encounterID", tileEncounter),
                ("@x", oldX),
                ("@y", oldY)
            );
            unit.occupiedTile.OccupiedUnit = null;
        }
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.occupiedTile = this;

        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE grid_contents SET content = (@unitID) WHERE encounter_id = @encounterID AND x = @x AND y = @y",
            ("@unitID", unit.UnitID),
            ("@encounterID", tileEncounter),
            ("@x", tileX),
            ("@y", tileY)
        );
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        bool leftClick = eventData.button == PointerEventData.InputButton.Left;
        GameState currentState = CombatStateManager.Instance.GameState;

        if (currentState == GameState.GenerateGrid ||
            currentState == GameState.SpawnHeroes ||
            currentState == GameState.SpawnMonsters ||
            currentState == GameState.Precombat ||
            currentState == GameState.RollInitiative ||
            currentState == GameState.MonsterTurn
        ) return; //If it's not your turn, you can't click

        switch (currentState)
        {
            case GameState.PlayerTurn:
                if (leftClick)
                {
                    if (OccupiedUnit != null && OccupiedUnit.Faction == Faction.PC) //If you click on tile containing a PC
                    {
                        SelectPC(); //Select that PC
                    }
                }
                else
                {
                    ClearUnitSelection();
                }
                break;
            
            case GameState.MovingPC:
                if (leftClick)
                {
                    if(OccupiedUnit != null)
                    {
                        Log("You cannot move through units");
                        //Check if halfling
                    }
                    else if (isWalkable)
                    {
                        int distance = CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this);
                        bool hasEnoughSpeed = CheckWithinUnitSpeed(distance, CombatUnitManager.Instance.SelectedPC);
                        if (hasEnoughSpeed)
                        {
                            if(distance == 1)
                            {
                                MoveUnit(CombatUnitManager.Instance.SelectedPC);
                            }
                            else
                            {
                                Log("Please move one tile at a time");
                            }
                        }
                        else
                        {
                            Log("Tile out of range");
                        }
                    }
                }
                else
                {
                    CombatStateManager.Instance.ChangeState(GameState.PlayerTurn); //Cancel moving
                    Log("No longer moving");
                }
                break;
            
            case GameState.SelectAttackTarget:
                if (leftClick)
                {
                    int melee_range = 0;
                    int normal_range = 0;
                    int long_range = 0;

                    DatabaseManager.Instance.ExecuteReader(
                        "SELECT melee_range, normal_range, long_range FROM weapons WHERE id = @weaponID",
                        reader =>
                        {
                            while (reader.Read())
                            {
                                if (reader["melee_range"] != DBNull.Value) melee_range = Convert.ToInt32(reader["melee_range"]);
                                if (reader["normal_range"] != DBNull.Value) normal_range = Convert.ToInt32(reader["normal_range"]);
                                if (reader["long_range"] != DBNull.Value) long_range = Convert.ToInt32(reader["long_range"]);
                            }
                        },
                        ("@weaponID", CombatStateManager.Instance.declaredWeapon)
                    );

                    if(OccupiedUnit.Faction == Faction.Monster)
                    {
                        int distance = CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this) * 5;
                        if (distance <= melee_range)
                        {
                            CombatActions.MeleeWeaponAttack(CombatUnitManager.Instance.SelectedPC.UnitID, CombatStateManager.Instance.declaredWeapon, OccupiedUnit.UnitID);
                        }
                        else if (distance <= normal_range)
                        {
                            CombatActions.RangedWeaponAttack(CombatUnitManager.Instance.SelectedPC.UnitID, OccupiedUnit.UnitID);
                        }
                        else if (distance <= long_range)
                        {
                            CombatActions.LongRangeWeaponAttack(CombatUnitManager.Instance.SelectedPC.UnitID, OccupiedUnit.UnitID);
                        }
                        else
                        {
                            Log("The target is out of range");
                        }
                    }
                    else
                    {
                        Log("You can only attack monsters");
                    }
                    
                }
                else
                {
                    CombatStateManager.Instance.ChangeState(GameState.PlayerTurn); //Cancel moving
                    Log("No longer attacking");
                }
                break;
            
            default:
                Log("No click action for the current game state: " + currentState);
                break;
        }
    }
    
    private void ClearUnitSelection()
    {
        CombatUnitManager.Instance.SetSelectedPC(null);
        CombatMenuManager.Instance.CloseAllMenus();
        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    private void SelectPC()
    {
        CombatMenuManager.Instance.CloseAllMenus(); //Close any existing menu
        CombatUnitManager.Instance.SetSelectedPC((BasePC)OccupiedUnit); //Select the newly clicked PC
        CombatMenuManager.Instance.OpenRootMenu(); //Open the action menu
    }

    //         else //Means you must be selecting a monster
    //         {
    //             if (CombatUnitManager.Instance.SelectedPC != null) //If you have a PC already selected
    //             {
    //                 var monster = (BaseMonster)OccupiedUnit;
    //                 // Put in what happens when a selected PC clicks on a monster
    //                 Destroy(monster.gameObject); //In this case, it dies in one hit
    //                 CombatUnitManager.Instance.SetSelectedPC(null); //Deselect the current unit
    //             }
    //         }


        
    //     else //If you click on an empty tile
    //     {
    //         if (CombatUnitManager.Instance.SelectedPC != null && isWalkable) //If you have a PC already selected, and the tile is walkable
    //         {
    //             bool inRange = CheckWithinUnitSpeed(CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this), CombatUnitManager.Instance.SelectedPC);
    //             if (inRange)
    //             {
    //                 MoveUnit(CombatUnitManager.Instance.SelectedPC);
    //             }
    //             else
    //             {
    //                 Log("Tile out of range");
    //             }
    //         }
    //     }
    // }

    //Returns the number of tiles between two tiles
    public int CheckDistance(Tile originTile, Tile targetTile)
    {
        int xDifference = Mathf.Abs(originTile.tileX - targetTile.tileX);
        int yDifference = Mathf.Abs(originTile.tileY - targetTile.tileY);

        return Mathf.Max(xDifference, yDifference);
    }

    private bool CheckWithinUnitSpeed(int numberOfTiles, BaseUnit movingUnit)
    {
        decimal unitSpeed = Convert.ToDecimal(DatabaseManager.Instance.ExecuteScalar(
            "SELECT current_speed FROM unit_resources WHERE id = (@PCID)",
            ("@PCID", movingUnit.UnitID)
        )) / 5;

        int speedInTiles = (int)Math.Floor(unitSpeed);

        return numberOfTiles <= speedInTiles;
    }

    private void MoveUnit(BaseUnit movingUnit)
    {
        int unitSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT current_speed FROM unit_resources WHERE id = (@PCID)",
            ("@PCID", movingUnit.UnitID)
        ));

        //If tile is difficult terrain, multiply by 10 instead
        int amountMoved = CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this) * 5;

        int newSpeed = unitSpeed - amountMoved;

        SetUnit(movingUnit); //Set this tile's unit as the selected unit

        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE unit_resources SET current_speed = (@newSpeed) WHERE id = @unitID",
            ("@newSpeed", newSpeed),
            ("@unitID", movingUnit.UnitID)
        );

        Log("Unit has " + newSpeed + " feet of movement left.");
        
        if(newSpeed == 0)
        {
            // CombatUnitManager.Instance.SetSelectedPC(null); //And deselect the PC
            CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
        }
    }
}

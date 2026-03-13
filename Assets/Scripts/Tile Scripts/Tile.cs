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
        if (CombatStateManager.Instance.GameState != GameState.PlayerTurn) return; //If it's not your turn, you can't click

        if (OccupiedUnit != null) //If you click on a tile that's not empty
        {
            if (OccupiedUnit.Faction == Faction.PC) //If the unit occupying this tile is a PC
            {
                if (OccupiedUnit == CombatUnitManager.Instance.SelectedPC) //If you select the same unit again
                {
                    //CombatMenuManager.Instance.ShowPCTurnMenu(); //Show the PC turn menu
                    CombatMenuManager.Instance.OpenRootMenu();
                }
                else
                {
                    CombatUnitManager.Instance.SetSelectedPC((BasePC)OccupiedUnit); //Select the newly clicked PC
                }
            }
            else //Means you must be selecting a monster
            {
                if (CombatUnitManager.Instance.SelectedPC != null) //If you have a PC already selected
                {
                    var monster = (BaseMonster)OccupiedUnit;
                    // Put in what happens when a selected PC clicks on a monster
                    Destroy(monster.gameObject); //In this case, it dies in one hit
                    CombatUnitManager.Instance.SetSelectedPC(null); //Deselect the current unit
                }
            }
        }
        else //If you click on an empty tile
        {
            if (CombatUnitManager.Instance.SelectedPC != null && isWalkable) //If you have a PC already selected, and the tile is walkable
            {
                bool inRange = CheckWithinUnitSpeed(CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this), CombatUnitManager.Instance.SelectedPC);
                if (inRange)
                {
                    MoveUnit(CombatUnitManager.Instance.SelectedPC);
                }
                else
                {
                    Log("Tile out of range");
                }
            }
        }
    }

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
            "SELECT remaining_speed FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", movingUnit.UnitID)
        )) / 5;

        int speedInTiles = (int)Math.Floor(unitSpeed);

        return numberOfTiles <= speedInTiles;
    }

    private void MoveUnit(BaseUnit movingUnit)
    {
        int unitSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT remaining_speed FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", movingUnit.UnitID)
        ));

        //If tile is difficult terrain, multiply by 10 instead
        int amountMoved = CheckDistance(CombatUnitManager.Instance.SelectedPC.occupiedTile, this) * 5;

        int newSpeed = unitSpeed - amountMoved;

        SetUnit(movingUnit); //Set this tile's unit as the selected unit

        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE saved_pcs SET remaining_speed = (@newSpeed) WHERE id = @unitID",
            ("@newSpeed", newSpeed),
            ("@unitID", movingUnit.UnitID)
        );

        Log("Unit has "+newSpeed+" feet of movement left.");
        
        CombatUnitManager.Instance.SetSelectedPC(null); //And deselect the PC
    }
}

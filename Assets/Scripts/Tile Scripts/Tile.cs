using UnityEngine;
using static UnityEngine.Debug;

public abstract class Tile : MonoBehaviour
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

    void OnMouseEnter()
    {
        highlight.SetActive(true);
        CombatMenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        highlight.SetActive(false);
        CombatMenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown()
    {
        if (CombatStateManager.Instance.GameState != GameState.PlayerTurn) return; //If it's not your turn, you can't click

        if (OccupiedUnit != null) //If you click on a tile that's not empty
        {
            if (OccupiedUnit.Faction == Faction.PC) //If the unit occupying this tile is a PC
            {
                if (OccupiedUnit == CombatUnitManager.Instance.SelectedPC) //If you select the same unit again
                {
                    CombatMenuManager.Instance.ShowEndTurnMenu(); //Show the end turn menu
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
                SetUnit(CombatUnitManager.Instance.SelectedPC); //Set this tile's unit as the selected unit
                CombatUnitManager.Instance.SetSelectedPC(null); //And deselect the PC
            }
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        int a;
        if (unit.occupiedTile != null)
        {
            // Go to unit's occupied tile, and set it's occupied unit to null (for when this unit is moving from a previous tile to this one)
            int oldX = unit.occupiedTile.tileX;
            int oldY = unit.occupiedTile.tileY;

            a = DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE grid_default_contents SET content = NULL WHERE encounter_id = @encounterID AND x = @x AND y = @y",
                ("@encounterID", tileEncounter),
                ("@x", oldX),
                ("@y", oldY)
            );
            unit.occupiedTile.OccupiedUnit = null;
        }
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.occupiedTile = this;

        int result = DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE grid_default_contents SET content = (@unitID) WHERE encounter_id = @encounterID AND x = @x AND y = @y",
            ("@unitID", unit.UnitID),
            ("@encounterID", tileEncounter),
            ("@x", tileX),
            ("@y", tileY)
        );
    }


}

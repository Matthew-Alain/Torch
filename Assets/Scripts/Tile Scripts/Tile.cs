using UnityEngine;

public abstract class Tile : MonoBehaviour
{

    [SerializeField] protected SpriteRenderer rend;     //Derived tiles can now access
    [SerializeField] private GameObject highlight;
    [SerializeField] private bool isWalkable;
    public string TileName;

    public BaseUnit OccupiedUnit;
    public bool Walkable => isWalkable && OccupiedUnit == null;

    //This logic runs on all tiles, but each tile has the chance to override it
    public virtual void Init(int x, int y)
    {

    }

    void OnMouseEnter()
    {
        highlight.SetActive(true);
        MenuManager.Instance.ShowTileInfo(this);
    }

    void OnMouseExit()
    {
        highlight.SetActive(false);
        MenuManager.Instance.ShowTileInfo(null);
    }

    void OnMouseDown()
    {
        if (GameManager.Instance.GameState != GameState.PlayerTurn) return; //If it's not your turn, you can't click

        if (OccupiedUnit != null) //If you click on a tile that's not empty
        {
            if (OccupiedUnit.Faction == Faction.PC) //If the unit occupying this tile is a PC
            {
                if (OccupiedUnit == UnitManager.Instance.SelectedPC)
                {
                    MenuManager.Instance.ShowEndTurnMenu();
                }
                else
                {
                    UnitManager.Instance.SetSelectedPC((BasePC)OccupiedUnit); //Select that PC
                }
            }
            else //Means you must be selecting a monster
            {
                if (UnitManager.Instance.SelectedPC != null) //If you have a PC already selected
                {
                    var monster = (BaseMonster)OccupiedUnit;
                    // Put in what happens when a selected PC clicks on a monster
                    Destroy(monster.gameObject);
                    UnitManager.Instance.SetSelectedPC(null);
                }
            }
        }
        else //If you click on an empty tile
        {
            if (UnitManager.Instance.SelectedPC != null) //If you have a PC already selected 
            {
                SetUnit(UnitManager.Instance.SelectedPC); //Set this tile's unit as the selected unit
                UnitManager.Instance.SetSelectedPC(null); //And deselect the PC
            }
        }
    }

    public void SetUnit(BaseUnit unit)
    {
        if (unit.occupiedTile != null)
        {
            // Go to unit's occupied tile, and set it's occupied unit to null (for when this unit is moving from a previous tile to this one)
            unit.occupiedTile.OccupiedUnit = null;
        }
        unit.transform.position = transform.position;
        OccupiedUnit = unit;
        unit.occupiedTile = this;
    }


}

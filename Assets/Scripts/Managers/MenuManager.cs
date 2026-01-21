using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    [SerializeField] private GameObject selectedPCObject, tileObject, tileUnitObject;
    [SerializeField] public GameObject endTurnMenu;

    void Awake()
    {
        Instance = this;
    }

    public void ShowSelectedPC(BasePC pc)
    {
        if (pc == null)
        {
            selectedPCObject.SetActive(false);
            return;
        }

        selectedPCObject.GetComponentInChildren<TMP_Text>().text = pc.UnitName;
        selectedPCObject.SetActive(true);
    }

    public void ShowTileInfo(Tile tile)
    {

        if (tile == null)
        {
            tileObject.SetActive(false);
            tileUnitObject.SetActive(false);
            return;
        }

        tileObject.GetComponentInChildren<TMP_Text>().text = tile.TileName;
        tileObject.SetActive(true);

        if (tile.OccupiedUnit) //If the tile has a unit on it
        {
            tileUnitObject.GetComponentInChildren<TMP_Text>().text = tile.OccupiedUnit.UnitName;
            tileUnitObject.SetActive(true);

        }
    }

    public void ShowEndTurnMenu()
    {
        endTurnMenu.SetActive(true);
    }
}

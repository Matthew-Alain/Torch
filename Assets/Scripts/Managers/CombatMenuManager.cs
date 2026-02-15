using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuManager : MonoBehaviour
{
    public static CombatMenuManager Instance;
    [SerializeField] private GameObject selectedPCObject, tileObject, tileUnitObject;
    [SerializeField] public GameObject endTurnMenu;

    void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //Now safe to create a new instance
        Instance = this;    
        DontDestroyOnLoad(gameObject);
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
        int result = DatabaseManager.Instance.ExecuteNonQuery(
            "INSERT INTO damage_types (name) VALUES (@name)",
            ("@name", "New type")
        );
        Debug.Log("Rows inserted: " + result);
    }
}

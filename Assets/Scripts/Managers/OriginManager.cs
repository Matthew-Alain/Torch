using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OriginManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown species, originFeat;
    public Button btnBack;

    //Current PC information
    public BasePC currentPC;

    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        species.SetValueWithoutNotify(currentPC.GetSpecies());
        originFeat.SetValueWithoutNotify(currentPC.GetOriginFeat());

        btnBack.onClick.AddListener(SaveCharacter);
    }
    
    void SaveCharacter()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET species = {species.value}, origin_feat = {originFeat.value} WHERE id = {currentPC.UnitID}");
    }
}

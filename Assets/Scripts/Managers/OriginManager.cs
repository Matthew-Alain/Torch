using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;

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
        GetCharacterSpecies(); //Populate the dropdown list
        GetOriginFeat();

        species.value = currentPC.GetSpecies(); //Populate the dropdown list
        originFeat.value = currentPC.GetOriginFeat();

        btnBack.onClick.AddListener(SaveCharacter);
    }
    
    void GetCharacterSpecies()
    {
        int savedSpecies = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT species FROM saved_pcs WHERE id = {currentPC.UnitID}"));

        species.value = savedSpecies;
    }
    
    void GetOriginFeat()
    {

        int savedOriginFeat = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT origin_feat FROM saved_pcs WHERE id = {currentPC.UnitID}"));

        originFeat.value = savedOriginFeat;
    }

    void SaveCharacter()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET species = {species.value}, origin_feat = {originFeat.value} WHERE id = {currentPC.UnitID}");
    }
}

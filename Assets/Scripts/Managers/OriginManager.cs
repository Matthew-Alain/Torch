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
    public int PCID;

    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(species == null)
        {
            Debug.Log("No species dropdown assigned");
            return;
        }
        GetCharacterSpecies(); //Populate the dropdown list

        if (originFeat == null)
        {
            Debug.Log("No originFeat dropdown assigned");
            return;
        }
        GetOriginFeat();

        
        btnBack.onClick.AddListener(SaveCharacter);
    }

    void GetCharacterSpecies()
    {
        int savedSpecies = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT species FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));

        species.value = savedSpecies;
    }
    
    void GetOriginFeat()
    {

        int savedOriginFeat = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT origin_feat FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));

        originFeat.value = savedOriginFeat;
    }

    void SaveCharacter()
    {
        int rowsAffected = DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE saved_pcs SET species = @species, origin_feat = @origin_feat WHERE id = @id",
            ("@species", species.value),
            ("@origin_feat", originFeat.value),
            ("@id", PCID)
        );

        // Debug.Log("Rows updated: " + rowsAffected);
    }
}

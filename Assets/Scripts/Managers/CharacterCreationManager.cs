using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;
using System.Linq;

public class CharacterCreationManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_InputField characterName;
    public Button btnSaveCharacter;

    //Current PC information
    public int PCID;

    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (characterName == null)
        {
            Debug.Log("No character name assigned");
            return;
        }
        GetCharacterName(); //Populate the character name

        btnSaveCharacter.onClick.AddListener(SaveCharacter);
    }

    void GetCharacterName()
    {
        string savedName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's name
            $"SELECT name FROM saved_pcs WHERE id = {PCID}"
        ));
        characterName.text = savedName;
        
    }

    void SaveCharacter()
    {
        int rowsAffected = DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE saved_pcs SET name = \"{characterName.text}\" WHERE id = {PCID}"
        );

        // Debug.Log("Rows updated: " + rowsAffected);
    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Debug = UnityEngine.Debug;

public class CharacterCreationManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_InputField characterName;
    public Button btnSaveCharacter;

    //Current PC information
    public BasePC currentPC;

    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (characterName == null)
        {
            Debug.LogError("No character name assigned");
            return;
        }
        characterName.SetTextWithoutNotify(GetCharacterName()); //Populate the character name

        btnSaveCharacter.onClick.AddListener(SaveCharacter);
    }
    
    string GetCharacterName()
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's name
            $"SELECT name FROM unit_info WHERE id = {currentPC.UnitID}"
        ));        
    }

    public void SaveCharacter()
    {
        if(!string.IsNullOrWhiteSpace(characterName.text))
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET name = \"{characterName.text}\" WHERE id = {currentPC.UnitID}");
        }
    }

}

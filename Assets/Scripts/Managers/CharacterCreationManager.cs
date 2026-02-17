using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;

public class CharacterCreationManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown species, dndclass, subclass;
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

        if(species == null)
        {
            Debug.Log("No species dropdown assigned");
            return;
        }
        GetCharacterSpecies(); //Populate the dropdown list

        if (dndclass == null)
        {
            Debug.Log("No dndclass dropdown assigned");
            return;
        }
        GetCharacterClass();

        if(subclass == null)
        {
            Debug.Log("No subclass dropdown assigned");
            return;
        }
        GetCharacterSubclass();

        dndclass.onValueChanged.AddListener(OnClassChanged);
        btnSaveCharacter.onClick.AddListener(SaveCharacter);
    }

    void GetCharacterName()
    {
        string savedName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's name
            "SELECT name FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));
        characterName.text = savedName;
    }

    void GetCharacterSpecies()
    {
        int savedSpecies = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT species FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));

        species.value = savedSpecies;
    }
    
    void GetCharacterClass()
    {

        int savedClass = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT dndclass FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));

        dndclass.value = savedClass;
    }

    void GetCharacterSubclass()
    {
        int savedSubclass = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's previous subclass
            "SELECT subclass FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));

        subclass.ClearOptions(); //Clear the old options
        List<string> subclassList = new List<string>(); //Create list to contain subclass names

        DatabaseManager.Instance.ExecuteReader(
            "SELECT name FROM subclasses WHERE dndclass = @classId", //Get all subclass names that belong to the current class id
            reader =>
            {
                while (reader.Read())
                {
                    subclassList.Add(reader["name"] as string);
                }
            },
            ("@classId", dndclass.value)
        );

        subclass.AddOptions(subclassList); //Add the list of names to the dropdown

        subclass.value = savedSubclass; //Re-select the previous option in the dropdown
    }

    void OnClassChanged(int index)
    {
        GetCharacterSubclass();
        subclass.value = 0; //If the class has changed, select the first subclass by default
    }

    void SaveCharacter()
    {
        int rowsAffected = DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE saved_pcs SET name = @name, species = @species, dndclass = @dndclass, subclass = @subclass WHERE id = @id",
            ("@name", characterName.text),
            ("@species", species.value),
            ("@dndclass", dndclass.value),
            ("@subclass", subclass.value),
            ("@id", PCID)
        );

        // Debug.Log("Rows updated: " + rowsAffected);
    }
}

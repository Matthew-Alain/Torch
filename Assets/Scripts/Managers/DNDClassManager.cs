using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;

public class DNDClassManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown dndClass1, dndClass2, dndClass3, dndClass4, dndClass5, subclass, level;
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
        if (level == null)
        {
            Debug.Log("No level dropdown assigned");
            return;
        }
        GetCharacterLevel();

        if (dndClass1 == null)
        {
            Debug.Log("No dndClass1 dropdown assigned");
            return;
        }
        if (dndClass2 == null)
        {
            Debug.Log("No dndClass2 dropdown assigned");
            return;
        }
        if (dndClass3 == null)
        {
            Debug.Log("No dndClass3 dropdown assigned");
            return;
        }
        if (dndClass4 == null)
        {
            Debug.Log("No dndClass4 dropdown assigned");
            return;
        }
        if (dndClass5 == null)
        {
            Debug.Log("No dndClass5 dropdown assigned");
            return;
        }
        GetCharacterClasses();

        if (subclass == null)
        {
            Debug.Log("No subclass dropdown assigned");
            return;
        }
        GetCharacterSubclass();



        dndClass1.onValueChanged.AddListener(OnClassChanged);
        level.onValueChanged.AddListener(OnLevelChanged);
        btnBack.onClick.AddListener(SaveCharacter);
    }
    
    void GetCharacterLevel()
    {
        int savedLevel = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar( //Get the character's level
            "SELECT level FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));
        level.value = savedLevel - 1;
    }

    void OnLevelChanged(int index)
    {
        // For each level row above the character's current level:
        // - Hide class that level's text label and dropdown menu
        // - Update the saved_pcs column for that level to NULL (may not be possible, need to add a variable, then instead of updating DB with the dropdown value, update it with the respective variables?)
        // If there is no class listed at least three times, hide subclass
    }
    
    void GetCharacterClasses()
    {

        int savedClass1 = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT dnd_class_1 FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        ));
        dndClass1.value = savedClass1;

        if (level.value+1 >= 2)
        {
            int savedClass2 = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                "SELECT dnd_class_2 FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", PCID)
            ));
            dndClass2.value = savedClass2;
        }

        if (level.value+1 >= 3)
        {
            int savedClass3 = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                "SELECT dnd_class_3 FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", PCID)
            ));
            dndClass3.value = savedClass3;
        }

        if (level.value+1 >= 4)
        {
            int savedClass4 = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                "SELECT dnd_class_4 FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", PCID)
            ));
            dndClass4.value = savedClass4;
        }

        if (level.value+1 >= 5)
        {
            int savedClass5 = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                "SELECT dnd_class_5 FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", PCID)
            ));
            dndClass5.value = savedClass5;
        }
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
            "SELECT name FROM subclasses WHERE dndclass = @dndClass1", //Get all subclass names that belong to the current class id
            reader =>
            {
                while (reader.Read())
                {
                    subclassList.Add(reader["name"] as string);
                }
            },
            ("@dndClass1", dndClass1.value)
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
            "UPDATE saved_pcs SET dnd_class_1 = @dndClass1, dnd_class_2 = @dndClass2, dnd_class_3 = @dndClass3, dnd_class_4 = @dndClass4, dnd_class_5 = @dndClass5, "
            + "subclass = @subclass, level = @level WHERE id = @id",
            ("@dndClass1", dndClass1.value),
            ("@dndClass2", dndClass2.value),
            ("@dndClass3", dndClass3.value),
            ("@dndClass4", dndClass4.value),
            ("@dndClass5", dndClass5.value),
            ("@subclass", subclass.value),
            ("@level", level.value+1),
            ("@id", PCID)
        );

        // Debug.Log("Rows updated: " + rowsAffected);
    }
}

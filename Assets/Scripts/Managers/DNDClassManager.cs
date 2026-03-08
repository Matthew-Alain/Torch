using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;
using Unity.VisualScripting;

public class DNDClassManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown dndClass1, dndClass2, dndClass3, dndClass4, dndClass5, subclass, level;
    public Button btnBack;

    //Current PC information
    public int PCID;
    private int classWithSubclass = -1;
    int lastClassWithSubclass;
    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dndClass1.onValueChanged.AddListener(OnClassChanged);
        dndClass2.onValueChanged.AddListener(OnClassChanged);
        dndClass3.onValueChanged.AddListener(OnClassChanged);
        dndClass4.onValueChanged.AddListener(OnClassChanged);
        dndClass5.onValueChanged.AddListener(OnClassChanged);
        level.onValueChanged.AddListener(OnLevelChanged);
        btnBack.onClick.AddListener(SaveCharacter);

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
        CheckForSubclass();

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
        if (level.value < 1)
        {
            dndClass2.gameObject.SetActive(false);
        }
        else
        {
            dndClass2.gameObject.SetActive(true);
        }

        if (level.value < 2)
        {
            dndClass3.gameObject.SetActive(false);
        }
        else
        {
            dndClass3.gameObject.SetActive(true);
        }

        if (level.value < 3)
        {
            dndClass4.gameObject.SetActive(false);
        }
        else
        {
            dndClass4.gameObject.SetActive(true);
        }

        if (level.value < 4)
        {
            dndClass5.gameObject.SetActive(false);
        }
        else
        {
            dndClass5.gameObject.SetActive(true);
        }

        CheckForSubclass();
        
        // For each level row above the character's current level:
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

    void UpdateSubclassList()
    {
        if (classWithSubclass == -1) //There is no class that could have a subclass
        {
            Debug.Log("No valid class");
            return;
        }

        var result = DatabaseManager.Instance.ExecuteScalar( //Check if the character has a saved subclass
            "SELECT subclass FROM saved_pcs WHERE id = (@PCID)",
            ("@PCID", PCID)
        );
        int currentSelection;

        if (!(classWithSubclass == lastClassWithSubclass)) //This covers changing between two classes
        {
            Debug.Log("The valid class has changed. Set to first option");
            currentSelection = 0;
        }
        else if (result == DBNull.Value) //This covers adding/changing levels without changing the class with a subclass
        {
            Debug.Log("The valid class has not changed, and it does not have a saved subclass. Keep the current subclass");
            currentSelection = subclass.value;
        }

        // TODO: Fix this issue: right now, if there is a saved subclass, and the list is updated without changing the valid class, it resets to the saved subclass
        // Want to change it to always either set currentSelection to 0 if it's changing valid class, or keep the same value if its not
        // Issue is that when the scene is first loaded, if there is a saved subclass, it needs to be set instead

        // else if ()
        // {
        //     Debug.Log("Class with a subclass has not changed, and is the same ");
        //     currentSelection = subclass.value;
        // }
        else //This covers loading the default option
        {
            Debug.Log("The class with a subclass has not changed, and it has a saved subclass. Set to saved option");
            currentSelection = Convert.ToInt32(result);
        }

        subclass.ClearOptions(); //Clear the old options
        List<string> subclassList = new List<string>(); //Create list to contain subclass names

        DatabaseManager.Instance.ExecuteReader(
            "SELECT name FROM subclasses WHERE dndclass = @classWithSubclass", //Get all subclass names that belong to the current class id
            reader =>
            {
                while (reader.Read())
                {
                    subclassList.Add(reader["name"] as string);
                }
            },
            ("@classWithSubclass", classWithSubclass)
        );

        subclass.AddOptions(subclassList); //Add the list of names to the dropdown

        subclass.value = currentSelection; //Re-select the previous option in the dropdown
    }

    void OnClassChanged(int index)
    {
        CheckForSubclass();
    }

    void SaveCharacter()
    {
        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE saved_pcs SET dnd_class_1 = @dndClass1, dnd_class_2 = @dndClass2, dnd_class_3 = @dndClass3, dnd_class_4 = @dndClass4, dnd_class_5 = @dndClass5, "
            + "subclass = @subclass, level = @level WHERE id = @id",
            ("@dndClass1", dndClass1.value),
            ("@dndClass2", dndClass2.value),
            ("@dndClass3", dndClass3.value),
            ("@dndClass4", dndClass4.value),
            ("@dndClass5", dndClass5.value),
            ("@subclass", subclass.value),
            ("@level", level.value + 1),
            ("@id", PCID)
        );

        if (level.value < 1)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE saved_pcs SET dnd_class_2 = null WHERE id = @id",
                ("@id", PCID)
            );
        }

        if (level.value < 2)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE saved_pcs SET dnd_class_3 = null WHERE id = @id",
                ("@id", PCID)
            );
        }

        if (level.value < 3)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE saved_pcs SET dnd_class_4 = null WHERE id = @id",
                ("@id", PCID)
            );
        }

        if (level.value < 4)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE saved_pcs SET dnd_class_5 = null WHERE id = @id",
                ("@id", PCID)
            );
        }

        if(classWithSubclass == -1)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE saved_pcs SET subclass = null WHERE id = @id",
                ("@id", PCID)
            );
        }
    }

    void CheckForSubclass()
    {
        if (level.value < 2) //If they are not at least level 3
        {
            Skip();
        }
        else
        {
            subclass.gameObject.SetActive(true);
        }

        lastClassWithSubclass = classWithSubclass;

        List<int> classIDs = new List<int>();
        if (level.value >=0) classIDs.Add(dndClass1.value);
        if (level.value >=1) classIDs.Add(dndClass2.value);
        if (level.value >=2) classIDs.Add(dndClass3.value);
        if (level.value >=3) classIDs.Add(dndClass4.value);
        if (level.value >=4) classIDs.Add(dndClass5.value);



        for (int i = 0; i < classIDs.Count; i++)
        {
            // Debug.Log("checking: "+classIDs[i]);
            int count = 1;
            for (int j = i + 1; j < classIDs.Count; j++)
            {
                // Debug.Log("comparing "+classIDs[i]+" with "+classIDs[j]);

                if (classIDs[i] == classIDs[j])
                {
                    count++;
                    // Debug.Log("Match: " + count+"/3");
                    if (count >= 3)
                    {
                        classWithSubclass = classIDs[i];
                        UpdateSubclassList();
                        return;
                    }
                }
                else
                {
                    // Debug.Log("No match");
                }
            }
        }
        Skip();

        void Skip()
        {
            classWithSubclass = -1; //They cannot have a subclass
            UpdateSubclassList();
            subclass.gameObject.SetActive(false); //They cannot select a subclass
            return;
        }
    } 

}

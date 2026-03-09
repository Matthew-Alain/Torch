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
    private int classWithSubclass = -1;
    int lastClassWithSubclass;
    bool firstLoad = true;
    int lastSubclass;
    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetDefaultInfo();
        level.onValueChanged.AddListener(OnLevelChanged);
        dndClass1.onValueChanged.AddListener(OnClassChanged);
        dndClass2.onValueChanged.AddListener(OnClassChanged);
        dndClass3.onValueChanged.AddListener(OnClassChanged);
        dndClass4.onValueChanged.AddListener(OnClassChanged);
        dndClass5.onValueChanged.AddListener(OnClassChanged);
        btnBack.onClick.AddListener(SaveCharacter);
        OnLevelChanged(0);
    }

    void GetDefaultInfo()
    {
        DatabaseManager.Instance.ExecuteReader(
            "SELECT level, dnd_class_1, dnd_class_2, dnd_class_3, dnd_class_4, dnd_class_5, subclass FROM saved_pcs WHERE id = (@PCID)",
            reader =>
            {
                while (reader.Read())
                {
                    level.value = Convert.ToInt32(reader["level"]) - 1;

                    if (level.value >= 0) { dndClass1.value = Convert.ToInt32(reader["dnd_class_1"]); }
                    if (level.value >= 1) { dndClass2.value = Convert.ToInt32(reader["dnd_class_2"]); }
                    if (level.value >= 2) { dndClass3.value = Convert.ToInt32(reader["dnd_class_3"]); }
                    if (level.value >= 3) { dndClass4.value = Convert.ToInt32(reader["dnd_class_4"]); }
                    if (level.value >= 4) { dndClass5.value = Convert.ToInt32(reader["dnd_class_5"]); }

                    var savedSubclass = reader["subclass"];
                    if (!(savedSubclass == DBNull.Value))
                    {
                        lastSubclass = Convert.ToInt32(reader["subclass"]);
                    }
                }
            },
            ("@PCID", PCID)
        );

    }

    void OnLevelChanged(int index)
    {
        if (level.value < 1) { dndClass2.gameObject.SetActive(false); }
        else { dndClass2.gameObject.SetActive(true); }

        if (level.value < 2) { dndClass3.gameObject.SetActive(false); }
        else { dndClass3.gameObject.SetActive(true); }

        if (level.value < 3) { dndClass4.gameObject.SetActive(false); }
        else { dndClass4.gameObject.SetActive(true); }

        if (level.value < 4) { dndClass5.gameObject.SetActive(false); }
        else { dndClass5.gameObject.SetActive(true); }

        CheckForSubclass();
    }

    void UpdateSubclassList()
    {
        if (classWithSubclass == -1) //There is no class that could have a subclass
        {
            // Debug.Log("No valid class");
            return;
        }

        int currentSelection;

        if (firstLoad)
        {
            // Debug.Log("First load, subclass defaults to: " + lastSubclass);
            currentSelection = lastSubclass;
            firstLoad = false;
        }
        else if (!(classWithSubclass == lastClassWithSubclass)) //This covers changing between two classes
        {
            // Debug.Log("Class changed. Set to first option");
            currentSelection = 0;
        }
        else
        {
            // Debug.Log("Class not changed. Keep the same option");
            currentSelection = subclass.value;
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
                        // Debug.Log("Class with subclass found: "+classIDs[i]);
                        classWithSubclass = classIDs[i];
                        UpdateSubclassList();
                        return;
                    }
                }
            }
        }
        Skip();

        void Skip()
        {
            // Debug.Log("Checking for subclass: skipping");
            classWithSubclass = -1; //They cannot have a subclass
            UpdateSubclassList();
            subclass.gameObject.SetActive(false); //They cannot select a subclass
            return;
        }
    } 

}

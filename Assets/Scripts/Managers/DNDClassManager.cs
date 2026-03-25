using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;

public class DNDClassManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown dndClass1, dndClass2, dndClass3, dndClass4, dndClass5, subclass, level;
    public GameObject row1, row2, row3, row4, row5;
    public Button btnBack;

    //Class Feature Buttons
    public Button lvl1feat1, lvl1feat2, lvl1feat3, lvl1feat4;
    public Button lvl2feat1, lvl2feat2, lvl2feat3, lvl2feat4;
    public Button lvl3feat1, lvl3feat2, lvl3feat3, lvl3feat4;
    public Button lvl4feat1, lvl4feat2, lvl4feat3, lvl4feat4;
    public Button lvl5feat1, lvl5feat2, lvl5feat3, lvl5feat4;
    private List<Button> buttonList = new List<Button>();

    //For the feature panel
    public TMP_Text featureName, featureText;
    public Button btnClose, btnNextScene;
    public GameObject featurePanel;

    //Current PC information
    private BasePC currentPC;
    private int classWithSubclass = -1;
    int lastClassWithSubclass;
    List<int> classIDs = new List<int>();
    List<int> classLevels = new List<int>();

    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
        buttonList.AddRange(row1.GetComponentsInChildren<Button>());
        buttonList.AddRange(row2.GetComponentsInChildren<Button>());
        buttonList.AddRange(row3.GetComponentsInChildren<Button>());
        buttonList.AddRange(row4.GetComponentsInChildren<Button>());
        buttonList.AddRange(row5.GetComponentsInChildren<Button>());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetDefaultInfo();
        level.onValueChanged.AddListener(RefreshData);
        dndClass1.onValueChanged.AddListener(RefreshData);
        dndClass2.onValueChanged.AddListener(RefreshData);
        dndClass3.onValueChanged.AddListener(RefreshData);
        dndClass4.onValueChanged.AddListener(RefreshData);
        dndClass5.onValueChanged.AddListener(RefreshData);
        subclass.onValueChanged.AddListener(RefreshData);
        btnBack.onClick.AddListener(SaveCharacter);
        RefreshData(0);

        btnClose.onClick.AddListener(CloseFeatureWindow);
    }

    void GetDefaultInfo()
    {
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT level, dnd_class_1, dnd_class_2, dnd_class_3, dnd_class_4, dnd_class_5, subclass FROM saved_pcs WHERE id = {currentPC.UnitID}",
            reader =>
            {
                while (reader.Read())
                {
                    level.value = Convert.ToInt32(reader["level"]) - 1;

                    if (level.value >= 0) { dndClass1.SetValueWithoutNotify(Convert.ToInt32(reader["dnd_class_1"])); }
                    if (level.value >= 1) { dndClass2.SetValueWithoutNotify(Convert.ToInt32(reader["dnd_class_2"])); }
                    if (level.value >= 2) { dndClass3.SetValueWithoutNotify(Convert.ToInt32(reader["dnd_class_3"])); }
                    if (level.value >= 3) { dndClass4.SetValueWithoutNotify(Convert.ToInt32(reader["dnd_class_4"])); }
                    if (level.value >= 4) { dndClass5.SetValueWithoutNotify(Convert.ToInt32(reader["dnd_class_5"])); }

                    if (reader["subclass"] != DBNull.Value)
                    {
                        UpdateClassList();
                        UpdateClassLevels();
                        CheckForSubclass();
                        UpdateSubclassList();
                        UpdateClassFeatures();
                        // Debug.Log($"subclass value should be {Convert.ToInt32(reader["subclass"])}");
                        subclass.SetValueWithoutNotify(Convert.ToInt32(reader["subclass"]));
                        // Debug.Log($"subclass.value is {subclass.value}");
                    }
                }
            }
        );
    }

    void RefreshData(int index)
    {
        UpdateClassList();
        UpdateClassLevels();
        CheckForSubclass();
        UpdateSubclassList();
        UpdateClassFeatures();
    }

    void UpdateClassList()
    {
        // Debug.LogWarning($"Subclass is {subclass.value} at beginning of UpdateClassList");
        classIDs.Clear();

        if (level.value >= 0)
        {
            classIDs.Add(dndClass1.value);
            row1.SetActive(true);
        }
        else{ row1.SetActive(false); }

        if (level.value >= 1)
        {
            classIDs.Add(dndClass2.value);
            row2.SetActive(true);
        }
        else { row2.SetActive(false); }

        if (level.value >= 2)
        {
            classIDs.Add(dndClass3.value);
            row3.SetActive(true);
        }
        else { row3.SetActive(false); }

        if (level.value >= 3)
        {
            classIDs.Add(dndClass4.value);
            row4.SetActive(true);
        }
        else { row4.SetActive(false); }

        if (level.value >= 4)
        {
            classIDs.Add(dndClass5.value);
            row5.SetActive(true);
        }
        else { row5.SetActive(false); } 
        // Debug.LogWarning($"Subclass is {subclass.value} at beginning of UpdateClassList");

    }
    
    void UpdateClassLevels()
    {
        // Debug.LogWarning($"Subclass is {subclass.value} at beginning of UpdateClassLevels");

        classLevels.Clear();
        for (int i = 0; i < classIDs.Count; i++)
        {
            int count = 1; //The current level is count 1
            if (!(i == 0)) //If it's the first class the list, don't check the previous classes 
            {
                for (int j = i - 1; j >= 0; j--) //Starting at the previous class in the list, then going backwards until there are no classes left
                {
                    if (classIDs[i] == classIDs[j]) //If the current class matches the one being checked
                    {
                        count++; //Then this is one more time that class has existed
                    }
                }
            }
            classLevels.Add(count); //Add the count to the list of levels
        }
        // Now, each element in the classIDs list has what level of features it should display in the classLevelUnique list
        // Debug.LogWarning($"Subclass is {subclass.value} at end of UpdateClassLevels");
    }

    void CheckForSubclass()
    {
        // Debug.LogWarning($"Subclass is {subclass.value} at beginning of CheckForSubclass");

        subclass.gameObject.SetActive(false); //They cannot select a subclass

        if (classIDs.Count >= 3) //If there are at least three class levels
        {
            for (int i = 0; i < classLevels.Count; i++)
            {
                if (classLevels[i] >= 3)
                {
                    // Debug.Log($"Class {classIDs[i]} is level {classLevels[i]}");
                    subclass.gameObject.SetActive(true);
                    lastClassWithSubclass = classWithSubclass;
                    // Debug.Log("lastClassWithSubclass set to " + classWithSubclass);
                    classWithSubclass = classIDs[i];
                    // Debug.Log("classWithSubclass set to " + classIDs[i]);
                }
            }
        }

        if (subclass.gameObject.activeSelf == false)
        {
            classWithSubclass = -1; //They do not have a subclass
            // Debug.Log("classWithSubclass set to -1");

        }
        // Debug.LogWarning($"Subclass is {subclass.value} at end of CheckForSubclass");
    }
    
    void UpdateSubclassList()
    {
        if (classWithSubclass == -1) return;

        List<string> subclassList = new List<string>();
        DatabaseManager.Instance.ExecuteReader($"SELECT name FROM subclasses WHERE dndclass = {classWithSubclass}",
            reader =>
            {
                while (reader.Read())
                    subclassList.Add(Convert.ToString(reader["name"]));
            }
        );

        // Only update dropdown if the options actually changed
        bool changed = false;
        if (subclass.options.Count != subclassList.Count)
            changed = true;
        else
        {
            for (int i = 0; i < subclassList.Count; i++)
            {
                if (subclass.options[i].text != subclassList[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            subclass.ClearOptions();
            subclass.AddOptions(subclassList);
        }

        // Preserve selection
        if (classWithSubclass != lastClassWithSubclass)
        {
            subclass.value = 0;
        }
        // Debug.LogWarning($"Subclass is {subclass.value} at end of UpdateSubclassList");
    }

    void UpdateClassFeatures()
    {
        // Debug.LogWarning($"Subclass is {subclass.value} at beginning of UpdateClassFeatures");
        for (int row = 0; row < classIDs.Count; row++)
        {
            int rowClass = classIDs[row];
            int classLevel = classLevels[row];

            int numberOfFeatures;
            List<string> featureNames = new List<string>();

            if (classWithSubclass == -1)
            {
                numberOfFeatures = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                    $"SELECT COUNT(*) FROM class_features WHERE class = {rowClass} AND level = {classLevel} AND subclass IS NULL"
                ));

                DatabaseManager.Instance.ExecuteReader(
                    $"SELECT name FROM class_features WHERE class = {rowClass} AND level = {classLevel} AND subclass IS NULL",
                    reader =>
                    {
                        while (reader.Read())
                        {
                            featureNames.Add(Convert.ToString(reader["name"]));
                        }
                    }
                );
            }
            else
            {
                numberOfFeatures = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                    $"SELECT COUNT(*) FROM class_features WHERE class = {rowClass} AND level = {classLevel} AND (subclass IS NULL OR subclass = {subclass.value})"
                ));

                DatabaseManager.Instance.ExecuteReader(
                    $"SELECT name FROM class_features WHERE class = {rowClass} AND level = {classLevel} AND (subclass IS NULL OR subclass = {subclass.value})",
                    reader =>
                    {
                        while (reader.Read())
                        {
                            featureNames.Add(Convert.ToString(reader["name"]));
                        }
                    }
                );
            }

            for (int column = 0; column < 4; column++)
            {
                int currentColumn = column;
                int currentButtonIndex = row * 4 + currentColumn;
                Button btn = buttonList[currentButtonIndex];
                btn.onClick.RemoveAllListeners();
                if (currentColumn <= numberOfFeatures - 1)
                {
                    btn.gameObject.SetActive(true);
                    btn.GetComponentInChildren<TMP_Text>().text = featureNames[currentColumn];
                    btn.onClick.AddListener(() => OpenFeatureWindow(rowClass, classLevel, currentColumn));
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }
        // Debug.LogWarning($"Subclass is {subclass.value} at end of UpdateClassFeatures");
    }
    
    public void OpenFeatureWindow(int rowClass, int rowLevel, int columnNumber)
    {
        featurePanel.SetActive(true);

        DatabaseManager.Instance.ExecuteReader(
            "SELECT name, description, scene_to_load FROM class_features " +
                $"WHERE class = {rowClass} AND level = {rowLevel} AND (subclass IS NULL OR subclass = {subclass.value})" +
                $"LIMIT 1 OFFSET {columnNumber}",
            reader =>
            {
                while (reader.Read())
                {
                    featureName.text = Convert.ToString(reader["name"]);

                    featureText.text = Convert.ToString(reader["description"]);
                    var sceneToLoad = reader["scene_to_load"];
                    if (!(sceneToLoad == DBNull.Value))
                    {
                        int featureScene = Convert.ToInt32(sceneToLoad);
                        btnNextScene.gameObject.SetActive(true);
                        btnNextScene.onClick.RemoveAllListeners();
                        btnNextScene.onClick.AddListener(() => SceneManager.LoadScene(featureScene));
                    }
                    else
                    {
                        btnNextScene.gameObject.SetActive(false);
                    }

                }
            }
        );
    }

    public void SaveCharacter()
    {
        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE saved_pcs SET dnd_class_1 = {dndClass1.value}, dnd_class_2 = {dndClass2.value}, dnd_class_3 = {dndClass3.value}, " +
            $"dnd_class_4 = {dndClass4.value}, dnd_class_5 = {dndClass5.value}, subclass = {subclass.value}, level = {level.value + 1} WHERE id = {currentPC.UnitID}"
        );

        if (level.value < 1)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET dnd_class_2 = null WHERE id = {currentPC.UnitID}");

        if (level.value < 2)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET dnd_class_3 = null WHERE id = {currentPC.UnitID}");

        if (level.value < 3)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET dnd_class_4 = null WHERE id = {currentPC.UnitID}");

        if (level.value < 4)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET dnd_class_5 = null WHERE id = {currentPC.UnitID}");

        if (level.value == 4)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_stats SET proficiency = 3 WHERE id = {currentPC.UnitID}");
        else
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_stats SET proficiency = 2 WHERE id = {currentPC.UnitID}");

        if (classWithSubclass == -1)
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET subclass = null WHERE id = {currentPC.UnitID}");

        UpdateProficiencies();
    }

    public void UpdateProficiencies()
    {
        UpdateWeaponProficiencies();
        UpdateArmorProficiencies();
        UpdateSaveProficiencies();
    }

    public void UpdateWeaponProficiencies()
    {
        // bool all_simple = true;
        bool martial_light = false;
        bool martial_finesse = false;
        bool all_martial = false;

        // Simple weapons
        // if (dndClass1.value == 1 || dndClass1.value == 2 || dndClass1.value == 3 || dndClass1.value == 9 || dndClass1.value == 10 || dndClass1.value == 11)
        // {
        //     simple_melee = true;
        //     simple_ranged = true;
        // }

        //Simple + Martial Light
        if (dndClass1.value == 5)
        {
            // simple_melee = true;
            // simple_ranged = true;
            martial_light = true;
        }

        //Simple + Martial finesse + Martial light
        if (dndClass1.value == 8)
        {
            // simple_melee = true;
            // simple_ranged = true;
            martial_light = true;
            martial_finesse = true;
        }

        //All weapons
        if (dndClass1.value == 0 || dndClass1.value == 4 || dndClass1.value == 6 || dndClass1.value == 7)
        {
            // simple_melee = true;
            // simple_ranged = true;
            martial_light = true;
            martial_finesse = true;
            all_martial = true;
        }

        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE pc_proficiencies SET " +
            // $"all_simple = {all_simple}, " +
            $"martial_light = {martial_light}, " +
            $"martial_finesse = {martial_finesse}, " +
            $"all_martial = {all_martial} " +
            $"WHERE id = {currentPC.UnitID}"
        );
    }

    public void UpdateArmorProficiencies()
    {
        bool light = false;
        bool medium = false;
        bool heavy = false;
        bool shields = false;

        // Light
        if (dndClass1.value == 0 ||
            dndClass1.value == 1 ||
            dndClass1.value == 2 ||
            dndClass1.value == 3 ||
            dndClass1.value == 4 ||
            dndClass1.value == 6 ||
            dndClass1.value == 7 ||
            dndClass1.value == 8 ||
            dndClass1.value == 10)
        {
            light = true;
        }

        // Medium
        if (dndClass1.value == 0 ||
            dndClass1.value == 2 ||
            dndClass1.value == 4 ||
            dndClass1.value == 6 ||
            dndClass1.value == 7)
        {
            medium = true;
        }

        // Heavy
        if (dndClass1.value == 0 ||
            dndClass1.value == 4 ||
            dndClass1.value == 6)
        {
            heavy = true;
        }

        // Shields
        if (dndClass1.value == 0 ||
            dndClass1.value == 2 ||
            dndClass1.value == 3 ||
            dndClass1.value == 4 ||
            dndClass1.value == 6 ||
            dndClass1.value == 7)
        {
            shields = true;
        }

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE pc_proficiencies SET light_armor = {light}, medium_armor = {medium}, heavy_armor = {heavy}, shields = {shields} WHERE id = {currentPC.UnitID}"
        );
    }
    
    private void UpdateSaveProficiencies()
    {
        bool str_save = false;
        bool dex_save = false;
        bool con_save = false;
        bool int_save = false;
        bool wis_save = false;
        bool cha_save = false;

        if (dndClass1.value == 0 || dndClass1.value == 4 || dndClass1.value == 5 || dndClass1.value == 7)
        {
            str_save = true;
        }

        if (dndClass1.value == 1 || dndClass1.value == 5 || dndClass1.value == 7 || dndClass1.value == 8)
        {
            dex_save = true;
        }

        if (dndClass1.value == 0 || dndClass1.value == 4 || dndClass1.value == 9)
        {
            con_save = true;
        }

        if (dndClass1.value == 3 || dndClass1.value == 8 || dndClass1.value == 11)
        {
            int_save = true;
        }

        if (dndClass1.value == 2 || dndClass1.value == 3 || dndClass1.value == 6 || dndClass1.value == 10 || dndClass1.value == 11)
        {
            wis_save = true;
        }

        if (dndClass1.value == 1 || dndClass1.value == 2 || dndClass1.value == 6 || dndClass1.value == 9 || dndClass1.value == 10)
        {
            cha_save = true;
        }

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE pc_proficiencies SET str_save = {str_save}, dex_save = {dex_save}, con_save = {con_save}, "+
            $"int_save = {int_save}, wis_save = {wis_save}, cha_save = {cha_save} WHERE id = {currentPC.UnitID}"
        );
    }

    private void CloseFeatureWindow()
    {
        btnNextScene.gameObject.SetActive(false);
        featurePanel.SetActive(false);
    }

}

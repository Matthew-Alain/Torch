using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NUnit.Framework.Internal;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpellSelectionManager : MonoBehaviour
{
    private BasePC currentPC;
    private int currentClass;
    private int currentSpell;
    private int numberOfPreparedCantrips = 0;
    private int numberOfPreparedSpells = 0;
    
    [SerializeField] private GameObject cantripWindows, level1Windows, level2Windows, level3Windows;
    [SerializeField] private Transform availableCantripsWindow, availableWindow1, availableWindow2, availableWindow3, preparedCantripsWindow, preparedWindow1, preparedWindow2, preparedWindow3;
    public List<(int, string)> spellList = new();
    private List<(int, GameObject)> spellListButtons = new();

    public GameObject spellPrefab;
    public GameObject spellPanel;
    [SerializeField] private Button btnPrepareSpell, btnUnprepareSpell, btnBackToClass, btnClosePanel;
    public TMP_Text txtCurrentClass, txtSpellName, txtSpellDescription, cantripCount, spellCount;

    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
        currentClass = DatabaseManager.Instance.spellListToEdit;
    }

    void Start()
    {
        txtCurrentClass.text = DatabaseManager.GetClassNameFromID(currentClass);
        btnPrepareSpell.onClick.AddListener(PrepareUnprepareCurrentSpell);
        btnUnprepareSpell.onClick.AddListener(PrepareUnprepareCurrentSpell);
        btnClosePanel.onClick.AddListener(HideSpellInfo);
        btnBackToClass.onClick.AddListener(SaveSpellList);
        GetSpellList();
        HideInvalidSpellLists();
        UpdateText();
    }

    private void GetSpellList()
    {
        // Debug.Log($"Getting spell list for {currentClass}");

        DatabaseManager.Instance.ExecuteReader($"SELECT * FROM spells WHERE {DatabaseManager.GetClassNameFromID(currentClass)} = 1",
            reader =>
            {
                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"]);
                    // Debug.Log($"Spell: {id}");

                    bool spellPrepared = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM pc_spells " +
                        $"WHERE unit_id = {currentPC.UnitID} AND spell_id = {id} AND source = '{DatabaseManager.GetClassNameFromID(currentClass)}'"));

                    if (spellPrepared && Convert.ToInt32(reader["level"]) <= ClassHighestSlot())
                    {                        
                        // Debug.Log($"Spell is already prepared");
                        AddSpell(id, "Prepared");

                        if (!SpellAlwaysPrepared(id))
                        {
                            if (Convert.ToInt32(reader["level"]) == 0)
                            {
                                numberOfPreparedCantrips += 1;
                            }
                            else
                            {
                                numberOfPreparedSpells += 1;
                            }
                        }
                    }
                    else
                    {
                        // Debug.Log($"Spell is NOT already prepared");
                        AddSpell(id, "Available");
                    }

                }
            }
        );
    }

    private void HideInvalidSpellLists()
    {
        int highestSlot = ClassHighestSlot();

        if (highestSlot < 1)
        {
            level1Windows.SetActive(false);
        }
        else
        {
            level1Windows.SetActive(true);
        }

        if (highestSlot < 2)
        {
            level2Windows.SetActive(false);
        }
        else
        {
            level2Windows.SetActive(true);
        }

        if (highestSlot < 3)
        {
            level3Windows.SetActive(false);
        }
        else
        {
            level3Windows.SetActive(true);
        }
    }

    private int ClassHighestSlot()
    {
        int classLevel = currentPC.GetClassLevelFromID(currentClass);

        if (currentClass == 1 || currentClass == 2 || currentClass == 3 || currentClass == 9 || currentClass == 11)
            if (classLevel < 3)
                return 1;
            else if (classLevel < 5)
                return 2;
            else
                return 3;
        else if (currentClass == 6 || currentClass == 7)
            if (classLevel == 5)
                return 2;
            else
                return 1;
        else if (currentClass == 4 || currentClass == 8)
            return 1;
        else
            return 0;
    }

    private int CantripLimit()
    {
        int classLevel = currentPC.GetClassLevelFromID(currentClass);

        switch (currentClass)
        {
            case 1:
                if (classLevel < 4) return 2;
                else return 3;
            case 2:
                if (classLevel < 4) return 3;
                else return 4;
            case 3:
                if (classLevel < 4) return 2;
                else return 3;
            case 4:
                return 2;
            case 8:
                return 3;
            case 9:
                if (classLevel < 4) return 4;
                else return 5;
            case 10:
                if (classLevel < 4) return 2;
                else return 3;
            case 11:
                if (classLevel < 4) return 3;
                else return 4;
            default:
                return 0;
        }
    }
    
    private int SpellLimit()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {DatabaseManager.GetClassNameFromID(currentClass)} " +
            $"FROM class_spells_known_by_level WHERE level = {currentPC.GetClassLevelFromID(currentClass)}"));
    }

    private Transform GetList(int level, string preppedAvailable)
    {
        return level switch
        {
            0 => preppedAvailable switch
            {
                "Available" => availableCantripsWindow,
                "Prepared" => preparedCantripsWindow,
                _ => null,
            },
            1 => preppedAvailable switch
            {
                "Available" => availableWindow1,
                "Prepared" => preparedWindow1,
                _ => null,
            },
            2 => preppedAvailable switch
            {
                "Available" => availableWindow2,
                "Prepared" => preparedWindow2,
                _ => null,
            },
            3 => preppedAvailable switch
            {
                "Available" => availableWindow3,
                "Prepared" => preparedWindow3,
                _ => null,
            },
            _ => null,
        };
    }

    public void CreateSpellPrefab(int id, string availablePrepared)
    {
        Transform location = GetList(SpellsManager.GetLevel(id), availablePrepared);
        GameObject obj = Instantiate(spellPrefab, location);

        spellListButtons.Add((id, obj));

        var ui = obj.GetComponentInChildren<TMP_Text>();
        ui.text = SpellsManager.GetName(id);

        var button = obj.GetComponentInChildren<Button>();
        button.onClick.AddListener(() => ShowSpellInfo(id));
    }

    public void ShowSpellInfo(int id)
    {
        currentSpell = id;
        spellPanel.SetActive(true);
        if (!SpellAlwaysPrepared(id))
        {
            if (SpellIsPrepared(id))
            {
                btnUnprepareSpell.gameObject.SetActive(true);
                btnPrepareSpell.gameObject.SetActive(false);
            }
            else
            {
                btnUnprepareSpell.gameObject.SetActive(false);
                if ((SpellsManager.GetLevel(id) == 0 && numberOfPreparedCantrips >= CantripLimit()) ||
                    (SpellsManager.GetLevel(id) > 0 && numberOfPreparedSpells >= SpellLimit()))
                    btnPrepareSpell.gameObject.SetActive(false);
                else
                    btnPrepareSpell.gameObject.SetActive(true);
            }
        }
        else
        {
            btnUnprepareSpell.gameObject.SetActive(false);
            btnPrepareSpell.gameObject.SetActive(false);            
        }
        DatabaseManager.Instance.ExecuteReader($"SELECT name, description FROM spells WHERE id = {id}",
            reader =>
            {
                txtSpellName.text = Convert.ToString(reader["name"]);
                txtSpellDescription.text = Convert.ToString(reader["description"]);
            }
        );
    }

    private void HideSpellInfo()
    {
        spellPanel.SetActive(false);
    }

    private void AddSpell(int id, string availablePrepared)
    {
        spellList.Add((id, availablePrepared));
        CreateSpellPrefab(id, availablePrepared);
    }

    private void RemoveSpell(int id)
    {
        var result = spellListButtons.FirstOrDefault(x => x.Item1 == id);

        if (result != default)
        {
            Destroy(result.Item2);
            spellListButtons.Remove(result);
        }
        else
        {
            Debug.LogError("No prefab found in list");
        }
        
        spellList.Remove(spellList.FirstOrDefault(x => x.Item1 == currentSpell));
    }

    private void PrepareUnprepareCurrentSpell()
    {
        bool wasPrepared = SpellIsPrepared(currentSpell);

        if (!wasPrepared)
        {
            if (SpellsManager.GetLevel(currentSpell) == 0)
            {
                if (numberOfPreparedCantrips >= CantripLimit())
                    return;
            }
            else
            {
                if (numberOfPreparedSpells >= SpellLimit())
                    return;
            }
        }

        RemoveSpell(currentSpell);

        if (wasPrepared)
        {
            if (!SpellAlwaysPrepared(currentSpell))
            {
                if (SpellsManager.GetLevel(currentSpell) == 0)
                {
                    numberOfPreparedCantrips -= 1;
                    UpdateText();
                }
                else
                {
                    numberOfPreparedSpells -= 1;
                    UpdateText();
                }
            }
            AddSpell(currentSpell, "Available");
            btnUnprepareSpell.gameObject.SetActive(false);
            btnPrepareSpell.gameObject.SetActive(true);
        }
        else
        {
            if (!SpellAlwaysPrepared(currentSpell))
            {
                if (SpellsManager.GetLevel(currentSpell) == 0)
                {
                    numberOfPreparedCantrips += 1;
                    UpdateText();
                }
                else
                {
                    numberOfPreparedSpells += 1;
                    UpdateText();
                }
            }
            AddSpell(currentSpell, "Prepared");
            btnUnprepareSpell.gameObject.SetActive(true);
            btnPrepareSpell.gameObject.SetActive(false);
        }
        HideSpellInfo();
    }

    private bool SpellIsPrepared(int id)
    {
        var result = spellList.FirstOrDefault(x => x.Item1 == id);

        if (result != default)
        {
            if (result.Item2 == "Prepared")
                return true;
            else return false;
        }
        Debug.LogError("No spell found with id " + id);
        return false;
    }

    private void UpdateText()
    {
        cantripCount.text = $"Cantrips: {numberOfPreparedCantrips} / {CantripLimit()}";
        spellCount.text = $"Spells: {numberOfPreparedSpells} / {SpellLimit()}";
    }
    
    private bool SpellAlwaysPrepared(int id)
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT always_prepared FROM pc_spells " +
            $"WHERE unit_id = {currentPC.UnitID} AND spell_id = {id}"));
    }

    private void SaveSpellList()
    {
        string castingStat = DatabaseManager.GetSpellcastingAbilityFromID(currentClass);
        string className = DatabaseManager.GetClassNameFromID(currentClass);

        DatabaseManager.Instance.ExecuteNonQuery($"DELETE FROM pc_spells WHERE unit_id = {currentPC.UnitID} AND source = '{className}'");

        for(int i = 0; i < spellList.Count; i++)
        {
            if(spellList[i].Item2 == "Prepared")
            {
                int spellID = spellList[i].Item1;

                DatabaseManager.Instance.ExecuteNonQuery($"INSERT INTO pc_spells (unit_id, spell_id, spellcasting_ability, source) " +
                $"VALUES ({currentPC.UnitID}, {spellID}, '{castingStat}', '{className}')");
            }

        }

    }
}

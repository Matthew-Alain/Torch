using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public TMP_Dropdown mainHand, offHand, armor;
    private List<TMP_Text> mainRow = new List<TMP_Text>();
    private List<TMP_Text> offRow = new List<TMP_Text>();

    public Button mainProperties, offProperties;
    public GameObject propertyPanel, mainTextElements, offTextElements;
    public TMP_Text weaponName, weaponDescription;
    public Button btnClosePropertyWindow;

    public TMP_Text baseACLabel, dexBonusLabel, totalACLabel, minStrLabel, loudLabel;
    public Button btnBack;
    private int PCID;
    
    void Awake()
    {
        PCID = DatabaseManager.Instance.lastPCEdited;
        mainRow.AddRange(mainTextElements.GetComponentsInChildren<TMP_Text>());
        offRow.AddRange(offTextElements.GetComponentsInChildren<TMP_Text>());

        mainHand.onValueChanged.AddListener(UpdateEquipment);
        offHand.onValueChanged.AddListener(UpdateEquipment);
        mainProperties.onClick.AddListener(() => OpenPropertiesWindow(mainHand.value));
        offProperties.onClick.AddListener(() => OpenPropertiesWindow(offHand.value));
        btnClosePropertyWindow.onClick.AddListener(ClosePropertyWindow);
        armor.onValueChanged.AddListener(UpdateArmor);
    }

    void Start()
    {
        UpdateEquipment(0);
        UpdateArmor(0);
    }

    void UpdateEquipment(int index)
    {
        UpdateEquipmentDetails(mainHand.value, mainRow, mainProperties);
        UpdateEquipmentDetails(offHand.value, offRow, offProperties);
    }
    
    void UpdateEquipmentDetails(int weaponID, List<TMP_Text> rowToEdit, Button propertyButton)
    {

        DatabaseManager.Instance.ExecuteReader(
            "SELECT * FROM weapons WHERE id = @weaponID",
            reader =>
            {
                while (reader.Read())
                {
                    if(Convert.ToString(reader["name"]) == "Shield")
                    {
                        rowToEdit[0].text = "--";
                        rowToEdit[1].text = "--";
                        rowToEdit[2].text = "--";
                        rowToEdit[3].text = "--";
                    }
                    else
                    {
                        string diceSize;
                        if (Convert.ToInt32(reader["versatile"]) > 0 && (offHand.value == 0 || mainHand.value == 0))
                        {
                            diceSize = Convert.ToString(reader["versatile"]);
                        }
                        else
                        {
                            diceSize = Convert.ToString(reader["dice_size"]);
                        }
                        rowToEdit[0].text = Convert.ToString(reader["dice_number"]) + "d" + diceSize;

                        rowToEdit[1].text = DatabaseManager.Instance.GetDamageType(Convert.ToInt32(reader["damage_type"]));

                        if (reader["melee_range"] == DBNull.Value)
                        {
                            rowToEdit[2].text = Convert.ToString(reader["normal_range"]) + " / " + Convert.ToString(reader["long_range"]);
                        }
                        else if (reader["normal_range"] == DBNull.Value)
                        {
                            rowToEdit[2].text = Convert.ToString(reader["melee_range"]);
                        }
                        else
                        {
                            rowToEdit[2].text = Convert.ToString(reader["melee_range"]) + " / " + Convert.ToString(reader["normal_range"]) + " / " + Convert.ToString(reader["long_range"]);
                        }

                        if (Convert.ToString(reader["stat"]) == "Finesse")
                        {
                            rowToEdit[3].text = "STR / DEX";
                        }
                        else
                        {
                            rowToEdit[3].text = Convert.ToString(reader["stat"]);
                        }

                        if (!(Convert.ToBoolean(reader["heavy"]) || Convert.ToBoolean(reader["light"]) || Convert.ToBoolean(reader["loading"]) ||
                        Convert.ToBoolean(reader["thrown"]) || Convert.ToBoolean(reader["two-handed"]) || Convert.ToString(reader["stat"]) == "Finesse" ||
                        Convert.ToInt32(reader["melee_range"]) == 10 || Convert.ToBoolean(reader["versatile"]) || Convert.ToBoolean(reader["mastery"])))
                        {
                            propertyButton.gameObject.SetActive(false);
                        }
                        else
                        {
                            propertyButton.gameObject.SetActive(true);
                        }
                    }
                }
            },
            ("@weaponID", weaponID)
        );
    }

    public void OpenPropertiesWindow(int weaponID)
    {
        propertyPanel.SetActive(true);

        DatabaseManager.Instance.ExecuteReader(
            "SELECT * FROM weapons WHERE id = @weaponID",
            reader =>
            {
                while (reader.Read())
                {
                    weaponName.text = Convert.ToString(reader["name"]);

                    weaponDescription.text = "";

                    if(Convert.ToString(reader["name"]) == "Shield")
                    {
                        weaponDescription.text = "Shield - Rather than being used as a weapon, having a shield equipped increases your AC by 2.";
                    }
                    else
                    {
                        if (!(reader["stat"] == DBNull.Value) && Convert.ToString(reader["stat"]) == "Finesse")
                        {
                            weaponDescription.text += "Finesse - Your may use either Strength or Dexterity as your stat for this weapon.\n";
                        }
                        if (!(reader["heavy"] == DBNull.Value) && Convert.ToBoolean(reader["heavy"]))
                        {
                            weaponDescription.text += "Heavy - It heavy\n";
                        }
                        if (!(reader["light"] == DBNull.Value) && Convert.ToBoolean(reader["light"]))
                        {
                            weaponDescription.text += "Light - It light\n";
                        }
                        if (!(reader["loading"] == DBNull.Value) && Convert.ToBoolean(reader["loading"]))
                        {
                            weaponDescription.text += "Loading - It loads\n";
                        }
                        if (!(reader["melee_range"] == DBNull.Value) && Convert.ToInt32(reader["melee_range"]) == 10)
                        {
                            weaponDescription.text += "Reach - Your melee range with this weapon is 10 feet instead of 5 feet.\n";
                        }
                        if (!(reader["thrown"] == DBNull.Value) && Convert.ToBoolean(reader["thrown"]))
                        {
                            weaponDescription.text += "Thrown - \n";
                        }
                        if (!(reader["two-handed"] == DBNull.Value) && Convert.ToBoolean(reader["two-handed"]))
                        {
                            weaponDescription.text += "Two-Handed - \n";
                        }
                        if (!(reader["versatile"] == DBNull.Value) && Convert.ToBoolean(reader["versatile"]))
                        {
                            weaponDescription.text += "Versatile - \n";
                        }
                        if (!(reader["mastery"] == DBNull.Value) && Convert.ToBoolean(reader["mastery"]))
                        {
                            weaponDescription.text += "Sap - \n";
                        }
                    }
                }
            },
            ("@weaponID", weaponID)
        );
    }

    private void ClosePropertyWindow()
    {
        propertyPanel.SetActive(false);
    }

    private void UpdateArmor(int index)
    {
        DatabaseManager.Instance.ExecuteReader(
            "SELECT * FROM armor WHERE id = @index",
            reader =>
            {
                while (reader.Read())
                {
                    int baseAC = Convert.ToInt32(reader["base_ac"]);
                    int armorType = Convert.ToInt32(reader["category"]);
                    int strRequirement = Convert.ToInt32(reader["strength"]);
                    bool stealthDis = Convert.ToBoolean(reader["stealth_disadvantage"]);

                    baseACLabel.text = baseAC.ToString();

                    int dexBonus = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                            "SELECT mDEX FROM pc_stats WHERE id = @PCID",
                            ("@PCID", PCID)
                        ));

                    if (armorType == 2)
                    {
                        dexBonus = 0;
                        dexBonusLabel.text = "--";
                    }
                    else if (armorType == 1)
                    {
                        dexBonus = Math.Min(2, dexBonus);
                    }

                    if (dexBonus < 0)
                    {
                        dexBonusLabel.text = dexBonus.ToString();
                    }
                    else
                    {
                        dexBonusLabel.text = "+" + dexBonus;
                    }

                    totalACLabel.text = (baseAC + dexBonus).ToString();

                    if (strRequirement == 0)
                    {
                        minStrLabel.text = "--";
                    }
                    else
                    {
                        minStrLabel.text = strRequirement.ToString();
                    }

                    if (stealthDis)
                    {
                        loudLabel.text = "Yes";
                    }
                    else
                    {
                        loudLabel.text = "No";
                    }
                }
            },
            ("@index", index)
        );
    }
    
    void SaveEquipment()
    {
        //TODO: add columns to the database to save AC and current equipment (maybe just hit mod?)
        //Import these columns as default values to populate dropdown lists
        //Write UPDATE query to save changes to the database
    }
}
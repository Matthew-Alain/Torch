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
    private BasePC currentPC;
    
    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
        
        mainRow.AddRange(mainTextElements.GetComponentsInChildren<TMP_Text>());
        offRow.AddRange(offTextElements.GetComponentsInChildren<TMP_Text>());

        mainHand.onValueChanged.AddListener(UpdateMainHand);
        offHand.onValueChanged.AddListener(UpdateOffHand);
        mainProperties.onClick.AddListener(() => OpenPropertiesWindow(mainHand.value));
        offProperties.onClick.AddListener(() => OpenPropertiesWindow(offHand.value));
        btnClosePropertyWindow.onClick.AddListener(ClosePropertyWindow);
        armor.onValueChanged.AddListener(UpdateArmor);

        btnBack.onClick.AddListener(SaveEquipment);
    }

    void Start()
    {
        GetDefaultInfo();
    }

    void GetDefaultInfo()
    {
        UpdateMainHand(currentPC.GetMainhandID());
        UpdateArmor(currentPC.GetArmorID());
    }

    void UpdateMainHand(int index)
    {
        HandleTwoHandedWeapons(index, true);
        UpdateEquipmentDetails(mainHand.value, mainRow, mainProperties);
        UpdateEquipmentDetails(offHand.value, offRow, offProperties);
    }

    void UpdateOffHand(int index)
    {
        HandleTwoHandedWeapons(index, false);
        UpdateEquipmentDetails(offHand.value, offRow, offProperties);
        UpdateEquipmentDetails(mainHand.value, mainRow, mainProperties);
    }

    void UpdateEquipmentDetails(int weaponID, List<TMP_Text> rowToEdit, Button propertyButton)
    {

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT * FROM weapons WHERE id = {weaponID}",
            reader =>
            {
                while (reader.Read())
                {
                    if (Convert.ToString(reader["name"]) == "Shield")
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

                        if (Convert.ToBoolean(reader["heavy"]) ||
                            Convert.ToBoolean(reader["light"]) ||
                            Convert.ToBoolean(reader["loading"]) ||
                            Convert.ToBoolean(reader["thrown"]) ||
                            Convert.ToBoolean(reader["two_handed"]) ||
                            Convert.ToString(reader["stat"]) == "Finesse" ||
                            (reader["melee_range"] != DBNull.Value && Convert.ToInt32(reader["melee_range"]) > 5 ) ||
                            Convert.ToBoolean(reader["versatile"]) ||
                            !(reader["mastery"] == DBNull.Value))
                        {
                            propertyButton.gameObject.SetActive(true);
                        }
                        else
                        {
                            propertyButton.gameObject.SetActive(false);
                        }
                    }
                }
            }
        );
    }
    
    void HandleTwoHandedWeapons(int weaponID, bool updatingMainHand)
    {
        bool mainHandTwoHanded = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT two_handed FROM weapons WHERE id = {mainHand.value}"
            ));
        
        bool offHandTwoHanded = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT two_handed FROM weapons WHERE id = {offHand.value}"
            ));

        if (!updatingMainHand && offHandTwoHanded) //If you are updating the offhand and the offhand is two-handed
        {
            offHand.value = 0;  //Clear the offhand
            mainHand.value = weaponID; //Put the new weapon into the mainhand
        }
        else if (updatingMainHand && (mainHandTwoHanded || offHandTwoHanded)) //If you are updating the mainhand and either weapon is two-handed
        {
            offHand.value = 0;  //Clear the offhand
        }
        else if (!updatingMainHand && mainHandTwoHanded && weaponID != 0) //If you are updating the offhand with a weapon that's NOT two-handed or unarmed, and the mainhand is two-handed
        {
            mainHand.value = 0; //Clear the mainhand
        }
    }

    public void OpenPropertiesWindow(int weaponID)
    {
        propertyPanel.SetActive(true);

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT * FROM weapons WHERE id = {weaponID}",
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
                        if (!(reader["two_handed"] == DBNull.Value) && Convert.ToBoolean(reader["two_handed"]))
                        {
                            weaponDescription.text += "Two-Handed - \n";
                        }
                        if (!(reader["versatile"] == DBNull.Value) && Convert.ToBoolean(reader["versatile"]))
                        {
                            weaponDescription.text += "Versatile - \n";
                        }
                        if (!(reader["mastery"] == DBNull.Value))
                        {
                            weaponDescription.text += "Mastery - If you have the Weapon Mastery class feature, you have the following ability with this weapon:\n";

                            DatabaseManager.Instance.ExecuteReader(
                                $"SELECT name, description FROM weapon_masteries WHERE id = {Convert.ToInt32(reader["mastery"])}",
                                reader2 =>
                                {
                                    while (reader2.Read())
                                    {
                                        weaponDescription.text += Convert.ToString(reader2["name"]) + " - " + Convert.ToString(reader2["description"]);
                                    }
                                }
                            );
                        }
                    }
                }
            }
        );
    }

    private void ClosePropertyWindow()
    {
        propertyPanel.SetActive(false);
    }

    private void UpdateArmor(int index)
    {
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT * FROM armor WHERE id = {index}",
            reader =>
            {
                while (reader.Read())
                {
                    int baseAC = Convert.ToInt32(reader["base_ac"]);
                    int armorType = Convert.ToInt32(reader["category"]);
                    int strRequirement = Convert.ToInt32(reader["strength"]);
                    bool stealthDis = Convert.ToBoolean(reader["stealth_disadvantage"]);

                    baseACLabel.text = baseAC.ToString();

                    int dexBonus = currentPC.GetModifier("mDEX");

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
            }
        );
    }
    
    void SaveEquipment()
    {
        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE saved_pcs SET main_hand_item = {mainHand.value}, off_hand_item = {offHand.value}, equipped_armor = {armor.value} WHERE id = {currentPC.UnitID}"
        );

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE unit_stats SET AC = {totalACLabel.text} WHERE id = {currentPC.UnitID}"
        );
    }
}
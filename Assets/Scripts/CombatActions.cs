using UnityEngine;
using System;


public class CombatActions
{
    public static bool HasMajor(int unitID)
    {
        bool hasMajor = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
            "SELECT major_action FROM unit_resources WHERE id = @unitID",
            ("@unitID", unitID)
        ));

        if (hasMajor)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE unit_resources SET major_action = 0 WHERE id = @unitID",
                ("@unitID", unitID)
            );
        }
        else
        {
            Debug.Log("No major action available");
        }

        return hasMajor;
    }

    public static bool HasMinor(int unitID)
    {
        bool hasMinor = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
            "SELECT minor_action FROM unit_resources WHERE id = @unitID",
            ("@unitID", unitID)
        ));

        if (hasMinor)
        {
            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE unit_resources SET minor_action = 0 WHERE id = @unitID",
                ("@unitID", unitID)
            );
        }
        else
        {
            Debug.Log("No minor action available");
        }

        return hasMinor;
    }

    public static bool HasReaction(int unitID)
    {
        bool hasReaction = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
            "SELECT reaction FROM unit_resources WHERE id = @unitID",
            ("@unitID", unitID)
        ));

        if (!hasReaction)
        {
            Debug.Log("No reaction available");
        }

        return hasReaction;
    }

    public static void Dash(int unitID)
    {
        if (HasMajor(unitID))
        {
            int current_speed = 0;
            int base_speed = 0;
            DatabaseManager.Instance.ExecuteReader(
                "SELECT current_speed, base_speed FROM unit_resources WHERE id = @unitID",
                reader =>
                {
                    while (reader.Read())
                    {
                        current_speed = Convert.ToInt32(reader["current_speed"]);
                        base_speed = Convert.ToInt32(reader["base_speed"]);
                    }
                },
                ("@unitID", unitID)
            );

            current_speed += base_speed;

            DatabaseManager.Instance.ExecuteNonQuery(
                "UPDATE unit_resources SET current_speed = @current_speed WHERE id = @unitID",
                ("@current_speed", current_speed),
                ("@unitID", unitID)
            );
        }
    }

    public static void MeleeWeaponAttack(int attackerID, int weaponID, int targetID)
    {
        if (HasMajor(attackerID))
        {
            int dieRoll = DiceRoller.Rolld20();

            string attackStat = "";
            string category = "";
            bool light = false;
            bool finesse = false;
            int dice_number = 0;
            int dice_size = 0;
            DatabaseManager.Instance.ExecuteReader(
                "SELECT stat, category, light, dice_number, dice_size FROM weapons WHERE id = @weaponID",
                reader =>
                {
                    attackStat = Convert.ToString(reader["stat"]);
                    category = Convert.ToString(reader["category"]);
                    light = Convert.ToBoolean(reader["light"]);
                    finesse = Convert.ToString(reader["stat"]) == "Finesse";
                    dice_number = Convert.ToInt32(reader["dice_number"]);
                    dice_size = Convert.ToInt32(reader["dice_number"]);
                },
                ("@weaponID", weaponID)
            );

            int mSTR = 0;
            int mDEX = 0;

            DatabaseManager.Instance.ExecuteReader(
                "SELECT mSTR, mDEX FROM unit_stats WHERE id = @unitID",
                reader =>
                {
                    while (reader.Read())
                    {
                        mSTR = Convert.ToInt32(reader["mSTR"]);
                        mDEX = Convert.ToInt32(reader["mDEX"]);

                    }
                },
                ("@unitID", attackerID)
            );

            int attackModifier = 0;

            if (attackStat == "STR" || (attackStat == "Finesse" && mSTR > mDEX))
            {
                attackModifier = mSTR;
            }
            else if (attackStat == "DEX" || attackStat == "Finesse" && mSTR <= mDEX)
            {
                attackModifier = mDEX;
            }

            int pb = 0;

            if (category == "Simple Melee" || category == "Simple Ranged")
            {
                pb = CombatUnitManager.Instance.GetProficiency(attackerID, "all_simple");
            }
            else if (light)
            {
                pb = CombatUnitManager.Instance.GetProficiency(attackerID, "martial_light");
            }
            else if (finesse)
            {
                pb = CombatUnitManager.Instance.GetProficiency(attackerID, "martial_finesse");
            }
            else
            {
                pb = CombatUnitManager.Instance.GetProficiency(attackerID, "all_martial");
            }

            int totalResult = dieRoll + attackModifier + pb;

            if (totalResult >= CombatUnitManager.Instance.GetAC(targetID))
            {
                Debug.Log("You hit!");

                if(dieRoll == 20)
                {
                    CombatUnitManager.Instance.DamageUnit(targetID, DiceRoller.Roll(dice_number*2, dice_size), true);
                }
                else
                {
                    CombatUnitManager.Instance.DamageUnit(targetID, DiceRoller.Roll(dice_number, dice_size), false);
                }
            }

        }
    }

    public static void RangedWeaponAttack(int attackerID, int targetID)
    {
        if (HasMajor(attackerID))
        {

        }
    }

    public static void LongRangeWeaponAttack(int attackerID, int targetID)
    {
        if (HasMajor(attackerID))
        {

        }
    }

}
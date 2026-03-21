using UnityEngine;
using System;


public class CombatActions
{

    public static void Dash(BaseUnit unit)
    {
        if (unit.UseMajorAction())
        {
            int current_speed = unit.GetCurrentSpeed();
            int base_speed = unit.GetBaseSpeed();

            current_speed += base_speed;

            unit.SetCurrentSpeed(current_speed);
        }
    }

    public static void MeleeWeaponAttack(BaseUnit attacker, int weaponID, BaseUnit target)
    {
        if (attacker.UseMajorAction())
        {
            int dieRoll = DiceRoller.Rolld20();

            string attackStat = "";
            int dice_number = 0;
            int dice_size = 0;
            DatabaseManager.Instance.ExecuteReader(
                $"SELECT stat, category, light, dice_number, dice_size FROM weapons WHERE id = {weaponID}",
                reader =>
                {
                    attackStat = Convert.ToString(reader["stat"]);
                    dice_number = Convert.ToInt32(reader["dice_number"]);
                    dice_size = Convert.ToInt32(reader["dice_size"]);
                }
            );

            int mSTR = attacker.GetModifier("mSTR");
            int mDEX = attacker.GetModifier("mDEX");

            int attackModifier = 0;
            if (attackStat == "STR" || (attackStat == "Finesse" && mSTR > mDEX))
            {
                attackModifier = mSTR;
            }
            else if (attackStat == "DEX" || attackStat == "Finesse" && mSTR <= mDEX)
            {
                attackModifier = mDEX;
            }
            Debug.Log("Your weapon uses " + attackStat + ", so you add " + attackModifier + " to your d20 roll.");


            int pb = attacker.GetWeaponProficiency(weaponID);
            if (pb > 0)
            {
                Debug.Log("You have proficiency with this weapon, so you had your proficiency bonus of " + pb + " to your roll.");
            }
            else
            {
                Debug.Log("You do not have proficiency with this weapon, so you do not add your proficiency bonus to this roll.");
            }


            int totalResult = dieRoll + attackModifier + pb;
            Debug.Log($"You rolled {dieRoll}, with a modifier of {attackModifier}, and a proficiency of {pb}, for a total result of: {totalResult}");

            if (totalResult >= target.GetAC())
            {
                Debug.Log($"The target's AC is {target.GetAC()}, so you hit!");
                CombatMenuManager.Instance.SetDisplayText($"The target's AC is {target.GetAC()}, so you hit!");

                if (dieRoll == 20)
                {
                    target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size), true);
                }
                else
                {
                    target.TakeDamage(DiceRoller.Roll(dice_number, dice_size), false);
                }
            }
            else
            {
                CombatMenuManager.Instance.SetDisplayText($"The target's AC is {target.GetAC()}, so you miss...");
            }

        }
    }

    public static void RangedWeaponAttack(BaseUnit attacker, BaseUnit target)
    {
        if (attacker.UseMajorAction())
        {

        }
    }

    public static void LongRangeWeaponAttack(BaseUnit attacker, BaseUnit target)
    {
        if (attacker.UseMajorAction())
        {

        }
    }

}
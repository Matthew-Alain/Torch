using UnityEngine;
using System;
using System.Collections;
using Unity.VisualScripting;


public class CombatActions
{

    public static void Dash(BaseUnit unit)
    {
        int current_speed = unit.GetResource("current_speed");
        int base_speed = unit.GetResource("base_speed");

        current_speed += base_speed;

        unit.SetResource("current_speed", current_speed);
    }

    public static void Dodge(BaseUnit unit)
    {
        unit.SetCondition("dodging", true);
    }

    public static void Disengage(BaseUnit unit)
    {
        unit.SetCondition("disengaging", true);
    }

    public static void Help(BaseUnit unit, BaseUnit target)
    {
        target.SetCondition("distracted", true);
    }

    public static void Hide(BaseUnit unit)
    {
        //Make d20 test
        //If successful
        unit.SetCondition("hiding", true);
    }

    public static void MeleeWeaponAttack(BaseUnit attacker, int weaponID, BaseUnit target)
    {
        if (attacker.UseResource("major_action"))
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
                CombatMenuManager.Instance.DisplayText($"The target's AC is {target.GetAC()}, so you hit!");

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
                CombatMenuManager.Instance.DisplayText($"The target's AC is {target.GetAC()}, so you miss...");
            }

        }
    }

    public static void RangedWeaponAttack(BaseUnit attacker, BaseUnit target)
    {
        if (attacker.UseResource("major_action"))
        {

        }
    }

    public static void LongRangeWeaponAttack(BaseUnit attacker, BaseUnit target)
    {
        if (attacker.UseResource("major_action"))
        {
            //If user has sharpshooter, just make a ranged weapon attack
        }
    }

    public static IEnumerator MonsterAttack(BaseMonster attacker, BaseUnit target, int attackID)
    {
        if (attacker.UseResource("major_action"))
        {
            int dieRoll = DiceRoller.Rolld20();

            int hitMod = 0;
            int dice_number = 0;
            int dice_size = 0;
            int damageBonus = 0;
            string attackName = "";
            DatabaseManager.Instance.ExecuteReader(
                $"SELECT * FROM monster_attacks WHERE id = {attackID}",
                reader =>
                {
                    hitMod = Convert.ToInt32(reader["hit_modifier"]);
                    dice_number = Convert.ToInt32(reader["dice_number"]);
                    dice_size = Convert.ToInt32(reader["dice_size"]);
                    damageBonus = Convert.ToInt32(reader["damage_bonus"]);
                    attackName = Convert.ToString(reader["name"]);
                }
            );

            int totalResult = dieRoll + hitMod;

            CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} is attacking {target.UnitName} with {attackName}");
            yield return new WaitForSeconds(2f);

            if (totalResult >= target.GetAC())
            {

                if (dieRoll == 20)
                {
                    CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a natural 20 to hit!");
                    yield return new WaitForSeconds(1.5f);
                    target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size)+damageBonus, true);
                }
                else
                {
                    CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a {totalResult} to hit, which hits!");
                    yield return new WaitForSeconds(1.5f);
                    target.TakeDamage(DiceRoller.Roll(dice_number, dice_size)+damageBonus, false);
                }
            }
            else
            {
                CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a {totalResult} to hit, which misses!");
            }

        }
    }
}
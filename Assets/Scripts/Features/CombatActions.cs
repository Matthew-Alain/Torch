using UnityEngine;
using System;
using System.Collections;

public class CombatActions: MonoBehaviour
{

    public static void Dash(BaseUnit unit)
    {
        unit.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{unit.UnitName} is dashing"));
        int current_speed = unit.GetResource("current_speed");
        int base_speed = unit.GetResource("base_speed");

        current_speed += base_speed;

        unit.SetResource("current_speed", current_speed);
    }

    public static void Dodge(BaseUnit unit)
    {
        unit.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{unit.UnitName} is dodging"));
        unit.SetCondition("dodging", true);
    }

    public static void Disengage(BaseUnit unit)
    {
        unit.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{unit.UnitName} is disengaging"));
        unit.SetCondition("disengaging", true);
    }

    public static void Help(BaseUnit target)
    {
        target.SetCondition("distracted", true);
    }

    public static void Hide(BaseUnit unit)
    {
        //Make d20 test
        //If successful
        unit.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{unit.UnitName} is hiding"));
        unit.SetCondition("hiding", true);
    }

    public static IEnumerator AttackWithWeapon(BaseUnit attacker, BaseUnit target, int weaponID, Action<bool> onComplete)
    {
        if (target == null)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        CombatStateManager.Instance.processing = true;

        int distance = attacker.occupiedTile.CheckDistanceInTiles(target.occupiedTile);

        int melee_range = RangeHelper.GetMeleeRangeInTiles(weaponID);
        int normal_range = RangeHelper.GetNormalRangeInTiles(weaponID);
        int long_range = RangeHelper.GetLongRangeInTiles(weaponID);

        if (distance <= melee_range)
        {
            yield return CombatUnitManager.Instance.StartCoroutine(MeleeWeaponAttack(attacker, target, weaponID));
        }
        else if (distance <= normal_range)
        {
            yield return CombatUnitManager.Instance.StartCoroutine(RangedWeaponAttack(attacker, target, weaponID));
        }
        else if (distance <= long_range)
        {
            yield return CombatUnitManager.Instance.StartCoroutine(LongRangeWeaponAttack(attacker, target, weaponID));
        }
        else
        {
            CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText("The target is out of range"));
            // Log("The target is out of range");
        }

        CombatStateManager.Instance.processing = false;
        onComplete?.Invoke(true);
    }

    public static IEnumerator MeleeWeaponAttack(BaseUnit attacker, BaseUnit target, int weaponID)
    {
        int dieRoll;
        if (target.GetCondition("dodging") || attacker.GetCondition("prone"))
        {
            dieRoll = DiceRoller.Rolld20(false, true);
        }
        else
        {
            dieRoll = DiceRoller.Rolld20();
        }

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
        // Debug.Log("Your weapon uses " + attackStat + ", so you add " + attackModifier + " to your d20 roll.");


        int pb = attacker.GetWeaponProficiency(weaponID);
        // Debug.Log("PB = " + pb);

        int totalResult = dieRoll + attackModifier + pb;
        // Debug.Log($"You rolled {dieRoll}, with a modifier of {attackModifier}, and a proficiency of {pb}, for a total result of: {totalResult}");

        bool result = totalResult >= target.GetAC();

        yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayDiceRoll(attacker, dieRoll, attackModifier, pb, result));

        if (result)
        {
            // Debug.Log($"The target's AC is {target.GetAC()}, so you hit!");

            if (dieRoll == 20)
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a natural 20 to hit {target.UnitName}!"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size)+attackModifier, true));
            }
            else
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which hits {target.UnitName}"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number, dice_size)+attackModifier, false));
            }
        }
        else
        {
            // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which misses {target.UnitName}"));
        }
    }

    public static IEnumerator RangedWeaponAttack(BaseUnit attacker, BaseUnit target, int weaponID)
    {
        int dieRoll;
        if (target.GetCondition("dodging") || target.GetCondition("prone"))
        {
            //Or if there is an enemy within 5 feet:
            dieRoll = DiceRoller.Rolld20(false, true);
        }
        else
        {
            dieRoll = DiceRoller.Rolld20();
        }

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
        // Debug.Log("Your weapon uses " + attackStat + ", so you add " + attackModifier + " to your d20 roll.");


        int pb = attacker.GetWeaponProficiency(weaponID);

        int totalResult = dieRoll + attackModifier + pb;
        // Debug.Log($"You rolled {dieRoll}, with a modifier of {attackModifier}, and a proficiency of {pb}, for a total result of: {totalResult}");

        bool result = totalResult >= target.GetAC();

        yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayDiceRoll(attacker, dieRoll, attackModifier, pb, result));

        if (result)
        {
            // Debug.Log($"The target's AC is {target.GetAC()}, so you hit!");

            if (dieRoll == 20)
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a natural 20 to hit {target.UnitName}!"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size)+attackModifier, true));
            }
            else
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which hits {target.UnitName}"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number, dice_size)+attackModifier, false));
            }


        }
        else
        {
            // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which misses {target.UnitName}"));
        }
    }

    public static IEnumerator LongRangeWeaponAttack(BaseUnit attacker, BaseUnit target, int weaponID)
    {
        //If user has sharpshooter, just make a ranged weapon attack
        int dieRoll = DiceRoller.Rolld20(false, true);

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
        // Debug.Log("Your weapon uses " + attackStat + ", so you add " + attackModifier + " to your d20 roll.");


        int pb = attacker.GetWeaponProficiency(weaponID);

        int totalResult = dieRoll + attackModifier + pb;
        // Debug.Log($"You rolled {dieRoll}, with a modifier of {attackModifier}, and a proficiency of {pb}, for a total result of: {totalResult}");

        bool result = totalResult >= target.GetAC();

        yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayDiceRoll(attacker, dieRoll, attackModifier, pb, result));

        if (result)
        {
            // Debug.Log($"The target's AC is {target.GetAC()}, so you hit!");

            if (dieRoll == 20)
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a natural 20 to hit {target.UnitName}!"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size)+attackModifier, true));
            }
            else
            {
                // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which hits {target.UnitName}"));
                yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number, dice_size)+attackModifier, false));
            }


        }
        else
        {
            // yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled {totalResult} to hit, which misses {target.UnitName}"));
        }
    }
}
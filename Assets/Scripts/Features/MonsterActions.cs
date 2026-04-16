
using System;
using System.Collections;
using UnityEngine;

public class MonsterActions: MonoBehaviour
{
    public static IEnumerator Attack(BaseMonster attacker, BaseUnit target, int attackID)
    {
        if (attacker.UseResource("current_number_of_attacks"))
        {
            int dieRoll = DiceRoller.Rolld20();

            int hitMod = 0;
            int dice_number = 0;
            int dice_size = 0;
            int damageBonus = 0;
            string attackName = "";
            DatabaseManager.Instance.ExecuteReader(
                $"SELECT * FROM monster_actions WHERE id = {attackID}",
                reader =>
                {
                    hitMod = Convert.ToInt32(reader["hit_modifier"]);
                    dice_number = Convert.ToInt32(reader["dice_number"]);
                    dice_size = Convert.ToInt32(reader["dice_size"]);
                    damageBonus = Convert.ToInt32(reader["damage_bonus"]);
                    attackName = Convert.ToString(reader["name"]);
                }
            );

            if(hitMod == 999)
            {
                // Debug.Log("Skipping to apply effect");
                yield return attacker.StartCoroutine(ApplyEffect(attacker, target, attackID));
            }
            else
            {
                int totalResult = dieRoll + hitMod;
                yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} is attacking {target.UnitName} with {attackName}"));

                if (totalResult >= target.GetAC())
                {
                    if (dieRoll == 20)
                    {
                        yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a natural 20 to hit"));
                        yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number * 2, dice_size) + damageBonus, true));
                    }
                    else
                    {
                        yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a {totalResult} to hit, which hits"));
                        yield return target.StartCoroutine(target.TakeDamage(DiceRoller.Roll(dice_number, dice_size) + damageBonus, false));
                    }

                    yield return attacker.StartCoroutine(ApplyEffect(attacker, target, attackID));
                }
                else
                {
                    yield return CombatMenuManager.Instance.StartCoroutine(CombatMenuManager.Instance.DisplayText($"{attacker.UnitName} rolled a {totalResult} to hit, which misses"));
                    if (attackID == 4)
                        yield return attacker.StartCoroutine(ApplyEffect(attacker, target, attackID));
                }
            }
        }
        yield return null;
    }

    public static void Dodge(BaseMonster monster)
    {
        if (monster.UseResource("major_action"))
        {
            monster.SetCondition("dodging", true);
        }
    }

    public static int GetEffect(int attackID)
    {
        var effect = DatabaseManager.Instance.ExecuteScalar($"SELECT effect_id FROM monster_actions WHERE id = {attackID}");

        if (effect == DBNull.Value)
            return -1;

        return Convert.ToInt32(effect);
    }
    
    public static IEnumerator ApplyEffect(BaseUnit attacker, BaseUnit target, int attackID)
    {
        var effect = DatabaseManager.Instance.ExecuteScalar($"SELECT effect_id FROM monster_actions WHERE id = {attackID}");

        if (effect == DBNull.Value)
        {
            yield return null;
        }
        else
        {
            int effectID = Convert.ToInt32(effect);

            switch(effectID)
            {
                case 1:
                    yield return attacker.StartCoroutine(attacker.PullTarget(target, 9));
                    break;
                case 2:
                    yield return attacker.StartCoroutine(attacker.PushTarget(target, 1));
                    break;
                case 3:
                    yield return attacker.StartCoroutine(((BaseMonster)attacker).AttackTarget(target, attackID));
                    break;
            }
        }
    }
}
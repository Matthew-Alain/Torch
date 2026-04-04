using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Spells: MonoBehaviour
{

    public static string GetName(int id)
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM spells WHERE id = {id}"));
    }

    public static int GetLevel(int id)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT level FROM spells WHERE id = {id}"));
    }

    public static string GetSlotCost(int id)
    {
        if (GetLevel(id) == 1)
            return "level_1_slots";
        if (GetLevel(id) == 2)
            return "level_2_slots";
        if (GetLevel(id) == 3)
            return "level_3_slots";

        Debug.Log("No slot cost");
        return "";
    }

    public static TargetType GetTargetType(int id)
    {
        string type = Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT target_type FROM spells WHERE id = {id}"));

        switch (type)
        {
            case "TargetType.Unit":
                return TargetType.Unit;
            case "TargetType.PC":
                return TargetType.PC;
            case "TargetType.Monster":
                return TargetType.Monster;
            case "TargetType.AnyTile":
                return TargetType.AnyTile;
            case "TargetType.EmptyTile":
                return TargetType.EmptyTile;
            default:
                UnityEngine.Debug.LogError("Invalid targetype for spell id " + id);
                return TargetType.AnyTile;
        }
    }

    public static string GetCastTime(int id)
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT cast_time FROM spells WHERE id = {id}"));
    }

    public static int GetRange(int id)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT range FROM spells WHERE id = {id}")) / 5;
    }

    public static int GetRadius(int id)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT radius FROM spells WHERE id = {id}")) / 5;
    }

    public static string GetAttackSaveNone(int id)
    {
        int type = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT attack_save_none FROM spells WHERE id = {id}"));

        switch (type)
        {
            case 1:
                return "attack";
            case 2:
                return "save";
            case 3:
                return "none";
            default:
                Debug.LogError("Invalid attack/save/none type for spell id " + id);
                return "";
        }
    }

    public static string GetSaveType(int id)
    {
        var saveType = DatabaseManager.Instance.ExecuteScalar($"SELECT save_type FROM spells WHERE id = {id}");
        if (saveType == DBNull.Value)
        {
            return "none";
        }
        return Convert.ToString(saveType).ToUpper();

        // string type = Convert.ToString(saveType).ToUpper();

        // switch (type)
        // {
        //     case "STR":
        //         return "str_save";
        //     case "DEX":
        //         return "dex_save";
        //     case "CON":
        //         return "con_save";
        //     case "INT":
        //         return "int_save";
        //     case "WIS":
        //         return "wis_save";
        //     case "CHA":
        //         return "cha_save";
        //     default:
        //         Debug.LogError("Invalid save type type for spell id " + id);
        //         return "";
        // }
    }

    public static int GetDiceNumber(int id)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_number FROM spells WHERE id = {id}"));
    }

    public static int GetDiceSize(int id)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_size FROM spells WHERE id = {id}"));
    }

    public static bool DoesHalfOnSave(int id)
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT half_on_save FROM spells WHERE id = {id}"));
    }





    public IEnumerator CastSpell(int id, BaseUnit caster, int spellLevel, string spellcastingAbility)
    {
        
        // var context = new ActionContext
        // {

        // };

        // yield return ReactionManager.Instance.CheckForReactions(
        //     ReactionTrigger.BeforeCastSpell,
        //     context
        // );

        switch (id)
        {
            case 0:
                StartCoroutine(Fireball(caster, spellLevel, spellcastingAbility));
                break;
            case 1:
                AcidSplash(caster, spellcastingAbility);
                break;
            default:
                Debug.LogError("No spell ID found for " + id);
                break;
        }

        // yield return ReactionManager.Instance.CheckForReactions(
        //     ReactionTrigger.AfterCastSpell,
        //     context
        // );
        yield return null;
    }

    public static void AcidSplash(BaseUnit caster, string spellcastingAbility)
    {

    }

    public static void FireBolt(BaseUnit caster)
    {

    }
    
    public static void CureWounds(BaseUnit caster, int spellLevel)
    {
        
    }

    public static IEnumerator Fireball(BaseUnit caster, int spellLevel, string spellcastingAbility)
    {
        int id = 3;
        //TODO: Add damage scaling based on spell level, and multi-targeting

        int DC = caster.GetSaveDCForStat(spellcastingAbility);
        int spellAttack = caster.GetAttackBonusForStat(spellcastingAbility);

        CombatStateManager.Instance.StartTileSelection(
            TargetType.AnyTile,
            (tile) =>
            {
                var targets = AOEHelper.GetUnitsInRadius(tile, GetRadius(id));
                // .Where(u => u.Faction == Faction.Monster).ToList(); //If it only affects enemies

                // var context = new ActionContext
                // {
                //     TriggeringUnit = caster,
                //     Targets = targets,
                //     Damage = 10
                // };

                int damage = DiceRoller.Roll(GetDiceNumber(id), GetDiceSize(id));

                foreach (BaseUnit target in targets)
                {
                    if (GetAttackSaveNone(id) == "attack")
                    {
                        int roll = caster.MakeAttackWithStat(spellcastingAbility);
                        if(roll == 200)
                        {
                            target.TakeDamage(damage, true);
                        }
                        else if(roll >= target.GetAC())
                        {
                            target.TakeDamage(damage, false);
                        }
                    }
                    else if (GetAttackSaveNone(id) == "save")
                    {
                        if (target.MakeSave(GetSaveType(id), DC))
                        {
                            target.TakeDamage(damage, false);
                            Debug.Log($"Dealt {damage} damage to {target.UnitName}");
                        }
                        else
                        {
                            if (DoesHalfOnSave(id))
                            {
                                target.TakeDamage((int)Math.Floor((decimal)(damage / 2)), false);
                                Debug.Log($"Dealt {(int)Math.Floor((decimal)(damage / 2))} damage to {target.UnitName}");
                            }
                        }
                    }
                    else if (GetAttackSaveNone(id) == "none")
                    {
                        target.TakeDamage(damage, false);
                        Debug.Log($"Dealt {damage} damage to {target.UnitName}");
                    }                    
                }

                caster.UseResource(GetCastTime(id));

                if(GetLevel(id) != 0)
                {
                    caster.UseResource(GetSlotCost(id));
                }
            },
            (tile) =>
            {
                int maxRange = GetRange(id);

                int distance = caster.occupiedTile.CheckDistanceInTiles(tile);

                if (distance > maxRange)
                    return (false, "That target is out of range");

                if (caster.GetResource(GetCastTime(id)) <= 0)
                    return (false, "Insufficient actions");

                return (true, "");
            }
        );
        yield return null;
    }
}
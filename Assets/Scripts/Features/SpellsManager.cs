using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SpellsManager : MonoBehaviour
{
    public static SpellsManager Instance { get; private set; }

    void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //This just allows manager scripts to be stored in a folder in the editor for organization, but during runtime, get deteached to avoid errors
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from parent
        }

        //Now safe to create a new instance
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }


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
                Debug.LogError("Invalid targetype for spell id " + id);
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
        return (int)Math.Floor((decimal)(Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT radius FROM spells WHERE id = {id}")) / 5));
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
        var dice = DatabaseManager.Instance.ExecuteScalar($"SELECT dice_number FROM spells WHERE id = {id}");
        if (dice != DBNull.Value)
            return Convert.ToInt32(dice);
        else
            return 0;
    }

    public static int GetDiceSize(int id)
    {
        var size = DatabaseManager.Instance.ExecuteScalar($"SELECT dice_size FROM spells WHERE id = {id}");
        if (size != DBNull.Value)
            return Convert.ToInt32(size);
        else
            return 0;
    }

    public static bool DoesHalfOnSave(int id)
    {
        var dice = DatabaseManager.Instance.ExecuteScalar($"SELECT half_on_save FROM spells WHERE id = {id}");
        if (dice != DBNull.Value)
            return Convert.ToBoolean(dice);
        else
            return false;
    }

    public static IEnumerator CastSpell(BaseUnit caster, int id, int spellLevel, string spellcastingAbility)
    {
        string spellType;

        if (id == 34 || id == 74) //TODO: convert this to a column in the database that identifies whether a spell does damage, heals, or provides a status effect
            spellType = "heal";
        else
            spellType = "damage";

        yield return Instance.StartCoroutine(CastSpell(caster, id, spellLevel, spellcastingAbility, spellType));
    }
    
    public static IEnumerator CastSpell(BaseUnit caster, int id, int spellLevel, string spellcastingAbility, string spellType)
    {
        //TODO: Add damage scaling based on spell level, and multi-targeting

        int DC = caster.GetSaveDCForStat(spellcastingAbility);
        int spellAttack = caster.GetAttackBonusForStat(spellcastingAbility);

        Tile chosenTile = null;
        BaseUnit chosenTarget = null;
        List<BaseUnit> targets = new();

        if(GetRadius(id) > 0)
        {
            CombatGridManager.inAOERange = (caster.occupiedTile, GetRange(id), GetRadius(id), false);
        }

        if (GetTargetType(id) == TargetType.AnyTile || GetTargetType(id) == TargetType.EmptyTile)
        {
            yield return CombatStateManager.Instance.StartTileSelection(
                GetTargetType(id), caster, GetRange(id),
                (tile) => chosenTile = tile,
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

            CombatGridManager.inAOERange = (null, 0, 0, false);

            if (chosenTile == null)
                yield break;

            targets = AOEHelper.GetUnitsInRadius(chosenTile, GetRadius(id));
            // .Where(u => u.Faction == Faction.Monster).ToList(); //If it only affects enemies
        }
        else
        {
            yield return CombatStateManager.Instance.StartTargetSelection(
                GetTargetType(id), caster, GetRange(id),
                (target) => chosenTarget = target,
                (target) =>
                {
                    int maxRange = GetRange(id);

                    int distance = caster.occupiedTile.CheckDistanceInTiles(target.occupiedTile);

                    if (distance > maxRange)
                        return (false, "That target is out of range");

                    if (caster.GetResource(GetCastTime(id)) <= 0)
                        return (false, "Insufficient actions");

                    return (true, "");
                }
            );

            CombatGridManager.inAOERange = (null, 0, 0, false);

            if (chosenTarget == null)
                yield break;

            targets = AOEHelper.GetUnitsInRadius(chosenTarget.occupiedTile, GetRadius(id));
        }
        
        CombatStateManager.Instance.processing = true;

        // Debug.Log("Target tile is is " + chosenTile.tileX + ", "+chosenTile.tileY);
        // Debug.Log("Radius is " + GetRadius(id));

        // Debug.Log("Target list:");
        // foreach(BaseUnit target in targets)
        //     Debug.Log(target.UnitName);


        // var context = new ActionContext
        // {
        //     TriggeringUnit = caster,
        //     Targets = targets,
        //     Damage = 10
        // };

        int diceResult = DiceRoller.Roll(GetDiceNumber(id), GetDiceSize(id));
        // Debug.Log("Damage: " + damage);
        
        caster.UseResource(GetCastTime(id));

        if (spellLevel == 1)
            caster.UseResource("level_1_slots");
        else if(spellLevel == 2)
            caster.UseResource("level_2_slots");
        else if(spellLevel == 3)
            caster.UseResource("level_3_slots");

        yield return Instance.StartCoroutine(Instance.ProcessSpellTargets(caster, id, targets, diceResult, spellcastingAbility, DC, spellLevel, spellType));
        yield return Instance.StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));
    }

    public IEnumerator ProcessSpellTargets(BaseUnit caster, int id, List<BaseUnit> targets, int diceResult, string spellcastingAbility, int DC, int spellLevel, string spellType)
    {
        if (spellType == "damage")
        {
            foreach (BaseUnit target in targets)
            {
                // Debug.Log("unit in range: " + target.UnitName);
                if (GetAttackSaveNone(id) == "attack")
                {
                    // Debug.Log("Making attack roll");
                    bool hit = false;
                    bool crit = false;

                    yield return StartCoroutine(caster.MakeAttackWithStat(spellcastingAbility, true, target.GetAC(), (hitResult, critResult) =>
                    {
                        hit = hitResult;
                        crit = critResult;
                    }));

                    if (crit)
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{caster} rolled a natural 20 to hit {target.UnitName}"));
                        if (diceResult > 0)
                            yield return Instance.StartCoroutine(target.TakeDamage(diceResult * 2, true)); //TODO: Currently this doubles all damage, not just dice
                    }
                    else if (hit)
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{caster} hit {target.UnitName}"));
                        if (diceResult > 0)
                            yield return Instance.StartCoroutine(target.TakeDamage(diceResult, false));
                    }
                    else
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{caster} missed {target.UnitName}"));
                        // Debug.Log("Missed " + target.UnitName);
                    }
                }
                else if (GetAttackSaveNone(id) == "save")
                {
                    // Debug.Log("Making saving throw");
                    if (!target.MakeSave(GetSaveType(id), DC))
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{target.UnitName} failed their save"));
                        // Debug.Log($"{target.UnitName} failed the save");
                        yield return Instance.StartCoroutine(ApplyEffect(caster, id, target));
                        yield return Instance.StartCoroutine(target.TakeDamage(diceResult, false));
                        // Debug.Log($"Dealt {damage} damage to {target.UnitName}");
                    }
                    else
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{target.UnitName} succeeded on their saving throw"));
                        // Debug.Log($"{target.UnitName} made the save");
                        if (DoesHalfOnSave(id))
                        {
                            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{target.UnitName} still takes half damage"));

                            // Debug.Log("Spell does half damage on save");
                            yield return Instance.StartCoroutine(target.TakeDamage((int)Math.Floor((decimal)(diceResult / 2)), false));
                            // Debug.Log($"Dealt {(int)Math.Floor((decimal)(damage / 2))} damage to {target.UnitName}");
                        }
                    }
                }
                else if (GetAttackSaveNone(id) == "none")
                {
                    yield return Instance.StartCoroutine(target.TakeDamage(diceResult, false));
                    // Debug.Log($"Dealt {damage} damage to {target.UnitName}");
                }
            }
        }
        else if (spellType == "heal")
        {
            foreach (BaseUnit target in targets)
            {
                yield return Instance.StartCoroutine(target.RestoreHealth(diceResult));
            }
        }
        yield return StartCoroutine(CombatStateManager.Instance.CheckForGameOver());
    }
    
    public IEnumerator ApplyEffect(BaseUnit caster, int id, BaseUnit target)
    {
        var effect = DatabaseManager.Instance.ExecuteScalar($"SELECT spell_effect_id FROM spells WHERE id = {id}");

        if (effect == DBNull.Value)
            yield break;

        int effectID = Convert.ToInt32(effect);
        
        switch (effectID)
        {
            case 1:
                target.SetCondition("blinded", true);
                break;
            default:
                break;
        }

        yield return null;
    }
}
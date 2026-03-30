using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public Tile occupiedTile;
    public Faction Faction;
    public string UnitName;
    public int UnitID;

    public virtual void Initialize()
    {
        
    }

    public List<IReaction> Reactions = new List<IReaction>()
    {
        new OpportunityAttack()
    };

    public string GetName()
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM saved_pcs WHERE id = {UnitID}"));
    }

    public void SetName(string newName)
    {
        UnitName = newName;
    }

    public int GetCurrentHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT current_hp FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetCurrentHP(int newHP)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET current_hp = {newHP} WHERE id = {UnitID}");
    }

    public int GetTempHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT temp_hp FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetTempHP(int newTempHP)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET temp_hp = {newTempHP} WHERE id = {UnitID}");
    }

    public int GetMaxHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT max_hp FROM unit_resources WHERE id = {UnitID}"));
    }

    public int GetAC()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT AC FROM unit_stats WHERE id = {UnitID}"));
    }

    public int GetModifier(string modifier)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {modifier} FROM unit_stats WHERE id = {UnitID}"));
    }

    public int GetStat(string stat)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {stat.ToString().ToLower()} FROM unit_stats WHERE id = {UnitID}"));
    }

    public int GetProficiency(string proficiency)
    {

        bool isProficient = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT {proficiency} FROM pc_proficiencies WHERE id = {UnitID}"));

        if (isProficient)
        {
            return GetPB();
        }
        else
        {
            return 0;
        }
    }

    public int GetPB()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT proficiency FROM unit_stats WHERE id = {UnitID}"));
    }

    public int GetWeaponProficiency(int weaponID)
    {
        string category = "";
        bool light = false;
        bool finesse = false;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT stat, category, light FROM weapons WHERE id = {weaponID}",
            reader =>
            {
                category = Convert.ToString(reader["category"]);
                light = Convert.ToBoolean(reader["light"]);
                finesse = Convert.ToString(reader["stat"]) == "Finesse";
            }
        );

        if (category == "Simple Melee" || category == "Simple Ranged")
        {
            return GetProficiency("all_simple");
        }
        else if (light)
        {
            return GetProficiency("martial_light");
        }
        else if (finesse)
        {
            return GetProficiency("martial_finesse");
        }
        else
        {
            return GetProficiency("all_martial");
        }
    }

    public bool GetCondition(string condition)
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT {condition} FROM unit_conditions WHERE id = {UnitID}"));
    }

    public void SetCondition(string condition, bool status)
    {
        int value = 0;
        if (status)
        {
            value = 1;
        }
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_conditions SET {condition} = {value} WHERE id = {UnitID}");
    }

    public bool CanSwim()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT can_swim FROM unit_flags WHERE id = {UnitID}"));
    }
    
    public bool CanClimb()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT can_climb FROM unit_flags WHERE id = {UnitID}"));
    }

    public bool HasReaction()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT reaction FROM unit_resources WHERE id = {UnitID}"));
    }

    public bool UseReaction()
    {
        bool hasReaction = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT reaction FROM unit_resources WHERE id = {UnitID}"));

        if (hasReaction)
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET reaction = 0 WHERE id = {UnitID}");
        }
        else
        {
            Debug.Log("No major action available");
        }

        return hasReaction;
    }
    
    public int GetResource(string resourceName)
    {
        var resourceValue = DatabaseManager.Instance.ExecuteScalar($"SELECT {resourceName} FROM unit_resources WHERE id = {UnitID}");
        
        if(resourceValue == DBNull.Value)
        {
            return -1;
        }

        return Convert.ToInt32(resourceValue);
    }

    public bool UseResource(string resourceName)
    {
        int currentResource = GetResource(resourceName);

        if (currentResource > 0)
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET {resourceName} = {currentResource - 1} WHERE id = {UnitID}");
            return true;
        }

        CombatMenuManager.Instance.DisplayText($"Insufficent resource to use {resourceName}");
        return false;
    }

    public void SetResource(string resourceName, int newValue)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET {resourceName} = {newValue} WHERE id = {UnitID}");
    }

    public bool IsActive()
    {
        return !(GetCondition("dead") || GetCondition("dying") || GetCondition("unconscious"));
    }

    public void RefreshStartOfTurnResources()
    {
        List<(string, int)> startOfTurnResources = new List<(string, int)>()
        {
            ("major_action", 1),
            ("minor_action", 1),
            ("reaction", 1),
            ("current_speed", GetResource("base_speed")),
            ("current_number_of_attacks", GetResource("max_number_of_attacks")),
            ("frenzy_available", 1),
            ("warrior_of_the_gods_available", 1),
            ("wild_resurgence_available", 1),
            ("hand_of_healing_available", 1),
            ("horde_breaker_available", 1),
            ("flurry_of_blows_uses", 2),
            ("hand_of_healing_available", 1),
            ("horde_breaker_available", 1),
            ("tavern_brawler_push_available", 1),
            ("shield_bash_available", 1),
            ("charge_attack_available", 1),
            ("punch_and_grab_available", 1),
            ("cleave_attack_available", 1),
            ("nick_attack_available", 1)
        };

        for (int i = 0; i < startOfTurnResources.Count; i++)
        {
            if (GetResource(startOfTurnResources[i].Item1) != -1)
            {
                SetResource(startOfTurnResources[i].Item1, startOfTurnResources[i].Item2);
            }
        }
    }

    public void TakeDamage(int damage, bool wasCrit)
    {
        CombatMenuManager menu = CombatMenuManager.Instance;
        menu.DisplayText($"{UnitName} is taking {damage} damage");

        // yield return new WaitForSeconds(0.5f);

        int damageRemaining = damage;
        int currentHP = GetCurrentHP();
        int maxHP = GetMaxHP();
        int tempHP = GetTempHP();


        if (tempHP > 0 && tempHP >= damageRemaining)
        {
            tempHP -= damageRemaining;
            damageRemaining = 0;
            menu.DisplayText($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
            // Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
            // yield return new WaitForSeconds(0.5f);
        }
        else if (tempHP > 0)
        {
            damageRemaining -= tempHP;
            tempHP = 0;
            menu.DisplayText($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
            // Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
            // yield return new WaitForSeconds(0.5f);
        }
        SetTempHP(tempHP);

        // Debug.Log($"Current HP: {currentHP}");
        // Debug.Log($"Max HP: {maxHP}");
        // Debug.Log($"Damage remaining: {damageRemaining}");


        if(currentHP > 0)
        {
            if (damageRemaining < currentHP)
            {
                currentHP -= damageRemaining;
                menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}, they now have {currentHP} HP left.");
            }
            else
            {
                if(Faction == Faction.Monster)
                {
                    Die();
                }
                else
                {
                    int overflowDamage = damageRemaining - currentHP;
                    currentHP = 0;
                    if(overflowDamage >= maxHP)
                    {
                        menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}, which was enough to instantly kill them!");
                        Die();
                    }
                    else
                    {
                        menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}, which knocks them unconscious!");
                        ((BasePC)this).FallUnconscious();
                    }
                }
            }
        }
        else
        {
            if (damageRemaining >= maxHP)
            {
                menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}, which was enough to instantly kill them!");
                Die();
            }
            else if (wasCrit)
            {
                ((BasePC)this).FailDeathSave(2);
            }
            else
            {
                ((BasePC)this).FailDeathSave(1);
            }
        }

        SetCurrentHP(currentHP);
    }

    public void RestoreHealth(int amount)
    {
        if (!GetCondition("dead"))
        {
            int currentHP = GetCurrentHP();
            int maxHP = GetMaxHP();

            if (currentHP + amount >= maxHP)
            {
                currentHP = maxHP;
                CombatMenuManager.Instance.DisplayText($"You healed {UnitName} to full hit points!");
                // Debug.Log($"You healed {UnitName} to full hit points!");
            }
            else
            {
                if (currentHP == 0) //And unit is not dead
                {
                    //Clear death saves
                }

                currentHP += amount;
                CombatMenuManager.Instance.DisplayText($"You healed {UnitName} by {amount} HP!");

                // Debug.Log($"You healed {UnitName} by {amount} HP!");
            }

            CombatMenuManager.Instance.DisplayText($"{UnitName} now has {currentHP} HP.");

            // Debug.Log($"{UnitName} now has {currentHP} HP.");

            SetCurrentHP(currentHP);

            if(Faction == Faction.PC)
            {
                ((BasePC)this).ClearDeathSaves();
            }
        }
    }

    public void Die()
    {

        if (Faction == Faction.Monster)
        {
            Destroy(gameObject);
            CombatMenuManager.Instance.DisplayText($"{UnitName} has been slain!");

            // Log("Unit " + unitID + " has been slain!");
        }
        else
        {
            CombatMenuManager.Instance.DisplayText($"{UnitName} has been killed!");

            // LogWarning(characterName + " has been killed!");
        }

        SetCondition("unconscious", true);
        SetCondition("prone", true);
        SetCondition("dying", false);
        SetCondition("dead", true);

        CombatUnitManager.Instance.baseUnits.Remove(this);


        InitiativeTracker.Instance.RemoveFromInitiative(UnitID);

        occupiedTile.EmptyTile();

        CombatUnitManager.Instance.UpdateActivePCList();
        CombatUnitManager.Instance.UpdateActiveMonsterList();
        CombatStateManager.Instance.CheckForGameOver();

        if (InitiativeTracker.Instance.currentTurnUnit == this)
        {
            // EndTurn();
        }
    }

    public int RollInitiative()
    {
        int result = DiceRoller.Rolld20();

        result += GetModifier("mDEX");

        return result;
    }

    public void EndTurn()
    {
        CombatUnitManager.Instance.SelectedPC?.OnTurnEnded?.Invoke();

        if (InitiativeTracker.Instance.currentTurnUnit.Faction == Faction.PC)
            CombatMenuManager.Instance.CloseAllMenus();
    }

    public IEnumerator PullTarget(BaseUnit user, BaseUnit target, int pullDistance)
    {

        for (int i = 0; i < pullDistance; i++)
        {
            Vector2Int userPos = new Vector2Int(user.occupiedTile.tileX, user.occupiedTile.tileY);
            Vector2Int targetPos = new Vector2Int(target.occupiedTile.tileX, target.occupiedTile.tileY);

            Vector2Int delta = targetPos - userPos;
            
            Vector2Int direction = new Vector2Int(
                Mathf.Clamp(delta.x, -1, 1),
                Mathf.Clamp(delta.y, -1, 1)
            );
            
            Vector2Int next = targetPos - direction;

            Tile nextTile = CombatGridManager.Instance.GetTileAtPosition(next);

            if (!nextTile.isWalkable || target.occupiedTile.CheckDistanceInTiles(user.occupiedTile) == 1) break;

            yield return StartCoroutine(nextTile.MoveUnit(target, true));
        }
    }
    
}

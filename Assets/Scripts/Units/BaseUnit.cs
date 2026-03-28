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

    public string GetName()
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM saved_pcs WHERE id = {UnitID}"));
    }

    public void SetName(string newName)
    {
        // DatabaseManager.Instance.ExecuteScalar($"UPDATE saved_pcs SET name = '{newName}' WHERE id = {UnitID}");
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


        if (currentHP > damageRemaining && currentHP > 0)
        {
            currentHP -= damageRemaining;
            menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}, they now have {currentHP} HP left.");
            // Debug.Log($"The attacker dealt {damageRemaining} damage to {UnitName}, they now have {currentHP} HP left.");
            // yield return new WaitForSeconds(0.5f);
        }
        else if (currentHP > 0) //If the damage remaining is greater than the unit's current HP, but they aren't unconscious yet
        {
            currentHP = 0;
            if (Faction == Faction.Monster)
            {
                menu.DisplayText($"The attacker dealt {damageRemaining} damage to {UnitName}.");
                Die();
            }
            else
            {
                int damageDealt = damageRemaining;
                damageRemaining -= currentHP;
                if (damageRemaining >= maxHP)
                {
                    
                    menu.DisplayText($"The attacker dealt {damageDealt} damage to {UnitName}, which was enough to instantly kill them!");

                    // Debug.Log($"The attacker dealt {damageDealt} damage to {UnitName}, which was enough to instantly kill them!");
                    Die();
                }
                else
                {
                    currentHP = 0;
                    menu.DisplayText($"The attacker dealt {damageDealt} damage to {UnitName}, which knocks them unconscious!");
                    // Debug.Log($"The attacker dealt {damageDealt} damage to {UnitName}, which knocks them unconscious!");
                    FallUnconscious();
                }
            }
        }
        else
        {
            if (damageRemaining >= maxHP)
            {
                // Debug.Log($"The attacker dealt {damageRemaining} damage to {UnitName}, which was enough to instantly kill them!");
                Die();
            }
            else if (wasCrit)
            {
                FailDeathSave(2);
            }
            else
            {
                FailDeathSave(1);
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

            ClearDeathSaves();
        }
    }

    public IEnumerator MakeDeathSave()
    {
        if (GetCondition("dying"))
        {
            CombatMenuManager.Instance.DisplayText($"{UnitName} is dying");
            yield return new WaitForSeconds(1f);
            int result = DiceRoller.Rolld20(false, false);
            yield return new WaitForSeconds(1f);

            if (result == 1)
            {
                FailDeathSave(2);
            }
            else if (result < 10)
            {
                FailDeathSave(1);
            }
            else if (result == 20)
            {
                RestoreHealth(1);
            }
            else
            {
                PassDeathSave(1);
            }
        }
    }


    public void FailDeathSave(int number)
    {
        int currentFails = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT death_save_fails FROM unit_resources WHERE id = {UnitID}"));
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET death_save_fails = {currentFails + number} WHERE id = {UnitID}");
        CheckForDeath(currentFails);
    }

    public void PassDeathSave(int number)
    {
        int currentPasses = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT death_save_successes FROM unit_resources WHERE id = {UnitID}"));
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET death_save_successes = {currentPasses + number} WHERE id = {UnitID}");
        CheckForStable(currentPasses);
    }

    public void CheckForDeath(int currentFails)
    {
        if (currentFails >= 3)
        {
            Die();
        }
    }

    public void CheckForStable(int currentPasses)
    {
        if (currentPasses >= 3)
        {
            SetCondition("dying", false);
        }
    }

    public void ClearDeathSaves()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET death_save_successes = 0, death_save_fails = 0 WHERE id = {UnitID}");
        SetCondition("dying", false);
        SetCondition("unconscious", false);
    }

    public void FallUnconscious()
    {

        if (Faction == Faction.Monster)
        {
            Debug.LogWarning("Tried to knock a monster unconscious, killing unit instead.");
            Die();
        }
        else
        {
            SetCondition("unconscious", true);
            SetCondition("prone", true);
            SetCondition("dying", true);
            CombatMenuManager.Instance.DisplayText($"{UnitName} is dying!");
            // Log($"{unit.UnitName} is dying!");
        }
        
        CombatStateManager.Instance.CheckForGameOver();

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

        if(InitiativeTracker.Instance.currentTurnUnit == this)
        {
            InitiativeTracker.Instance.EndTurn();
        }

        InitiativeTracker.Instance.RemoveFromInitiative(UnitID);

        occupiedTile.EmptyTile();

        CombatUnitManager.Instance.UpdateActivePCList();
        CombatUnitManager.Instance.UpdateActiveMonsterList();
        CombatStateManager.Instance.CheckForGameOver();
    }

    public int RollInitiative()
    {
        int result = DiceRoller.Rolld20();

        result += GetModifier("mDEX");

        return result;
    }
    
}

using System;
using Unity.Profiling;
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

    public string SetName(string newName)
    {
        return Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM saved_pcs WHERE id = {UnitID}"));
    }

    public int GetCurrentSpeed()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT current_speed FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetCurrentSpeed(int newValue)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET current_speed = {newValue} WHERE id = {UnitID}");
    }

    public int GetBaseSpeed()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT base_speed FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetBaseSpeed(int newValue)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET base_speed = {newValue} WHERE id = {UnitID}");
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
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT max_hp FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetTempHP(int newTempHP)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET temp_hp = {newTempHP} WHERE id = {UnitID}");
    }

    public int GetMaxHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT max_hp FROM unit_resources WHERE id = {UnitID}"));
    }

    public void SetMaxHP(int newHP)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET max_hp = {newHP} WHERE id = {UnitID}");
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
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {stat.ToLower()} FROM unit_stats WHERE id = {UnitID}"));
    }

    public int GetProficiency(string proficiency)
    {

        bool isProficient = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT {proficiency} FROM pc_proficiencies WHERE id = {UnitID}"));

        if (isProficient)
        {
            return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT proficiency FROM unit_stats WHERE id = {UnitID}"));
        }
        else
        {
            return 0;
        }
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

    public bool UseMajorAction()
    {
        bool hasMajor = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT major_action FROM unit_resources WHERE id = {UnitID}"));

        if (hasMajor)
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET major_action = 0 WHERE id = {UnitID}");
        }
        else
        {
            Debug.Log("No major action available");
        }

        return hasMajor;
    }

    public bool UseMinorAction()
    {
        bool hasMinor = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT minor_action FROM unit_resources WHERE id = {UnitID}"));

        if (hasMinor)
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET minor_action = 0 WHERE id = {UnitID}");
        }
        else
        {
            Debug.Log("No major action available");
        }

        return hasMinor;
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
    
    public void RefreshSpeed()
    {
        SetCurrentSpeed(GetBaseSpeed());
    }

    public void RefreshActions()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_resources SET major_action = 1, minor_action = 1, reaction = 1 WHERE id = {UnitID}");
    }

    public void TakeDamage(int damage, bool wasCrit)
    {
        int damageRemaining = damage;
        int currentHP = GetCurrentHP();
        int maxHP = GetMaxHP();
        int tempHP = GetTempHP();


        if (tempHP > 0 && tempHP >= damageRemaining)
        {
            tempHP -= damageRemaining;
            damageRemaining = 0;
            Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
        }
        else if (tempHP > 0)
        {
            damageRemaining -= tempHP;
            tempHP = 0;
            Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
        }
        SetTempHP(tempHP);


        if (currentHP > damageRemaining && currentHP > 0)
        {
            currentHP -= damageRemaining;
            Debug.Log($"The attacker dealt {damageRemaining} damage to {UnitName}, they now have {currentHP} HP left.");
        }
        else if (currentHP > 0) //If the damage remaining is greater than the unit's current HP, but they aren't unconscious yet
        {
            currentHP = 0;
            if (Faction == Faction.Monster)
            {
                CombatUnitManager.Instance.KillUnit(UnitID);
            }
            else
            {
                int damageDealt = damageRemaining;
                damageRemaining -= currentHP;
                if (damageRemaining >= maxHP)
                {
                    //Unit dies
                    Debug.Log($"The attacker dealt {damageDealt} damage to {UnitName}, which was enough to instantly kill them!");
                    CombatUnitManager.Instance.KillUnit(UnitID);
                }
                else
                {
                    currentHP = 0;
                    Debug.Log($"The attacker dealt {damageDealt} damage to {UnitName}, which knocks them unconscious!");
                    CombatUnitManager.Instance.FallUnconscious(UnitID);
                }
            }
        }
        else
        {
            if (damageRemaining >= maxHP)
            {
                Debug.Log($"The attacker dealt {damageRemaining} damage to {UnitName}, which was enough to instantly kill them!");
                CombatUnitManager.Instance.KillUnit(UnitID);
                //Unit dies
            }
            else if (wasCrit)
            {
                //Unit fails two death saves
                //Check for death
            }
            else
            {
                //Unit fails one death save
                //Check for death
            }
        }

        SetCurrentHP(currentHP);
    }
    
    public void RestoreHealth(int amount)
    {
        int currentHP = GetCurrentHP();
        int maxHP = GetMaxHP();

        if (currentHP + amount >= maxHP)
        {
            currentHP = maxHP;
            Debug.Log($"You healed {UnitName} to full hit points!");
        }
        else
        {
            if (currentHP == 0) //And unit is not dead
            {
                //Clear death saves
            }

            currentHP += amount;
            Debug.Log($"You healed {UnitName} by {amount} HP!");
        }

        Debug.Log($"{UnitName} now has {currentHP} HP.");

        SetCurrentHP(currentHP);
    }

    
}

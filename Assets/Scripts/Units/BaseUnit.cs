using System;
using System.Collections;
using System.Collections.Generic;
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
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT current_hp FROM unit_info WHERE id = {UnitID}"));
    }

    public void SetCurrentHP(int newHP)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET current_hp = {newHP} WHERE id = {UnitID}");
    }

    public int GetTempHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT temp_hp FROM unit_info WHERE id = {UnitID}"));
    }

    public void SetTempHP(int newTempHP)
    {
        int currentTempHP = GetTempHP();
        if(newTempHP > currentTempHP)
        {
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET temp_hp = {newTempHP} WHERE id = {UnitID}");
        }
    }

    public int GetMaxHP()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT max_hp FROM unit_info WHERE id = {UnitID}"));
    }

    public int GetAC()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT AC FROM unit_info WHERE id = {UnitID}"));
    }

    public int GetSaveDCForStat(string stat)
    {
        switch (stat)
        {
            case "STR":
                return GetStat("str_dc");
            case "DEX":
                return GetStat("dex_dc");
            case "CON":
                return GetStat("con_dc");
            case "INT":
                return GetStat("int_dc");
            case "WIS":
                return GetStat("wis_dc");
            case "CHA":
                return GetStat("cha_dc");
            default:
                Debug.LogError("Tried to retrieve an invalid stat for " + stat);
                return 0;
        }
    }

    public int GetAttackBonusForStat(string stat)
    {
        switch (stat)
        {
            case "STR":
                return GetStat("mSTR") + GetPB();
            case "DEX":
                return GetStat("mDEX") + GetPB();
            case "CON":
                return GetStat("mCON") + GetPB();
            case "INT":
                return GetStat("mINT") + GetPB();
            case "WIS":
                return GetStat("mWIS") + GetPB();
            case "CHA":
                return GetStat("mCHA") + GetPB();
            default:
                Debug.LogError("Tried to retrieve an invalid stat for " + stat);
                return 0;
        }
    }

    public int GetModifier(string modifier)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {modifier} FROM unit_info WHERE id = {UnitID}"));
    }

    public int GetStat(string stat)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT {stat} FROM unit_info WHERE id = {UnitID}"));
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
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT proficiency FROM unit_info WHERE id = {UnitID}"));
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
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT {condition} FROM unit_info WHERE id = {UnitID}"));
    }

    public void SetCondition(string condition, bool status)
    {
        int value = 0;
        if (status)
        {
            value = 1;
        }
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET {condition} = {value} WHERE id = {UnitID}");
        CombatMenuManager.Instance.ReRenderMenu();
    }

    public bool CanSwim()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT can_swim FROM unit_info WHERE id = {UnitID}"));
    }
    
    public bool CanClimb()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT can_climb FROM unit_info WHERE id = {UnitID}"));
    }

    public bool HasReaction()
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT reaction FROM unit_info WHERE id = {UnitID}"));
    }

    // public bool UseReaction()
    // {
    //     bool hasReaction = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT reaction FROM unit_info WHERE id = {UnitID}"));

    //     if (hasReaction)
    //     {
    //         DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET reaction = 0 WHERE id = {UnitID}");
    //     }
    //     else
    //     {
    //         // Debug.Log("No reaction available");
    //     }

    //     return hasReaction;
    // }
    
    public int GetResource(string resourceName)
    {
        var resourceValue = DatabaseManager.Instance.ExecuteScalar($"SELECT {resourceName} FROM unit_info WHERE id = {UnitID}");
        
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
            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET {resourceName} = {currentResource - 1} WHERE id = {UnitID}");
            CombatMenuManager.Instance.ReRenderMenu();

            return true;
        }

        Debug.Log($"{UnitName} has Insufficent resource to use {resourceName}");
        return false;
    }

    public void SetResource(string resourceName, int newValue)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE unit_info SET {resourceName} = {newValue} WHERE id = {UnitID}");
        CombatMenuManager.Instance.ReRenderMenu();
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

    public IEnumerator TakeDamage(int damage, bool wasCrit)
    {
        // Debug.LogWarning("Called TakeDamage()");

        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} is taking {damage} damage"));

        int damageRemaining = damage;
        int currentHP = GetCurrentHP();
        int maxHP = GetMaxHP();
        int tempHP = GetTempHP();

        if (tempHP > 0 && tempHP >= damageRemaining)
        {
            tempHP -= damageRemaining;
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage, they have {tempHP} temp HP left"));
            SetTempHP(tempHP);
            yield break;
            // Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
        }
        else if (tempHP > 0)
        {
            damageRemaining -= tempHP;
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} lost their {tempHP} temp HP"));
            SetTempHP(0);
            // Debug.Log($"{UnitName} had {GetTempHP()} temp HP, they now have {tempHP}");
        }

        // Debug.Log($"Current HP: {currentHP}");
        // Debug.Log($"Max HP: {maxHP}");
        // Debug.Log($"Damage remaining: {damageRemaining}");


        if(currentHP > 0)
        {
            if (damageRemaining < currentHP)
            {
                currentHP -= damageRemaining;
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage, they now have {currentHP} HP left."));
            }
            else
            {
                if(Faction == Faction.Monster)
                {
                    // Debug.LogWarning("About to call Die()");
                    yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage"));
                    yield return StartCoroutine(Die());
                    // Debug.LogWarning("Returned from Die()");
                }
                else
                {
                    int overflowDamage = damageRemaining - currentHP;
                    currentHP = 0;
                    if(overflowDamage >= maxHP)
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage, which is enough to instantly kill them"));
                        yield return StartCoroutine(Die());
                    }
                    else
                    {
                        yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage"));
                        yield return StartCoroutine(((BasePC)this).FallUnconscious());
                    }
                }
            }
        }
        else
        {
            if (damageRemaining >= maxHP)
            {
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took {damageRemaining} damage, which is enough to instantly kill them"));
                yield return StartCoroutine(Die());
            }
            else if (wasCrit)
            {
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} was critically hit, they fail 2 death saves"));
                yield return StartCoroutine(((BasePC)this).FailDeathSave(2));
            }
            else
            {
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} took damage, they fail a death save"));
                yield return StartCoroutine(((BasePC)this).FailDeathSave(1));
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
                StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} was healed to full hit points (Current HP: {currentHP})"));
                // Debug.Log($"You healed {UnitName} to full hit points!");
            }
            else
            {
                if (currentHP == 0) //And unit is not dead
                {
                    //Clear death saves
                }

                currentHP += amount;
                StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} healed {amount} HP (Current HP: {currentHP})"));

                // Debug.Log($"You healed {UnitName} by {amount} HP!");
            }

            // Debug.Log($"{UnitName} now has {currentHP} HP.");

            SetCurrentHP(currentHP);

            if (Faction == Faction.PC)
            {
                ((BasePC)this).ClearDeathSaves();
            }
        }
    }

    public bool MakeSave(string saveType, int DC)
    {
        bool result = false;
        saveType = saveType.ToUpper();
        int saveModifier = 0;
        int savePB = 0;

        switch (saveType)
        {
            case "STR":
                saveModifier = GetStat("mSTR");
                savePB = GetProficiency("str_save");
                break;
            case "DEX":
                saveModifier = GetStat("mDEX");
                savePB = GetProficiency("dex_save");
                break;
            case "CON":
                saveModifier = GetStat("mCON");
                savePB = GetProficiency("con_save");
                break;
            case "INT":
                saveModifier = GetStat("mINT");
                savePB = GetProficiency("int_save");
                break;
            case "WIS":
                saveModifier = GetStat("mWIS");
                savePB = GetProficiency("wis_save");
                break;
            case "CHA":
                saveModifier = GetStat("mCHA");
                savePB = GetProficiency("cha_save");
                break;
            default:
                Debug.LogError("Tried to make an invalid save for " + saveType);
                break;
        }

        int dieRoll = DiceRoller.Rolld20();

        if (dieRoll + saveModifier + savePB >= DC)
        {
            //Prompt for reaction
            result = true;
        }
        else
        {
            //Prompt for reaction
            result = false;
        }

        StartCoroutine(CombatMenuManager.Instance.DisplayDiceRoll(this, dieRoll, saveModifier, savePB, result));

        return result;
    }

    public void Dash()
    {
        if (UseResource("major_action"))
        {
            CombatActions.Dash(this);
            if(Faction == Faction.PC)
            {
                StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.MovingPC));
                CombatMenuManager.Instance.ReRenderMenu();                
            }
        }
        else
        {
            StartCoroutine(CombatMenuManager.Instance.DisplayText("No action available"));
        }
    }

    public void Disengage()
    {
        if (UseResource("major_action"))
        {
            CombatActions.Disengage(this);
        }
        else
        {
            StartCoroutine(CombatMenuManager.Instance.DisplayText("No action available"));
        }
    }

    public void Dodge()
    {
        if (UseResource("major_action"))
        {
            CombatActions.Dodge(this);
        }
        else
        {
            StartCoroutine(CombatMenuManager.Instance.DisplayText("No action available"));
        }
    }
    
    public IEnumerator MakeAttackWithStat(
        string stat,
        bool proficient,
        int AC,
        Action<bool, bool> onComplete)
    {
        int hitMod = 0;
        int hitPB = 0;
        if (proficient) hitPB = GetPB();
        bool crit = false;
        int total = 0;

        int dieRoll = DiceRoller.Rolld20();

        switch (stat)
        {
            case "STR": hitMod = GetStat("mSTR"); break;
            case "DEX": hitMod = GetStat("mDEX"); break;
            case "CON": hitMod = GetStat("mCON"); break;
            case "INT": hitMod = GetStat("mINT"); break;
            case "WIS": hitMod = GetStat("mWIS"); break;
            case "CHA": hitMod = GetStat("mCHA"); break;
            default:
                Debug.LogError("Invalid attack stat: " + stat);
                yield break;
        }

        if (dieRoll == 20)
            crit = true;

        total += dieRoll + hitMod + hitPB;

        bool result = crit || total > AC;

        yield return StartCoroutine(CombatMenuManager.Instance.DisplayDiceRoll(this, dieRoll, hitMod, hitPB, result));

        if (result)
            onComplete?.Invoke(true, crit);
        else
            onComplete?.Invoke(false, false);
    }

    public IEnumerator Die()
    {
        SetCondition("unconscious", true);
        SetCondition("prone", true);
        SetCondition("dying", false);
        SetCondition("dead", true);

        yield return new WaitForSeconds(0.1f); // let coroutines finish

        // Debug.LogWarning("Called Die()");
        if (Faction == Faction.Monster)
        {
            // Debug.LogWarning($"{UnitName} is about to die");
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} has been slain!"));
            // Destroy(gameObject);

            // Log("Unit " + unitID + " has been slain!");
        }
        else
        {
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{UnitName} has been killed."));
            // LogWarning(characterName + " has been killed!");
        }


        // Debug.LogWarning($"Removing {UnitName} from BaseUnits");
        CombatUnitManager.Instance.baseUnits.Remove(this);

        // Debug.LogWarning($"Removing {UnitName} from initiative");
        InitiativeTracker.Instance.RemoveFromInitiative(UnitID);

        if (InitiativeTracker.Instance.currentTurnUnit == this)
        {
            // Debug.LogWarning("Unit died on their turn, ending turn");
            EndTurn();
        }

        // Debug.LogWarning($"Emptying {UnitName} from tile");
        occupiedTile.EmptyTile();

        // Debug.LogWarning($"Updating active unit lists");
        CombatUnitManager.Instance.UpdateActivePCList();
        CombatUnitManager.Instance.UpdateActiveMonsterList();

        // Debug.LogWarning($"Waiting for gameover");
        // yield return new WaitForSeconds(0.5f);
        // Debug.LogWarning($"Checking for gameover");
        // StartCoroutine(CombatStateManager.Instance.CheckForGameOver());
        // Debug.Log("Made it to the end of Die()");
    }

    public int RollInitiative()
    {
        int result = DiceRoller.Rolld20();

        result += GetModifier("mDEX");

        return result;
    }

    public void EndTurn()
    {

        if (InitiativeTracker.Instance.currentTurnUnit.Faction == Faction.PC)
        {
            CombatMenuManager.Instance.CloseAllMenus();
            CombatUnitManager.Instance.SelectedPC?.OnTurnEnded?.Invoke();
        }
        if (InitiativeTracker.Instance.currentTurnUnit.Faction == Faction.Monster)
        {
            
        }

        // InitiativeTracker.Instance.AdvanceTurn();
        
    }

    public IEnumerator PullTarget(BaseUnit target, int pullDistance)
    {
        for (int i = 0; i < pullDistance; i++)
        {
            Vector2Int userPos = new Vector2Int(occupiedTile.tileX, occupiedTile.tileY);
            Vector2Int targetPos = new Vector2Int(target.occupiedTile.tileX, target.occupiedTile.tileY);

            Vector2Int delta = targetPos - userPos;

            Vector2Int direction = new Vector2Int(
                Mathf.Clamp(delta.x, -1, 1),
                Mathf.Clamp(delta.y, -1, 1)
            );

            Vector2Int next = targetPos - direction;

            Tile nextTile = CombatGridManager.Instance.GetTileAtPosition(next);

            if (!nextTile.isWalkable || target.occupiedTile.CheckDistanceInTiles(occupiedTile) == 1) break;

            yield return StartCoroutine(nextTile.MoveUnit(target, true));
        }
    }

    public IEnumerator PushTarget(BaseUnit target, int pushDistance)
    {
        Vector2Int userPos = new Vector2Int(occupiedTile.tileX, occupiedTile.tileY);
        Vector2Int targetPos = new Vector2Int(target.occupiedTile.tileX, target.occupiedTile.tileY);

        Vector2Int delta = targetPos - userPos;

        Vector2Int direction = new Vector2Int(
            Mathf.Clamp(delta.x, -1, 1),
            Mathf.Clamp(delta.y, -1, 1)
        );

        // Vector2Int newTargetPos = targetPos + direction * pushDistance;

        Vector2Int currentPos = targetPos;

        for (int i = 0; i < pushDistance; i++)
        {
            Vector2Int next = currentPos + direction;

            Tile nextTile = CombatGridManager.Instance.GetTileAtPosition(next);

            if (nextTile == null || !nextTile.isWalkable) break;

            yield return StartCoroutine(nextTile.MoveUnit(target, true));
            currentPos = next;
        }
    }

    public void FallProne()
    {
        SetCondition("prone", true);
    }
    
    public void StandUp()
    {
        int movementCost = GetResource("base_speed") / 2;
        if (GetResource("current_speed") >= movementCost)
        {
            SetCondition("prone", false);
            SetResource("current_speed", GetResource("current_speed") - movementCost);
        }
        else
        {
            Debug.Log("Not enough speed to stand up");
        }
    }
    
}

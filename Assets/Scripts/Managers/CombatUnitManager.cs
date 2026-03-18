using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using TMPro.Examples;
using UnityEngine;
using System;
using static UnityEngine.Debug;
using Unity.VisualScripting;


public class CombatUnitManager : MonoBehaviour
{
    private List<ScriptableUnit> units;
    private List<BaseUnit> baseUnits = new List<BaseUnit>();
    public BasePC SelectedPC; //We only ever want a PC to be actionable
    public static CombatUnitManager Instance;

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

        //Goes into resources folder, goes into units folder, look into all subfolders for all types of scriptable units and put them into this list
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnPCs(int encounterID)
    {
        int numberOfPCs = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT COUNT(*) FROM grid_contents WHERE encounter_id = {encounterID} AND unit_id IN (0,1,2,3,4)"
            ));

        for (int i = 0; i < numberOfPCs; i++) //For each PC
        {
            ScriptableUnit pcToSpawn = units.FirstOrDefault(u => u.UnitID == i); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

            if (pcToSpawn == null)
            {
                Log($"No ScriptableUnit found with UnitID {i}");
            }
            if (pcToSpawn.UnitPrefab == null)
            {
                LogError($"ScriptableUnit {pcToSpawn.UnitPrefab} has no prefab assigned!");
                return;
            }

            var spawnedPC = Instantiate(pcToSpawn.UnitPrefab); //Creates the PC unit
            UpdatePCName(spawnedPC);
            baseUnits.Add(spawnedPC);
            
            var sr = spawnedPC.GetComponent<SpriteRenderer>();
            if (sr != null && pcToSpawn.UnitSprite != null)
                sr.sprite = pcToSpawn.UnitSprite;


            var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(i, encounterID); //Gets which tile that PC is supposed to spawn on

            spawnTile.SetUnit(spawnedPC); //Set that spawn tile's unit to the spawned PC
        }

        CombatStateManager.Instance.ChangeState(GameState.SpawnMonsters);
    }

    private void UpdatePCName(BaseUnit pc)
    {
        string savedName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT name FROM saved_pcs WHERE id = {pc.UnitID}"
            ));
        pc.UnitName = savedName;
    }

    public void SpawnMonsters(int encounterID)
    {
        int enemyCount = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT COUNT(*) FROM grid_contents WHERE encounter_id = {encounterID} AND unit_id > 4"
        ));
        
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT unit_id FROM grid_contents WHERE encounter_id = {encounterID} AND unit_id > 4",
            reader =>
            {
                while (reader.Read())
                {
                    int monsterID = Convert.ToInt32(reader["unit_id"]);

                    ScriptableUnit monsterToSpawn = units.FirstOrDefault(u => u.UnitID == monsterID); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

                    if (monsterToSpawn == null)
                    {
                        Log($"No ScriptableUnit found with UnitID {monsterID}");
                    }
                    if (monsterToSpawn.UnitPrefab == null)
                    {
                        LogError($"ScriptableUnit {monsterToSpawn.UnitPrefab} has no prefab assigned!");
                        return;
                    }

                    var spawnedMonster = Instantiate(monsterToSpawn.UnitPrefab); //Creates the monster unit
                    baseUnits.Add(spawnedMonster);

                    var sr = spawnedMonster.GetComponent<SpriteRenderer>();
                    if (sr != null && monsterToSpawn.UnitSprite != null)
                        sr.sprite = monsterToSpawn.UnitSprite;

                    var spawnTile = CombatGridManager.Instance.GetMonsterSpawnTile(monsterID, encounterID); //Gets which tile that PC is supposed to spawn on

                    spawnTile.SetUnit(spawnedMonster); //Set that spawn tile's unit to the spawned PC

                }
            }
        );

        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    public void SetSelectedPC(BasePC pc)
    {
        SelectedPC = pc;
        CombatMenuManager.Instance.ShowSelectedPC(pc);
    }

    public void RefreshUnitSpeed(int unitID)
    {
        BaseUnit unit = baseUnits.FirstOrDefault(u => u.UnitID == unitID); //Get the unit object from the list of units
        if (unit == null)
        {
            Log($"No unit found with UnitID {unitID}");
        }

        int maxSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT base_speed FROM unit_resources WHERE id = {unitID}"
        ));

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE unit_resources SET current_speed = {maxSpeed} WHERE id = {unitID}"
        );
    }

    public void RefreshUnitActions(int unitID)
    {
        BaseUnit unit = baseUnits.FirstOrDefault(u => u.UnitID == unitID); //Get the unit object from the list of units
        if (unit == null)
        {
            Log($"No unit found with UnitID {unitID}");
        }

        DatabaseManager.Instance.ExecuteNonQuery(
        $"UPDATE unit_resources SET major_action = 1, minor_action = 1, reaction = 1 WHERE id = {unitID}"
        );
    }

    public void DamageUnit(int unitID, int damage, bool wasCrit)
    {
        int damageRemaining = damage;
        int currentHP = 0;
        int tempHP = 0;
        int maxHP = 0;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT current_hp, temp_hp, max_hp FROM unit_resources WHERE id = {unitID}",
            reader =>
            {
                while (reader.Read())
                {
                    currentHP = Convert.ToInt32(reader["current_hp"]);
                    tempHP = Convert.ToInt32(reader["temp_hp"]);
                    maxHP = Convert.ToInt32(reader["max_hp"]);
                }
            }
        );

        if (tempHP > 0 && tempHP >= damageRemaining)
        {
            Log("The target had "+tempHP+" temp HP.");
            tempHP -= damageRemaining;
            damageRemaining = 0;
            Log("They now have "+tempHP+" left.");
        }
        else if(tempHP > 0)
        {
            Log("The target had "+tempHP+" temp HP, which is now all gone.");
            damageRemaining -= tempHP;
            tempHP = 0;
        }

        if (currentHP > 0 && currentHP > damageRemaining)
        {
            currentHP -= damageRemaining;
            Log("The attacker dealt " + damageRemaining + " damage to the target, who now has " + currentHP + " HP left.");
        }
        else if (currentHP > 0)
        {
            int damageDealt = damageRemaining;
            damageRemaining -= currentHP;
            if (damageRemaining >= maxHP)
            {
                //Unit dies
                Log("The attacker dealt " + damageDealt + " damage to the target, which was enough to instantly kill them!");
            }
            else
            {
                currentHP = 0;
                Log("The attacker dealt " + damageDealt + " damage to the target, which knocks them unconscious!");
            }
        }
        else
        {
            KillUnit(unitID);
            if (damageRemaining >= maxHP)
            {
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
        

        // Log("Unit's current HP is: " + currentHP);


        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE unit_resources SET temp_hp = {tempHP}, current_hp = {currentHP} WHERE id = {unitID}"
        );
    }

    public void HealUnit(int unitID, int healing)
    {
        int currentHP = 0;
        int maxHP = 0;
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT current_hp, max_hp FROM unit_resources WHERE id = {unitID}",
            reader =>
            {
                while (reader.Read())
                {
                    currentHP = Convert.ToInt32(reader["current_hp"]);
                    maxHP = Convert.ToInt32(reader["max_hp"]);
                }
            }
        );

        if (currentHP + healing >= maxHP)
        {
            currentHP = maxHP;
            Log("You healed the target to full hit points!");
        }
        else
        {
            if (currentHP == 0) //And unit is not dead
            {
                //Clear death saves
            }

            currentHP += healing;
            Log("You healed the target by " + healing + " HP!");
        }

        Log("Unit's current HP is: " + currentHP);

        DatabaseManager.Instance.ExecuteNonQuery(
            $"UPDATE unit_resources SET current_hp = {currentHP} WHERE id = {unitID}"
        );
    }
    
    public void KillUnit(int unitID)
    {
        BaseUnit unit = baseUnits.FirstOrDefault(u => u.UnitID == unitID);
        
        if (unit.Faction == Faction.Monster)
        {
            Destroy(unit.gameObject);
            Log("Unit " + unitID + " has been slain!");
        }
        else
        {
            string characterName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT name FROM saved_pcs WHERE id = {unitID}"
            ));
            Log(characterName + " has fallen unconscious!");
        }

        unit.occupiedTile.EmptyTile();

    }

    public int GetAC(int unitID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT AC FROM unit_stats WHERE id = {unitID}"
            ));
    }

    public int GetProficiency(int unitID, string proficiency)
    {
        Log("Getting " + proficiency + " proficiency for unit " + unitID);
        bool isProficient = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT {proficiency} FROM pc_proficiencies WHERE id = {unitID}"
        ));

        if (isProficient)
        {
            return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT proficiency FROM unit_stats WHERE id = {unitID}"
            ));
        }

        Log("Not proficient");
        return 0;
    }
    
    public BaseUnit GetUnitByID(int id)
    {
        return baseUnits.FirstOrDefault(u => u.UnitID == id);
    }

}

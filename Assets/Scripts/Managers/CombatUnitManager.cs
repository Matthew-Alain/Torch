using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using TMPro.Examples;
using UnityEngine;
using System;
using static UnityEngine.Debug;


public class CombatUnitManager : MonoBehaviour
{
    private List<ScriptableUnit> units;
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
                "SELECT COUNT(*) FROM grid_contents WHERE encounter_id = @encounter_id AND content IN (0,1,2,3,4)",
                ("@encounter_id", encounterID)
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
                Debug.LogError($"ScriptableUnit {pcToSpawn.UnitPrefab} has no prefab assigned!");
                return;
            }

            UpdatePCPrefab(pcToSpawn);

            var spawnedPC = Instantiate(pcToSpawn.UnitPrefab); //Creates the PC unit
            var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(i, encounterID); //Gets which tile that PC is supposed to spawn on

            spawnTile.SetUnit(spawnedPC); //Set that spawn tile's unit to the spawned PC
        }

        CombatStateManager.Instance.ChangeState(GameState.SpawnMonsters);
    }

    private void UpdatePCPrefab(ScriptableUnit pc)
    {
        string savedName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar(
                "SELECT name FROM saved_pcs WHERE id = (@PCID)",
                ("@PCID", pc.UnitID)
            ));
        pc.UnitPrefab.UnitName = savedName;
    }

    public void SpawnMonsters(int encounterID)
    {
        int enemyCount = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT COUNT(*) FROM grid_contents WHERE encounter_id = @encounter_id AND content > 4",
            ("@encounter_id", encounterID)
        ));
        
        DatabaseManager.Instance.ExecuteReader(
            "SELECT content FROM grid_contents WHERE encounter_id = @encounter_id AND content > 4",
            reader =>
            {
                while (reader.Read())
                {
                    int monsterID = Convert.ToInt32(reader["content"]);

                    ScriptableUnit monsterToSpawn = units.FirstOrDefault(u => u.UnitID == monsterID); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

                    if (monsterToSpawn == null)
                    {
                        Log($"No ScriptableUnit found with UnitID {monsterID}");
                    }
                    if (monsterToSpawn.UnitPrefab == null)
                    {
                        Debug.LogError($"ScriptableUnit {monsterToSpawn.UnitPrefab} has no prefab assigned!");
                        return;
                    }

                    var spawnedMonster = Instantiate(monsterToSpawn.UnitPrefab); //Creates the PC unit
                    var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(monsterID, encounterID); //Gets which tile that PC is supposed to spawn on

                    spawnTile.SetUnit(spawnedMonster); //Set that spawn tile's unit to the spawned PC

                }
            },
            ("@encounter_id", encounterID)
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
        ScriptableUnit unit = units.FirstOrDefault(u => u.UnitID == unitID); //Get the unit object from the list of units
        if (unit == null)
        {
            Log($"No ScriptableUnit found with UnitID {unitID}");
        }
        if (unit.UnitPrefab == null)
        {
            LogError($"ScriptableUnit {unit.UnitPrefab} has no prefab assigned!");
            return;
        }

        int maxSpeed = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            "SELECT max_speed FROM unit_resources WHERE id = (@unitID)",
            ("@unitID", unitID)
        ));

        DatabaseManager.Instance.ExecuteNonQuery(
        "UPDATE unit_resources SET current_speed = (@newSpeed) WHERE id = @unitID",
            ("@newSpeed", maxSpeed),
            ("@unitID", unitID)
        );
    }

    public void RefreshUnitActions(int unitID)
    {
        ScriptableUnit unit = units.FirstOrDefault(u => u.UnitID == unitID); //Get the unit object from the list of units
        if (unit == null)
        {
            Log($"No ScriptableUnit found with UnitID {unitID}");
        }
        if (unit.UnitPrefab == null)
        {
            LogError($"ScriptableUnit {unit.UnitPrefab} has no prefab assigned!");
            return;
        }

        DatabaseManager.Instance.ExecuteNonQuery(
        "UPDATE unit_resources SET major_action = 1, minor_action = 1, reaction = 1 WHERE id = @unitID",
            ("@unitID", unitID)
        );
    }

    public void DamageUnit(int unitID, int damage, bool wasCrit)
    {
        int damageRemaining = damage;

        int currentHP = 0;
        int tempHP = 0;
        int maxHP = 0;

        DatabaseManager.Instance.ExecuteReader(
            "SELECT current_hp, temp_hp, max_hp FROM unit_resources WHERE id = @unitID",
            reader =>
            {
                while (reader.Read())
                {
                    currentHP = Convert.ToInt32(reader["current_hp"]);
                    tempHP = Convert.ToInt32(reader["temp_hp"]);
                    maxHP = Convert.ToInt32(reader["max_hp"]);
                }
            },
            ("@unitID", unitID)
        );

        if (tempHP > 0 && tempHP >= damageRemaining)
        {
            tempHP -= damageRemaining;
            damageRemaining = 0;
        }
        else
        {
            damageRemaining -= tempHP;
            tempHP = 0;
        }

        if (currentHP > 0 && currentHP > damageRemaining)
        {
            currentHP -= damageRemaining;
        }
        else if (currentHP > 0)
        {
            damageRemaining -= currentHP;
            if (damageRemaining >= maxHP)
            {
                //Unit dies
            }
            else
            {
                currentHP = 0;
            }
        }
        else
        {
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
        
        Log("Unit's current HP is: " + currentHP);


        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE unit_stats SET temp_hp = @tempHP, current_hp = @currentHP "+
            "WHERE id = @unitID",
            ("@tempHP", tempHP),
            ("@currentHP", currentHP),
            ("@unitID", unitID)
        );
    }
    
    public void HealUnit(int unitID, int healing)
    {
        int currentHP = 0;
        int maxHP = 0;
        DatabaseManager.Instance.ExecuteReader(
            "SELECT current_hp, max_hp FROM unit_resources WHERE id = @unitID",
            reader =>
            {
                while (reader.Read())
                {
                    currentHP = Convert.ToInt32(reader["current_hp"]);
                    maxHP = Convert.ToInt32(reader["max_hp"]);
                }
            },
            ("@unitID", unitID)
        );

        if (currentHP + healing >= maxHP)
        {
            currentHP = maxHP;
        }
        else
        {
            if (currentHP == 0) //And unit is not dead
            {
                //Clear death saves
            }

            currentHP += healing;
        }

        Log("Unit's current HP is: " + currentHP);
        
        DatabaseManager.Instance.ExecuteNonQuery(
            "UPDATE unit_stats SET current_hp = @currentHP WHERE id = @unitID",
            ("@currentHP", currentHP),
            ("@unitID", unitID)
        );
    }

}

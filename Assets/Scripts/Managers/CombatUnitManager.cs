using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEngine.Debug;

public class CombatUnitManager : MonoBehaviour
{
    private List<ScriptableUnit> units;
    public List<BaseUnit> baseUnits = new List<BaseUnit>();
    public List<int> activePCIDs = new List<int>();
    public List<int> activeMonsterIDs = new List<int>();
    public BasePC SelectedPC; //We only ever want a PC to be actionable
    public static CombatUnitManager Instance;
    public string PCList = "(0,1,2,3,4,5,9991)";

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

        UpdateActivePCList();

        for (int i = 0; i < activePCIDs.Count; i++) //For each PC
        {
            ScriptableUnit pcToSpawn = units.FirstOrDefault(u => u.UnitID == activePCIDs[i]); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

            if (pcToSpawn == null)
            {
                LogError($"No ScriptableUnit found with UnitID {activePCIDs[i]}");
            }
            if (pcToSpawn.UnitPrefab == null)
            {
                LogError($"ScriptableUnit {pcToSpawn.UnitPrefab} has no prefab assigned!");
                return;
            }

            var spawnedPC = Instantiate(pcToSpawn.UnitPrefab); //Creates the PC unit
            spawnedPC.Initialize();
            baseUnits.Add(spawnedPC);
            
            var sr = spawnedPC.GetComponent<SpriteRenderer>();
            if (sr != null && pcToSpawn.UnitSprite != null)
            {
                sr.sprite = pcToSpawn.UnitSprite;
                Log("Had to manually set Sprite Renderer for " + spawnedPC.UnitName);
            }


            var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(activePCIDs[i], encounterID); //Gets which tile that PC is supposed to spawn on

            StartCoroutine(spawnTile.SetUnit(spawnedPC)); //Set that spawn tile's unit to the spawned PC

            ReactionManager.Instance.RegisterUnit(spawnedPC);
        }

        // StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.SpawnMonsters));
    }

    public void SpawnMonsters(int encounterID)
    {
        UpdateActiveMonsterList();
        
        for(int i = 0; i < activeMonsterIDs.Count; i++)
        {
            ScriptableUnit monsterToSpawn = units.FirstOrDefault(u => u.UnitID == activeMonsterIDs[i]); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

            if (monsterToSpawn == null)
            {
                LogError($"No ScriptableUnit found with UnitID {activeMonsterIDs[i]}");
            }
            if (monsterToSpawn.UnitPrefab == null)
            {
                LogError($"ScriptableUnit {monsterToSpawn.UnitPrefab} has no prefab assigned!");
                return;
            }

            var spawnedMonster = Instantiate(monsterToSpawn.UnitPrefab); //Creates the monster unit
            spawnedMonster.SetName(monsterToSpawn.name);
            spawnedMonster.Initialize();

            baseUnits.Add(spawnedMonster);

            var sr = spawnedMonster.GetComponent<SpriteRenderer>();
            if (sr != null && monsterToSpawn.UnitSprite != null)
                sr.sprite = monsterToSpawn.UnitSprite;

            var spawnTile = CombatGridManager.Instance.GetMonsterSpawnTile(activeMonsterIDs[i], encounterID); //Gets which tile that PC is supposed to spawn on

            StartCoroutine(spawnTile.SetUnit(spawnedMonster)); //Set that spawn tile's unit to the spawned PC
            ReactionManager.Instance.RegisterUnit(spawnedMonster);

        }

        // StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.Precombat));
    }

    public void SetSelectedPC(BasePC pc)
    {
        SelectedPC = pc;
        CombatMenuManager.Instance.ShowSelectedPC(pc);
    }

    public BaseUnit GetUnitByID(int id)
    {
        return baseUnits.FirstOrDefault(u => u.UnitID == id);
    }

    public void UpdateActivePCList()
    {
        activePCIDs.Clear();

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT unit_id FROM grid_contents WHERE unit_id IN {PCList} AND encounter_id = {DatabaseManager.Instance.currentEncounter}",
            reader =>
            {
                while (reader.Read())
                {
                    activePCIDs.Add(Convert.ToInt32(reader["unit_id"]));
                }
            }
        );
    }

    public void UpdateActiveMonsterList()
    {
        activeMonsterIDs.Clear();
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT unit_id FROM grid_contents WHERE unit_id NOT IN {PCList} AND encounter_id = {DatabaseManager.Instance.currentEncounter}",
            reader =>
            {
                while (reader.Read())
                {
                    activeMonsterIDs.Add(Convert.ToInt32(reader["unit_id"]));
                }
            }
        );
    }

    public void ResetOncePerTurnResources()
    {
        List<(string, int)> oncePerTurnResources = new List<(string, int)>()
        {
            ("stunning_strike_available", 1),
            ("dreadful_strikes_target", 1),
            ("dreadful_strike_available", 1),
            ("colossus_slayer_available", 1),
            ("sneak_attack_available", 1),
            ("eldritch_smite_available", 1),
            ("savage_attacker_available", 1),
            ("crusher_push_available", 1),
            ("piercer_reroll_available", 1),
            ("slasher_slow_available", 1)
        };

        for(int i = 0; i < units.Count; i++)
        {
            BaseUnit currentUnit = units[i].UnitPrefab;


            for(int j = 0; j < oncePerTurnResources.Count; j++)
            {
                if (currentUnit.GetResource(oncePerTurnResources[j].Item1) != -1)
                {
                    currentUnit.SetResource(oncePerTurnResources[j].Item1, oncePerTurnResources[j].Item2);
                }
            }
        }
    }

}

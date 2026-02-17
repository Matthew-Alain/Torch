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
        DontDestroyOnLoad(gameObject);

        //Goes into resources folder, goes into units folder, look into all subfolders for all types of scriptable units and put them into this list
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnPCs(int encounterID)
    {
        // int PCCount = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(   //Get the number of PCs to spawn, in theory this should always be 5
        //     "SELECT COUNT(content) FROM grid_default_contents WHERE encounter_id = (@encounterID) AND content NOT NULL",
        //     ("@encounterID", encounterID)
        // ));

        for (int i = 0; i < 5; i++) //For each PC
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

            var spawnedPC = Instantiate(pcToSpawn.UnitPrefab); //Creates the PC unit
            var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(i); //Gets which tile that PC is supposed to spawn on

            spawnTile.SetUnit(spawnedPC); //Set that spawn tile's unit to the spawned PC
        }

        CombatStateManager.Instance.ChangeState(GameState.SpawnMonsters);
    }

    public void SpawnMonsters()
    {
        var enemyCount = 1;

        for (int i = 0; i < enemyCount; i++)
        {
            var randomPrefab = GetRandomUnit<BaseMonster>(Faction.Monster);
            var spawnedEnemy = Instantiate(randomPrefab);
            var randomSpawnTile = CombatGridManager.Instance.GetMonsterSpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
        }

        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }


    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        //Go through list, getting all units according to the called faction, shuffle them, select the first one, and return its prefab
        return (T)units.Where(u => u.Faction == faction).OrderBy(o => UnityEngine.Random.value).First().UnitPrefab;
    }

    public void SetSelectedPC(BasePC pc)
    {
        SelectedPC = pc;
        CombatMenuManager.Instance.ShowSelectedPC(pc);
    }

}

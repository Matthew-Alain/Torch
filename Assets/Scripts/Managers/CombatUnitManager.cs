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
    public List<int> activePCIDs = new List<int>();
    public List<int> activeMonsterIDs = new List<int>();
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

        UpdateActivePCList();

        for (int i = 0; i < activePCIDs.Count; i++) //For each PC
        {
            ScriptableUnit pcToSpawn = units.FirstOrDefault(u => u.UnitID == activePCIDs[i]); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

            if (pcToSpawn == null)
            {
                Log($"No ScriptableUnit found with UnitID {activePCIDs[i]}");
            }
            if (pcToSpawn.UnitPrefab == null)
            {
                LogError($"ScriptableUnit {pcToSpawn.UnitPrefab} has no prefab assigned!");
                return;
            }

            var spawnedPC = Instantiate(pcToSpawn.UnitPrefab); //Creates the PC unit
            spawnedPC.SetName(spawnedPC.GetName());
            baseUnits.Add(spawnedPC);
            
            var sr = spawnedPC.GetComponent<SpriteRenderer>();
            if (sr != null && pcToSpawn.UnitSprite != null)
                sr.sprite = pcToSpawn.UnitSprite;


            var spawnTile = CombatGridManager.Instance.GetPCSpawnTile(activePCIDs[i], encounterID); //Gets which tile that PC is supposed to spawn on

            spawnTile.SetUnit(spawnedPC); //Set that spawn tile's unit to the spawned PC
        }

        CombatStateManager.Instance.ChangeState(GameState.SpawnMonsters);
    }

    public void SpawnMonsters(int encounterID)
    {
        UpdateActiveMonsterList();
        
        for(int i = 0; i < activeMonsterIDs.Count; i++)
        {
            ScriptableUnit monsterToSpawn = units.FirstOrDefault(u => u.UnitID == activeMonsterIDs[i]); //Get the PCs from the list of units (based on the fact PC unitIDs are manually assigned)

            if (monsterToSpawn == null)
            {
                Log($"No ScriptableUnit found with UnitID {activeMonsterIDs[i]}");
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

            var spawnTile = CombatGridManager.Instance.GetMonsterSpawnTile(activeMonsterIDs[i], encounterID); //Gets which tile that PC is supposed to spawn on

            spawnTile.SetUnit(spawnedMonster); //Set that spawn tile's unit to the spawned PC
        }

        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }

    public void SetSelectedPC(BasePC pc)
    {
        SelectedPC = pc;
        CombatMenuManager.Instance.ShowSelectedPC(pc);
    }

    public void FallUnconscious(int unitID)
    {
        BaseUnit unit = GetUnitByID(unitID);
        
        if (unit.Faction == Faction.Monster)
        {
            LogWarning("Tried to knock a monster unconscious, killing unit instead.");
            KillUnit(unitID);
        }
        else
        {
            string characterName = Convert.ToString(DatabaseManager.Instance.ExecuteScalar(
                $"SELECT name FROM saved_pcs WHERE id = {unitID}"
            ));
            Log(characterName + " has fallen unconscious!");
        }

    }

    public void KillUnit(int unitID)
    {
        BaseUnit unit = GetUnitByID(unitID);

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
            LogWarning(characterName + " has been killed!");
        }

        unit.occupiedTile.EmptyTile();

        UpdateActivePCList();
        UpdateActivePCList();

        CombatStateManager.Instance.CheckForGameOver();
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

    public void UpdateActivePCList()
    {
        activePCIDs.Clear();
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT unit_id FROM grid_contents WHERE unit_id <= 4 AND encounter_id = {DatabaseManager.Instance.currentEncounter}",
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
            $"SELECT unit_id FROM grid_contents WHERE unit_id > 4 AND encounter_id = {DatabaseManager.Instance.currentEncounter}",
            reader =>
            {
                while (reader.Read())
                {
                    activeMonsterIDs.Add(Convert.ToInt32(reader["unit_id"]));
                }
            }
        );
    }

}

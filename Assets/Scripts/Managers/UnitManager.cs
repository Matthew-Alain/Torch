using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using TMPro.Examples;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private List<ScriptableUnit> units;
    public BasePC SelectedPC; //We only ever want a PC to be actionable
    public static UnitManager Instance;

    void Awake()
    {
        Instance = this;

        //Goes into resources folder, goes into units folder, look into all subfolders for all types of scriptable units and put them into this list
        units = Resources.LoadAll<ScriptableUnit>("Units").ToList();
    }

    public void SpawnPCs()
    {
        var PCCount = 1;

        for (int i = 0; i < PCCount; i++) //For each PC
        {
            var randomPrefab = GetRandomUnit<BasePC>(Faction.PC); //Get one of them
            var spawnedPC = Instantiate(randomPrefab); //Spawn the PC on the map
            var randomSpawnTile = GridManager.Instance.GetPCSpawnTile(); //

            randomSpawnTile.SetUnit(spawnedPC);
        }

        GameManager.Instance.ChangeState(GameState.SpawnMonsters);
    }

    public void SpawnMonsters()
    {
        var enemyCount = 1;

        for (int i = 0; i < enemyCount; i++)
        {
            var randomPrefab = GetRandomUnit<BaseMonster>(Faction.Monster);
            var spawnedEnemy = Instantiate(randomPrefab);
            var randomSpawnTile = GridManager.Instance.GetMonsterSpawnTile();

            randomSpawnTile.SetUnit(spawnedEnemy);
        }

        GameManager.Instance.ChangeState(GameState.PlayerTurn);
    }


    private T GetRandomUnit<T>(Faction faction) where T : BaseUnit
    {
        //Go through list, getting all units according to the called faction, shuffle them, select the first one, and return its prefab
        return (T)units.Where(u => u.Faction == faction).OrderBy(o => Random.value).First().UnitPrefab;
    }

    public void SetSelectedPC(BasePC pc)
    {
        SelectedPC = pc;
        MenuManager.Instance.ShowSelectedPC(pc);
    }

}

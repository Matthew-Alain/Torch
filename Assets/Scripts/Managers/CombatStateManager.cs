using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatStateManager : MonoBehaviour
{

    public static CombatStateManager Instance;
    public GameState GameState;
    public int declaredWeapon;

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
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ChangeState(GameState.GenerateGrid);
    }

    void OnEnable()
    {
        DatabaseManager.Instance.SwitchDatabase(DatabaseManager.Instance.currentEncounter);
    }

    void OnDisable()
    {
        DatabaseManager.Instance.SwitchDatabase(-1);
    }

    public void EndPlayerTurn()
    {
        CombatMenuManager.Instance.CloseMenu();
        CombatUnitManager.Instance.SetSelectedPC(null);

        ChangeState(GameState.StartMonsterTurn);
    }

    public void ChangeState(GameState newState)
    {
        if (GameState == newState) return;
        GameState = newState;

        switch (newState)
        {
            case GameState.GenerateGrid:
                CombatGridManager.Instance.GenerateGrid(DatabaseManager.Instance.currentEncounter);
                break;
            case GameState.SpawnHeroes:
                CombatUnitManager.Instance.SpawnPCs(DatabaseManager.Instance.currentEncounter);
                break;
            case GameState.SpawnMonsters:
                CombatUnitManager.Instance.SpawnMonsters(DatabaseManager.Instance.currentEncounter);
                break;
            case GameState.Precombat:
                break;
            case GameState.RollInitiative:
                break;
            case GameState.StartPlayerTurn:
                StartPlayerTurn();
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.MovingPC:
                Debug.Log("Select the space to move to.");
                break;
            case GameState.SelectWeapon:
                break;
            case GameState.SelectAttackTarget:
                break;
            case GameState.StartMonsterTurn:
                Debug.Log("It is now the monster's turn");
                StartMonsterTurn();
                break;
            case GameState.MonsterTurn:
                TakeMonsterTurn();
                ChangeState(GameState.StartPlayerTurn);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

    }

    private void StartPlayerTurn()
    {
        for (int i = 0; i < CombatUnitManager.Instance.activePCIDs.Count; i++)
        {
            int currentUnitID = CombatUnitManager.Instance.activePCIDs[i];
            BaseUnit currentUnitBase = CombatUnitManager.Instance.GetUnitByID(currentUnitID);

            currentUnitBase.RefreshSpeed();
            currentUnitBase.RefreshActions();
        }

        ChangeState(GameState.PlayerTurn);
    }

    private void StartMonsterTurn()
    {
        // DatabaseManager.Instance.ExecuteReader(
        //     $"SELECT unit_id FROM grid_contents WHERE unit_id > 4 AND encounter_id = {DatabaseManager.Instance.encounterToLoad}",
        //     reader =>
        //     {
        //         while (reader.Read())
        //         {
        //             int monsterID = Convert.ToInt32(reader["unit_id"]);
        //             CombatUnitManager.Instance.RefreshUnitActions(monsterID);
        //             CombatUnitManager.Instance.RefreshUnitSpeed(monsterID);
        //         }
        //     }
        // );

        List<int> monsterIDList = CombatUnitManager.Instance.activeMonsterIDs;

        for (int i = 0; i < monsterIDList.Count; i++)
        {
            int currentMonsterID = monsterIDList[i];
            BaseUnit currentMonsterBase = CombatUnitManager.Instance.GetUnitByID(currentMonsterID);

            currentMonsterBase.RefreshSpeed();
            currentMonsterBase.RefreshActions();

        }

        ChangeState(GameState.MonsterTurn);
    }

    public void DeclareAttack(int weaponID)
    {
        declaredWeapon = weaponID;
        ChangeState(GameState.SelectAttackTarget);
        Debug.Log("Selecting attack target");
    }

    private void TakeMonsterTurn()
    {
        List<int> monsterIDList = CombatUnitManager.Instance.activeMonsterIDs;

        for (int i = 0; i < CombatUnitManager.Instance.activeMonsterIDs.Count; i++)
        {
                    Debug.Log("GetUnitByID: " + CombatUnitManager.Instance.GetUnitByID(monsterIDList[i]) + ", Type: " +
                    CombatUnitManager.Instance.GetUnitByID(monsterIDList[i]).GetType().ToString());
            
            BaseMonster currentMonster = (BaseMonster)CombatUnitManager.Instance.GetUnitByID(monsterIDList[i]);
            currentMonster.CheckValidActions();
            if (currentMonster.validActions != null)
            {
                int targetID = currentMonster.ChooseTarget();
                int attackID = currentMonster.ChooseAttack();
                currentMonster.MoveToUnit(targetID);

                currentMonster.AttackTarget(targetID, attackID);
                currentMonster.EndTurn();
            }
        }
    }
    
    public void CheckForGameOver()
    {
        if (Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar("SELECT COUNT(*) FROM grid_contents WHERE unit_id > 4")) <= 0)
        {
            Debug.LogWarning("The last monster has been killed, you win!");
        }
        else if (Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar("SELECT COUNT(*) FROM grid_contents WHERE unit_id <= 4")) <= 0)
        {
            Debug.LogWarning("The last PC has been killed, you lose...");
        }
        else
        {
            return;
        }
        SceneManager.LoadScene(0);
        DatabaseManager.Instance.DeleteEncounterDatabase(DatabaseManager.Instance.currentEncounter);
    }
}

public enum GameState
{
    GenerateGrid,
    SpawnHeroes,
    SpawnMonsters,
    Precombat,
    RollInitiative,
    StartPlayerTurn,
    PlayerTurn,
    MovingPC,
    SelectWeapon,
    SelectAttackTarget,
    StartMonsterTurn,
    MonsterTurn

}
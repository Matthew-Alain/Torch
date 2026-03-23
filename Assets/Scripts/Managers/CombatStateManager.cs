using System;
using System.Collections;
using System.Collections.Generic;
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

        StartCoroutine(ChangeState(GameState.StartMonsterTurn));
    }

    public IEnumerator ChangeState(GameState newState)
    {
        if (GameState == newState) yield return null;
        GameState = newState;

        // Debug.Log("New state: "+GameState);

        switch (GameState)
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
                StartCoroutine(StartPlayerTurn());
                break;
            case GameState.PlayerTurn:
                CheckForGameOver();
                break;
            case GameState.MovingPC:
                CombatMenuManager.Instance.DisplayText($"{CombatUnitManager.Instance.SelectedPC.UnitName} is now moving");
                // Debug.Log("Select the space to move to.");
                break;
            case GameState.SelectWeapon:
                break;
            case GameState.SelectAttackTarget:
                break;
            case GameState.StartMonsterTurn:
                // Debug.Log("It is now the monster's turn");
                StartCoroutine(StartMonsterTurn());
                break;
            case GameState.MonsterTurn:
                StartCoroutine(TakeMonsterTurn());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

    }

    private IEnumerator StartPlayerTurn()
    {
        for (int i = 0; i < CombatUnitManager.Instance.activePCIDs.Count; i++)
        {
            int currentUnitID = CombatUnitManager.Instance.activePCIDs[i];
            BaseUnit currentUnit = CombatUnitManager.Instance.GetUnitByID(currentUnitID);

            currentUnit.RefreshSpeed();
            currentUnit.RefreshActions();

            if (currentUnit.GetCondition("dying"))
            {
                yield return StartCoroutine(currentUnit.MakeDeathSave());
            }
        }

        StartCoroutine(ChangeState(GameState.PlayerTurn));
    }

    private IEnumerator StartMonsterTurn()
    {
        CombatMenuManager.Instance.DisplayText($"It is now the monster's turn");

        List<int> monsterIDList = CombatUnitManager.Instance.activeMonsterIDs;

        for (int i = 0; i < monsterIDList.Count; i++)
        {
            int currentMonsterID = monsterIDList[i];
            BaseUnit currentMonsterBase = CombatUnitManager.Instance.GetUnitByID(currentMonsterID);

            currentMonsterBase.RefreshSpeed();
            currentMonsterBase.RefreshActions();
            yield return null;
        }

        StartCoroutine(ChangeState(GameState.MonsterTurn));
    }

    public void DeclareAttack(int weaponID)
    {
        declaredWeapon = weaponID;
        StartCoroutine(ChangeState(GameState.SelectAttackTarget));
        Debug.Log("Selecting attack target");
    }

    IEnumerator TakeMonsterTurn()
    {
        List<int> monsterIDList = CombatUnitManager.Instance.activeMonsterIDs;

        for (int i = 0; i < CombatUnitManager.Instance.activeMonsterIDs.Count; i++)
        {
            BaseMonster currentMonster = (BaseMonster)CombatUnitManager.Instance.GetUnitByID(monsterIDList[i]);
            yield return StartCoroutine(currentMonster.CheckValidActions());
            // Debug.Log("Finished finding valid actions for " + currentMonster.UnitName);

            if (currentMonster.validActions != null && currentMonster.validActions.Count > 0)
            {
                (BaseUnit, int) targetAndAttack = currentMonster.ChooseTargetAndAttack();

                if (targetAndAttack != (null, -1))
                {
                    BaseUnit target = targetAndAttack.Item1;
                    int attackID = targetAndAttack.Item2;
                    // CombatMenuManager.Instance.DisplayText($"{currentMonster.UnitName} is attacking {target.UnitName}");

                    yield return StartCoroutine(currentMonster.MoveToTile(currentMonster.GetPathToBestAttackTile(target.occupiedTile, attackID)));
                    yield return new WaitForSeconds(0.5f);
                    yield return StartCoroutine(currentMonster.AttackTarget(target, attackID));
                    yield return new WaitForSeconds(0.5f);
                }
                
                currentMonster.EndTurn();
            }
        }

        yield return StartCoroutine(ChangeState(GameState.StartPlayerTurn));
    }

    public void CheckForGameOver()
    {
        int numberOfActivePCs = 0;

        for (int i = 0; i < CombatUnitManager.Instance.activePCIDs.Count; i++)
        {
            BaseUnit currentPC = CombatUnitManager.Instance.GetUnitByID(CombatUnitManager.Instance.activePCIDs[i]);
            if (!(currentPC.GetCondition("dying") || currentPC.GetCondition("unconscious") || currentPC.GetCondition("dead")))
            {
                numberOfActivePCs += 1;
            }
        }

        if (Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM grid_contents WHERE encounter_id = {DatabaseManager.Instance.currentEncounter} "+
        $"AND unit_id NOT IN {CombatUnitManager.Instance.PCList}")) <= 0)
        {
            Debug.LogWarning("The last monster has been killed, you win!");
            //Create a canvas window that announces this, with a button to reset
        }
        else if (numberOfActivePCs <= 0)
        {
            Debug.LogWarning("You have no more active PCs, you lose...");
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
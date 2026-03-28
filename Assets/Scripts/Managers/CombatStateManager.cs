using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CombatStateManager : MonoBehaviour
{

    public static CombatStateManager Instance;
    public GameState GameState;
    public int declaredWeapon;
    public BaseUnit selectedTarget = null;
    public Tile selectedTile = null;

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
            case GameState.SpawnPCs:
                CombatUnitManager.Instance.SpawnPCs(DatabaseManager.Instance.currentEncounter);
                break;
            case GameState.SpawnMonsters:
                CombatUnitManager.Instance.SpawnMonsters(DatabaseManager.Instance.currentEncounter);
                break;
            case GameState.Precombat:
                StartCoroutine(ChangeState(GameState.RollInitiative));
                break;
            case GameState.RollInitiative:
                StartCoroutine(InitiativeTracker.Instance.RollInitiative());
                break;
            case GameState.StartPlayerTurn:
                StartCoroutine(InitiativeTracker.Instance.StartTurn());
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
            case GameState.SelectTarget:
                Debug.Log("You are now selecting a target");
                break;
            case GameState.SelectTargetUnit:
                Debug.Log("The target you are selecting is any unit");
                break;
            case GameState.SelectTargetMonster:
                Debug.Log("The target you are selecting is a monster");
                break;
            case GameState.StartMonsterTurn:
                // Debug.Log("It is now the monster's turn");
                StartCoroutine(InitiativeTracker.Instance.StartTurn());
                break;
            case GameState.MonsterTurn:
                StartCoroutine(TakeMonsterTurn());
                break;
            default:
                Debug.LogWarning("No state for " + newState);
                break;
        }

    }

    public void DeclareAttack(int weaponID)
    {
        declaredWeapon = weaponID;
        StartCoroutine(ChangeState(GameState.SelectTarget));
        Debug.Log("Selecting attack target");
    }

    IEnumerator TakeMonsterTurn()
    {
        // List<int> monsterIDList = CombatUnitManager.Instance.activeMonsterIDs;

        // for (int i = 0; i < CombatUnitManager.Instance.activeMonsterIDs.Count; i++)
        // {
        // BaseMonster currentMonster = (BaseMonster)CombatUnitManager.Instance.GetUnitByID(monsterIDList[i]);

        BaseMonster currentMonster = (BaseMonster)InitiativeTracker.Instance.currentTurnUnit;

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
            }
        // }

        yield return null;
        InitiativeTracker.Instance.EndTurn();
    }

    public IEnumerator SelectTarget(TargetType targetType, Action<BaseUnit> onComplete)
    {

        switch (targetType)
        {
            case TargetType.Monster:
                yield return StartCoroutine(ChangeState(GameState.SelectTargetMonster));
                break;
            case TargetType.PC:
                yield return StartCoroutine(ChangeState(GameState.SelectTargetPC));
                break;
            case TargetType.Unit:
                yield return StartCoroutine(ChangeState(GameState.SelectTargetUnit));
                break;
            default:
                break;
        }

        yield return new WaitUntil(() => selectedTarget != null);

        BaseUnit returnTarget = selectedTarget;
        selectedTarget = null;

        Debug.Log("Selected target is: " + returnTarget.UnitName);
        
        onComplete?.Invoke(returnTarget);
    }

    public Tile SelectTile()
    {
        StartCoroutine(ChangeState(GameState.SelectTargetTile));

        Tile returnTile = null;
        StartCoroutine(Instance.GetTile(target =>
        {
            returnTile = target;
        }));

        Debug.Log("Selected tile is: " + returnTile.tileX+", "+returnTile.tileY);
        
        return returnTile;
    }
    
    private IEnumerator GetTile(Action<Tile> tile)
    {
        yield return StartCoroutine(ChangeState(GameState.SelectTarget));

        StartCoroutine(ChangeState(GameState.SelectTargetMonster));

        yield return new WaitUntil(() => selectedTile != null);

        Tile returnTile = selectedTile;
        selectedTarget = null;

        tile?.Invoke(returnTile);
    }

    public void CheckForGameOver()
    {
        bool activePC = false;

        for (int i = 0; i < CombatUnitManager.Instance.activePCIDs.Count; i++)
        {
            BaseUnit currentPC = CombatUnitManager.Instance.GetUnitByID(CombatUnitManager.Instance.activePCIDs[i]);
            if (!(currentPC.GetCondition("dying") || currentPC.GetCondition("unconscious") || currentPC.GetCondition("dead")))
            {
                activePC = true;
                break;
            }
        }

        if (Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM grid_contents WHERE encounter_id = {DatabaseManager.Instance.currentEncounter} "+
        $"AND unit_id NOT IN {CombatUnitManager.Instance.PCList}")) <= 0)
        {
            Debug.LogWarning("The last monster has been killed, you win!");
            //Create a canvas window that announces this, with a button to reset
        }
        else if (!activePC)
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
    SpawnPCs,
    SpawnMonsters,
    Precombat,
    RollInitiative,
    StartPlayerTurn,
    PlayerTurn,
    MovingPC,
    SelectWeapon,
    SelectTarget,
    SelectTargetPC,
    SelectTargetMonster,
    SelectTargetUnit,
    SelectTargetTile,
    StartMonsterTurn,
    MonsterTurn

}

public enum TargetType
{
    Unit,
    PC,
    Monster,
    Tile
}
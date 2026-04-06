using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class CombatStateManager : MonoBehaviour
{

    public static CombatStateManager Instance;
    public GameState GameState;
    public int declaredWeapon;
    private Action<BaseUnit> onTargetSelected;
    private Action<Tile> onTileSelected;
    private Func<BaseUnit, (bool, string)> targetValidator;
    private Func<Tile, (bool, string)> tileValidator;
    private Action onReactionComplete;
    public bool endTurn = false;
    private bool gameOver = false;
    public bool isSelectingTile;
    public Tile selectedTile;
    public bool isSelectingTarget;
    public BaseUnit selectedTarget;
    public bool reloadedPreviousSave;
    public bool cancelSelection = false;

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
            // case GameState.Precombat:
            //     StartCoroutine(ChangeState(GameState.RollInitiative));
            //     break;
            // case GameState.RollInitiative:
            //     StartCoroutine(InitiativeTracker.Instance.RollInitiative());
            //     break;
            // case GameState.StartPlayerTurn:
            //     // StartCoroutine(InitiativeTracker.Instance.StartTurn());
            //     break;
            case GameState.PlayerTurn:
                StartCoroutine(CheckForGameOver());
                break;
            case GameState.MovingPC:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"{CombatUnitManager.Instance.SelectedPC.UnitName} is now moving"));
                // Debug.Log("Select the space to move to.");
                break;
            // case GameState.SelectWeapon:
            //     break;
            case GameState.SelectTarget:
                // Debug.Log("You are now selecting a target");
                break;
            case GameState.SelectTargetUnit:
                // Debug.Log("The target you are selecting is any unit");
                break;
            case GameState.SelectTargetMonster:
                // Debug.Log("The target you are selecting is a monster");
                break;
            // case GameState.StartMonsterTurn:
            //     // Debug.Log("It is now the monster's turn");
            //     // StartCoroutine(InitiativeTracker.Instance.StartTurn());
            //     break;
            // case GameState.MonsterTurn:
            //     yield return StartCoroutine(TakeMonsterTurn());
            //     break;
            default:
                // Debug.LogWarning("No state for " + newState);
                break;
        }

    }

    public void CancelSelection()
    {
        if (!isSelectingTarget && !isSelectingTile)
            return;

        cancelSelection = true;
    }

    public IEnumerator StartTargetSelection(
        TargetType targetType,
        Action<BaseUnit> callback,
        Func<BaseUnit, (bool success, string message)> validator = null
    )
    {
        isSelectingTarget = true;
        selectedTarget = null;
        cancelSelection = false;

        onTargetSelected = callback;
        targetValidator = validator;

        EnableSelectionVisuals(targetType);

        switch (targetType)
        {
            case TargetType.Monster:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText("Select a monster to target"));
                StartCoroutine(ChangeState(GameState.SelectTargetMonster));
                break;
            case TargetType.PC:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText("Select a PC to target"));
                StartCoroutine(ChangeState(GameState.SelectTargetPC));
                break;
            case TargetType.Unit:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText("Select a unit to target"));
                StartCoroutine(ChangeState(GameState.SelectTargetUnit));
                break;
        }

        yield return new WaitUntil(() => selectedTarget != null || cancelSelection);

        isSelectingTarget = false;
        DisableSelectionVisuals();

        if (cancelSelection)
            yield break;

        onTargetSelected?.Invoke(selectedTarget);
    }

    public IEnumerator StartTileSelection(
        TargetType targetType,
        Action<Tile> callback,
        Func<Tile, (bool success, string message)> validator = null
    )
    {
        isSelectingTile = true;
        selectedTile = null;
        cancelSelection = false;

        onTileSelected = callback;
        tileValidator = validator;

        EnableSelectionVisuals(targetType);

        switch (targetType)
        {
            case TargetType.AnyTile:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText("Select a tile to target"));
                yield return StartCoroutine(ChangeState(GameState.SelectTargetTile));
                break;
            case TargetType.EmptyTile:
                yield return StartCoroutine(CombatMenuManager.Instance.DisplayText("Select an empty tile to target"));
                yield return StartCoroutine(ChangeState(GameState.SelectTargetEmptyTile));
                break;
        }

        yield return new WaitUntil(() => selectedTile != null || cancelSelection);

        isSelectingTile = false;
        DisableSelectionVisuals();

        if (cancelSelection)
            yield break;

        onTileSelected?.Invoke(selectedTile);
    }

    public IEnumerator ConfirmTileTargetSelection(Tile tile)
    {
        if (GameState == GameState.SelectTargetTile || GameState == GameState.SelectTargetEmptyTile)
        {
            if (tileValidator != null)
            {
                var result = tileValidator(tile);

                if (!result.Item1)
                {
                    yield return StartCoroutine(CombatMenuManager.Instance.DisplayText(result.Item2));
                    yield break;
                }
            }

            onTileSelected.Invoke(tile);
            selectedTile = tile;
            onTileSelected = null;
            tileValidator = null;
        }
        else
        {
            BaseUnit target = tile.OccupiedUnit;

            if (onTargetSelected == null && onTileSelected == null)
                yield break;

            if (targetValidator != null)
            {
                var result = targetValidator(target);

                if (!result.Item1)
                {
                    yield return StartCoroutine(CombatMenuManager.Instance.DisplayText(result.Item2));
                    yield break;
                }
            }

            onTargetSelected.Invoke(target);
            selectedTarget = target;
            onTargetSelected = null;
            targetValidator = null;
        }
    }

    // public IEnumerator ConfirmTarget(BaseUnit target)
    // {
    //     if (onTargetSelected == null) yield break;

    //     if (targetValidator != null)
    //     {
    //         var result = targetValidator(target);

    //         if (!result.Item1)
    //         {
    //             yield return StartCoroutine(CombatMenuManager.Instance.DisplayText(result.Item2));
    //             yield break;
    //         }
    //     }

    //     onTargetSelected.Invoke(target);

    //     selectedTarget = target;

    //     onTargetSelected = null;
    //     targetValidator = null;

    //     // yield return StartCoroutine(ChangeState(GameState.PlayerTurn));
    // }

    // public IEnumerator ConfirmTile(Tile tile)
    // {
    //     if (onTileSelected == null) yield break;

    //     if (tileValidator != null)
    //     {
    //         var result = tileValidator(tile);

    //         if (!result.Item1)
    //         {
    //             yield return StartCoroutine(CombatMenuManager.Instance.DisplayText(result.Item2));
    //             yield break;
    //         }
    //     }

    //     onTileSelected.Invoke(tile);

    //     selectedTile = tile;

    //     onTileSelected = null;
    //     tileValidator = null;
    // }

    public void EnableSelectionVisuals(TargetType type)
    {
        foreach (Tile tile in CombatGridManager.Instance.tilesList)
        {
            tile.ShowValidTargeting(type);
        }
    }
    
    public void DisableSelectionVisuals()
    {
        foreach (Tile tile in CombatGridManager.Instance.tilesList)
        {
            tile.HideValidTargeting();
        }
    }


    public void RequestReaction(List<MenuOption> options, Action onComplete)
    {
        onReactionComplete = onComplete;

        CombatMenuManager.Instance.OpenMenu(() => options);

        StartCoroutine(ChangeState(GameState.SelectReaction));
    }

    public void CompleteReaction()
    {
        CombatMenuManager.Instance.CloseAllMenus();

        onReactionComplete?.Invoke();
        onReactionComplete = null;

        // StartCoroutine(ChangeState(GameState.PlayerTurn));
    }

    public void TryReaction(BaseUnit unit, List<MenuOption> options, Action onComplete)
    {
        if (unit.GetResource("reaction") > 0)
        {
            RequestReaction(options, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }


    public IEnumerator CheckForGameOver()
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

        if (!activePC)
        {
            Debug.LogWarning("You have no more active PCs, you lose...");
        }
        else if (Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM grid_contents " +
            $"WHERE encounter_id = {DatabaseManager.Instance.currentEncounter} " +
            $"AND unit_id NOT IN {CombatUnitManager.Instance.PCList}")) <= 0)
        {
            Debug.LogWarning("The last monster has been killed, you win!");
            //Create a canvas window that announces this, with a button to reset
        }
        else
        {
            yield break;
        }
        gameOver = true;
        StopAllCoroutines();
        SceneManager.LoadScene(0);
        DatabaseManager.Instance.DeleteEncounterDatabase(DatabaseManager.Instance.currentEncounter);
    }



    public IEnumerator CombatLoop()
    {
        // Setup phase
        reloadedPreviousSave = Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT in_progress FROM encounters WHERE id = {DatabaseManager.Instance.currentEncounter}"));
        yield return StartCoroutine(ChangeState(GameState.GenerateGrid));
        yield return StartCoroutine(ChangeState(GameState.SpawnPCs));
        yield return StartCoroutine(ChangeState(GameState.SpawnMonsters));
        yield return StartCoroutine(InitiativeTracker.Instance.RollInitiative(reloadedPreviousSave));

        while (true)
        {
            BaseUnit unit = InitiativeTracker.Instance.currentTurnUnit;

            if (unit == null)
                yield break;

            yield return StartCoroutine(RunTurn(unit));

            if (gameOver)
                yield break;

            yield return StartCoroutine(EndTurnFlow());
        }
    }

    private IEnumerator RunTurn(BaseUnit unit)
    {
        StartCoroutine(CheckForGameOver());

        if (unit.GetCondition("dead") || (unit.GetCurrentHP() <= 0 && !unit.GetCondition("dying")))
            yield break;

        StartCoroutine(CombatMenuManager.Instance.DisplayText($"Current turn: {unit.UnitName}"));

        if(!reloadedPreviousSave)
            InitiativeTracker.Instance.currentTurnUnit.RefreshStartOfTurnResources();

        if (unit.Faction == Faction.PC)
        {
            yield return StartCoroutine(RunPlayerTurn((BasePC)unit));
        }
        else
        {
            yield return StartCoroutine(RunMonsterTurn((BaseMonster)unit));
        }
    }

    private IEnumerator RunPlayerTurn(BasePC pc)
    {
        CombatUnitManager.Instance.SetSelectedPC(pc);

        if (pc.GetCondition("dying"))
        {
            yield return StartCoroutine(pc.MakeDeathSave());

            if (pc.GetCurrentHP() == 0)
            {
                CombatUnitManager.Instance.SetSelectedPC(null);
                yield break;
            }
        }

        bool turnComplete = false;

        CombatMenuManager.Instance.OpenRootMenu();

        // Wait until player ends turn
        pc.OnTurnEnded = () => turnComplete = true;

        yield return new WaitUntil(() => turnComplete);

        CombatUnitManager.Instance.SetSelectedPC(null);
    }

    private IEnumerator RunMonsterTurn(BaseMonster monster)
    {
        yield return StartCoroutine(monster.CheckValidActions());

        if (monster.validActions.Count > 0)
        {
            var (target, attackID) = monster.ChooseTargetAndAttack();

            if (target != null)
            {
                var path = monster.GetPathToBestAttackTile(target.occupiedTile, attackID);

                yield return StartCoroutine(monster.MoveToTile(path));

                if (TurnUtility.ShouldStop(monster)) goto EndTurn;

                yield return new WaitForSeconds(0.5f);

                yield return StartCoroutine(monster.AttackTarget(target, attackID));

                if (TurnUtility.ShouldStop(monster)) goto EndTurn;

                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                // fallback movement
                var tiles = monster.GetilesWithActivePCs();
                var randomTile = tiles[UnityEngine.Random.Range(0, tiles.Count)];
                var path = monster.GetPathToBestAttackTile(randomTile, 0);

                yield return StartCoroutine(monster.MoveToTile(path));
            }
        }

        EndTurn:
            monster.ClearActionList();
    }

    public IEnumerator EndTurnFlow()
    {
        CombatUnitManager.Instance.ResetOncePerTurnResources();

        reloadedPreviousSave = false;

        InitiativeTracker.Instance.AdvanceTurn();

        yield return null;
    }

}

public static class TurnUtility
{
    public static bool ShouldStop(BaseUnit unit)
    {
        if(unit == null)
        {
            // Debug.Log("Unit is null");
        }
        if(!unit.IsActive())
        {
            // Debug.Log("Unit is inactive");
        }

        return unit == null || !unit.IsActive();
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
    SelectReaction,
    SelectTarget,
    SelectTargetPC,
    SelectTargetMonster,
    SelectTargetUnit,
    SelectTargetTile,
    SelectTargetEmptyTile,
    StartMonsterTurn,
    MonsterTurn

}

public enum TargetType
{
    Unit,
    PC,
    Monster,
    AnyTile,
    EmptyTile
}
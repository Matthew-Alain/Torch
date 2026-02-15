using System;
using Unity.VisualScripting;
using UnityEngine;

public class CombatManager : MonoBehaviour
{

    public static CombatManager Instance;
    public GameState GameState;

    void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //Now safe to create a new instance
        Instance = this;    
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeState(GameState.GenerateGrid);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EndPlayerTurn()
    {
        ChangeState(GameState.MonsterTurn);
        CombatMenuManager.Instance.endTurnMenu.SetActive(false);
    }

    public void ChangeState(GameState newState)
    {
        if (GameState == newState) return;
        GameState = newState;

        switch (newState)
        {
            case GameState.GenerateGrid:
                CombatGridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnHeroes:
                CombatUnitManager.Instance.SpawnPCs();
                break;
            case GameState.SpawnMonsters:
                CombatUnitManager.Instance.SpawnMonsters();
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.MonsterTurn:
                Debug.Log("It is now the monster's turn");
                ChangeState(GameState.PlayerTurn);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

    }
}

public enum GameState
{
    GenerateGrid = 0,
    SpawnHeroes = 1,
    SpawnMonsters = 2,
    PlayerTurn = 3,
    MonsterTurn = 4

}
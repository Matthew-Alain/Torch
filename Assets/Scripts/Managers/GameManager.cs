using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public GameState GameState;

    void Awake()
    {
        Instance = this;
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
        MenuManager.Instance.endTurnMenu.SetActive(false);
    }

    public void ChangeState(GameState newState)
    {
        if (GameState == newState) return;
        GameState = newState;

        switch (newState)
        {
            case GameState.GenerateGrid:
                GridManager.Instance.GenerateGrid();
                break;
            case GameState.SpawnHeroes:
                UnitManager.Instance.SpawnPCs();
                break;
            case GameState.SpawnMonsters:
                UnitManager.Instance.SpawnMonsters();
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
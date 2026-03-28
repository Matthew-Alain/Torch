using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitiativeTracker: MonoBehaviour
{
    public static InitiativeTracker Instance;

    private List<(BaseUnit, int)> initiativeRolls = new List<(BaseUnit, int)>();
    public BaseUnit currentTurnUnit = null;
    
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

    public IEnumerator RollInitiative()
    {
        if (!Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT in_progress FROM encounters WHERE id = {DatabaseManager.Instance.currentEncounter}")))
        {
            foreach (BaseUnit unit in CombatUnitManager.Instance.baseUnits)
            {
                int initiative = unit.RollInitiative();
                initiativeRolls.Add((unit, initiative));
                Debug.Log($"{unit} rolled {initiative} for initiative");
            }

            initiativeRolls = initiativeRolls.OrderByDescending(x => x.Item2).ToList();

            Debug.Log("The turn order is: ");
            for (int i = 0; i < initiativeRolls.Count; i++)
            {
                Debug.Log(initiativeRolls[i].Item1.UnitName);

                DatabaseManager.Instance.ExecuteNonQuery($"INSERT INTO initiative_order VALUES ({initiativeRolls[i].Item1.UnitID}, {i})");
            }

            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE encounters SET in_progress = 1 WHERE id = {DatabaseManager.Instance.currentEncounter}");
        }

        currentTurnUnit = GetCurrentUnit();

        yield return StartCoroutine(StartTurn());

    }

    public int GetTurnCount()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT turn_count FROM encounters WHERE id = {DatabaseManager.Instance.currentEncounter}"));
    }

    public int GetNumberOfCombatants()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM initiative_order"));
    }
    
    private void IncrementTurnCount()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE encounters SET turn_count = {GetTurnCount()+1} WHERE id = {DatabaseManager.Instance.currentEncounter}");
    }

    public int GetInitiativeOrder()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT initiative_count FROM encounters WHERE id = {DatabaseManager.Instance.currentEncounter}"));
    }

    private void IncrementInitiativeOrder()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE encounters SET initiative_count = {GetInitiativeOrder() + 1} WHERE id = {DatabaseManager.Instance.currentEncounter}");
    }

    private void ResetInitiativeCount()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE encounters SET initiative_count = 0 WHERE id = {DatabaseManager.Instance.currentEncounter}");
    }
    
    public void RemoveFromInitiative(int PCID)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE initiative_count SET unit_id = NULL WHERE unit_id = {PCID}");
    }

    public BaseUnit GetCurrentUnit()
    {
        var turn_order = DatabaseManager.Instance.ExecuteScalar($"SELECT unit_id FROM initiative_order WHERE turn_order = {GetInitiativeOrder()}");
        while (turn_order == null)
        {
            Debug.Log("No unit found at initiative order " + GetInitiativeOrder());
            IncrementInitiativeOrder();
            if (GetInitiativeOrder() >= GetNumberOfCombatants())
            {
                ResetInitiativeCount();
                IncrementTurnCount();
            }
            turn_order = DatabaseManager.Instance.ExecuteScalar($"SELECT unit_id FROM initiative_order WHERE turn_order = {GetInitiativeOrder()}");
        }

        int unitID = Convert.ToInt32(turn_order);
        return CombatUnitManager.Instance.GetUnitByID(unitID);
    }
    
    public IEnumerator StartTurn()
    {
        if(currentTurnUnit == null)
        {
            EndTurn();
        }
        else
        {
            currentTurnUnit.RefreshStartOfTurnResources();

            if (currentTurnUnit.Faction == Faction.PC)
            {
                CombatUnitManager.Instance.SetSelectedPC((BasePC)currentTurnUnit);

                if (currentTurnUnit.GetCondition("dying"))
                {
                    yield return StartCoroutine(currentTurnUnit.MakeDeathSave());
                }

                yield return StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));
            }
            else
            {

                yield return StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.MonsterTurn));
            }
        }
    }

    public void EndTurn()
    {
        if (currentTurnUnit.Faction == Faction.PC)
        {
            CombatMenuManager.Instance.CloseMenu();
            CombatUnitManager.Instance.SetSelectedPC(null);
        }
        else
        {
            ((BaseMonster)currentTurnUnit).EndTurn();
        }

        IncrementInitiativeOrder();
        if (GetInitiativeOrder() >= GetNumberOfCombatants())
        {
            ResetInitiativeCount();
            IncrementTurnCount();
        }

        currentTurnUnit = GetCurrentUnit();

        CombatUnitManager.Instance.ResetOncePerTurnResources();
        
        if(currentTurnUnit.Faction == Faction.PC)
        {
            StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.StartPlayerTurn));
        }
        else
        {
            StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.StartMonsterTurn));
        }
    }
}
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

    public IEnumerator RollInitiative(bool reloadedPreviousSave)
    {
        if (!reloadedPreviousSave)
        {
            foreach (BaseUnit unit in CombatUnitManager.Instance.baseUnits)
            {
                int initiative = unit.RollInitiative();
                initiativeRolls.Add((unit, initiative));
                // Debug.Log($"{unit} rolled {initiative} for initiative");
            }

            initiativeRolls = initiativeRolls.OrderByDescending(x => x.Item2).ToList();

            // Debug.Log("The turn order is: ");
            for (int i = 0; i < initiativeRolls.Count; i++)
            {
                // Debug.Log(initiativeRolls[i].Item1.UnitName);
                DatabaseManager.Instance.ExecuteNonQuery($"INSERT INTO initiative_order (unit_id, turn_order) VALUES ({initiativeRolls[i].Item1.UnitID}, {i})");
            }

            DatabaseManager.Instance.ExecuteNonQuery($"UPDATE encounters SET in_progress = 1 WHERE id = {DatabaseManager.Instance.currentEncounter}");
            
            yield return StartCoroutine(CombatMenuManager.Instance.DisplayText($"The turn order is: {string.Join(", ", initiativeRolls)}"));
        }
        currentTurnUnit = GetCurrentUnit();

        yield return null;

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
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE initiative_order SET active = 0 WHERE unit_id = {PCID}");
    }

    public BaseUnit GetCurrentUnit()
    {
        BaseUnit nextUnit = null;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT unit_id, active FROM initiative_order WHERE turn_order = {GetInitiativeOrder()}",
            reader =>
            {
                while (reader.Read())
                {
                    if (Convert.ToBoolean(reader["active"]))
                    {
                        nextUnit = CombatUnitManager.Instance.GetUnitByID(Convert.ToInt32(reader["unit_id"]));
                    }
                }
            }
        );

        // Debug.Log("No unit found at initiative order " + GetInitiativeOrder());

        return nextUnit;
    }

    public void AdvanceTurn()
    {
        do
        {
            // Debug.LogWarning("Advancing Turn");

            IncrementInitiativeOrder();
            if (GetInitiativeOrder() >= GetNumberOfCombatants())
            {
                ResetInitiativeCount();
                IncrementTurnCount();
                // Debug.Log("Increasing turn count");
            }

            currentTurnUnit = GetCurrentUnit();
        } while (currentTurnUnit == null);

        // Debug.LogWarning("Current unit turn: "+currentTurnUnit);
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BaseMonster : BaseUnit
{
    public List<int> actionList = new List<int> {0,1};
    public List<int> validActions;
    public List<int> validTargets;

    public void EndTurn()
    {
        validActions.Clear();
    }

    public void CheckValidActions()
    {
        for (int i = 0; i < actionList.Count; i++)
        {
            bool valid = CheckValidity(actionList[i]);

            if (valid)
            {
                validActions.Add(actionList[i]);
            }
        }
    }

    private bool CheckValidity(int attackID)
    {
        bool valid = false;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT melee_range, normal_range, long_range FROM monster_attacks WHERE id = {attackID}",
            reader =>
            {
                while (reader.Read())
                {
                    int maxMeleeRange = 0;
                    int maxNormalRange = 0;
                    int maxLongRange = 0;

                    if (reader["melee_range"] != DBNull.Value)
                    {
                        maxMeleeRange = Convert.ToInt32(reader["melee_range"]) + GetCurrentSpeed();
                    }

                    if (reader["normal_range"] != DBNull.Value)
                    {
                        maxNormalRange = Convert.ToInt32(reader["normal_range"]) + GetCurrentSpeed();
                    }

                    if (reader["long_range"] != DBNull.Value)
                    {
                        maxLongRange = Convert.ToInt32(reader["long_range"]) + GetCurrentSpeed();
                    }

                    if (maxMeleeRange > 0)
                    {
                        List<Tile> targetTiles = CheckUnitsInRange(maxMeleeRange / 5);
                        for (int i = 0; i < targetTiles.Count; i++)
                        {
                            validTargets.Add(targetTiles[i].OccupiedUnit.UnitID);
                            valid = true;
                        }
                    }

                    if (maxNormalRange > 0)
                    {
                        List<Tile> targetTiles = CheckUnitsInRange(maxMeleeRange / 5);
                        for (int i = 0; i < targetTiles.Count; i++)
                        {
                            validTargets.Add(targetTiles[i].OccupiedUnit.UnitID);
                            valid = true;
                        }
                    }
                    
                    if (maxLongRange > 0)
                    {
                        List<Tile> targetTiles = CheckUnitsInRange(maxMeleeRange / 5);
                        for(int i = 0; i < targetTiles.Count; i++)
                        {
                            validTargets.Add(targetTiles[i].OccupiedUnit.UnitID);
                            valid = true;
                        }
                    }

                }
            }
        );

        return valid;
    }

    public int ChooseTarget()
    {
        return UnityEngine.Random.Range(0, validTargets.Count);
    }

    public int ChooseAttack()
    {
        return UnityEngine.Random.Range(0, validActions.Count);
    }

    public void AttackTarget(int targetID, int attackID)
    {
        int damage = 0;

        switch (attackID)
        {
            case 0:
                Debug.Log("Targeting unit "+targetID+" with attack 1: "+validActions[attackID]);
                damage = 1;
                break;
            case 1:
                Debug.Log("Targeting unit "+targetID+" with attack 2: "+validActions[attackID]);
                damage = 2;
                break;
            case 2:
                Debug.Log("Targeting unit "+targetID+" with attack 3: "+validActions[attackID]);
                damage = 3;
                break;
            default:
                Debug.Log("Invalid attack");
                break;
        }

        CombatUnitManager.Instance.DamageUnit(targetID, damage, false);
    }

    public List<Tile> CheckUnitsInRange(int range)
    {
        List<Tile> localTileList = CombatGridManager.Instance.tilesList;
        List<Tile> validTargetTileList = new List<Tile>();

        for (int i = 0; i < localTileList.Count; i++)
        {
            if(localTileList[i].OccupiedUnit != null && localTileList[i].OccupiedUnit.UnitID < 5) 
            {
                int distance = occupiedTile.CheckDistance(localTileList[i]);
                if (distance <= range)
                {
                    Debug.Log("There is a valid target " + distance + " tiles away");
                    validTargetTileList.Add(localTileList[i]);
                }
            }
        }

        return validTargetTileList;
    }

    public void MoveToUnit(int targetID)
    {
        int maxMovement = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(
            $"SELECT current_speed FROM unit_resources WHERE id = {UnitID}"));

        Tile targetUnitTile = CombatUnitManager.Instance.GetUnitByID(targetID).occupiedTile;

        List<Tile> path = GetPath(occupiedTile, targetUnitTile, maxMovement);

        if(path != null)
        {
            for(int i = 0; i < path.Count; i++)
            {
                path[i].MoveUnit(this);
                Debug.Log("Monster moved to tile: (" + occupiedTile.tileX + ", " + occupiedTile.tileY + ")");
            }
        }

    }

    List<Tile> GetPath(Tile originTile, Tile targetTile, int maxSteps)
    {
        var path = new List<Tile>();
        var visited = new HashSet<Vector2Int>();

        if (FindPathRecursive(originTile, targetTile, visited, path, maxSteps))
            return path;

        Debug.Log("There is no found path");
        return null;
    }

    public int GetCurrentSpeed()
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar(   //Get the width of the grid
            $"SELECT current_speed FROM unit_resources WHERE id = {UnitID}"
        ));
    }

    public bool FindPathRecursive(Tile currentTile, Tile targetTile, HashSet<Vector2Int> visited, List<Tile> path, int stepsRemaining)
    {
        // Debug.Log("Trying tile " +currentTile.tileX+", "+currentTile.tileY);

        if (stepsRemaining < 0)
        {
            // Debug.Log("No more steps remaining, stopped at tile " +currentTile.tileX+", "+currentTile.tileY);
            return false;
        }
        
        Vector2Int current = new Vector2Int(currentTile.tileX, currentTile.tileY);


        // ❌ Out of bounds or blocked
        if (currentTile == null)
        {
            // Debug.Log($"Current tile is null");
            return false;
        }
        else if (!currentTile.Walkable && currentTile.OccupiedUnit != this)
        {
            // Debug.Log($"Current tile is not walkable and its occupied unit is not the current unit");
            return false;
        }
        else if (visited.Contains(current))
        {
            // Debug.Log($"The current tile has already been visited");
            return false;
        }

        // ✅ Add to path + visited
        visited.Add(current);
        path.Add(currentTile);

        // 🎯 Reached target
        if ((currentTile.tileX == targetTile.tileX - 1 || currentTile.tileX == targetTile.tileX || currentTile.tileX == targetTile.tileX + 1) &&
            (currentTile.tileY == targetTile.tileY - 1 || currentTile.tileY == targetTile.tileY || currentTile.tileY == targetTile.tileY + 1))
            return true;

        // 🔁 Explore neighbors
        Vector2Int[] surroundingTiles = {
            new Vector2Int(currentTile.tileX - 1, currentTile.tileY - 1),
            new Vector2Int(currentTile.tileX - 1, currentTile.tileY),
            new Vector2Int(currentTile.tileX - 1, currentTile.tileY + 1),
            new Vector2Int(currentTile.tileX, currentTile.tileY - 1),
            new Vector2Int(currentTile.tileX, currentTile.tileY + 1),
            new Vector2Int(currentTile.tileX + 1, currentTile.tileY - 1),
            new Vector2Int(currentTile.tileX + 1, currentTile.tileY),
            new Vector2Int(currentTile.tileX + 1, currentTile.tileY + 1)
        };

        foreach (var tile in surroundingTiles)
        {
            // Debug.Log($"Checking tile {tile}");
            Tile nextTile = CombatGridManager.Instance.GetTileAtPosition(tile);
            // Debug.Log(nextTile == null);
            if (nextTile != null)
            {
                if (FindPathRecursive(nextTile, targetTile, visited, path, stepsRemaining - 1))
                    return true;
            }
        }

        // ❌ Dead end → backtrack
        path.RemoveAt(path.Count - 1);
        visited.Remove(current);
        return false;
    }
}

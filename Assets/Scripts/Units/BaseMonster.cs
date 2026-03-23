using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BaseMonster : BaseUnit
{
    public List<int> actionList = new List<int>();
    public List<int> validActions;
    public List<(BaseUnit, int, int)> validTargetsWithAttackAndPriority = new List<(BaseUnit, int, int)>();
    
    public void EndTurn()
    {
        validActions.Clear();
        validTargetsWithAttackAndPriority.Clear();
    }

    public IEnumerator CheckValidActions()
    {
        for (int i = 0; i < actionList.Count; i++)
        {
            bool valid = CheckValidity(actionList[i]);

            // Debug.Log($"Action {i} valid? - {valid}");

            if (valid)
            {
                validActions.Add(actionList[i]);
            }
        }
        yield return null;
    }

    private bool CheckValidity(int attackID)
    {
        bool hasValidAttack = false;
        
        for (int i = 0; i < GetilesWithActivePCs().Count; i++) //For each tile that has a PC in it
        {
            Tile tile = GetilesWithActivePCs()[i]; //Get the tile
            BaseUnit target = GetilesWithActivePCs()[i].OccupiedUnit; //Get the PC in that tile

            List<Tile> pathToTarget = GetPathToBestAttackTile(target.occupiedTile, attackID);

            bool pathToTargetExists = pathToTarget != null && pathToTarget.Count > 0;
            bool alreadyInRange = false;

            if (occupiedTile.CheckDistanceInTiles(tile) <= GetMeleeRange(attackID) || occupiedTile.CheckDistanceInTiles(tile) <= GetNormalRange(attackID))
            {
                alreadyInRange = true;
            }

            if (pathToTargetExists || alreadyInRange)
            {
                Tile destinationTile = occupiedTile;
                
                if (pathToTargetExists)
                {
                    destinationTile = pathToTarget[^1]; //Get the tile the monster will end up at when it tries to attack                
                }
                
                // Debug.Log($"{UnitName} will end at {destinationTile.tileX}, {destinationTile.tileY} when they attack {target.UnitName}");

                //If that tile is within either the melee or normal range
                if (destinationTile.CheckDistanceInTiles(tile) <= GetMeleeRange(attackID) ||
                    destinationTile.CheckDistanceInTiles(tile) <= GetNormalRange(attackID))
                {
                    validTargetsWithAttackAndPriority.Add((target, attackID, GetPriority(attackID))); //Add it at normal priority
                }
                else if (destinationTile.CheckDistanceInTiles(tile) <= GetLongRange(attackID)) //If it's only within long range
                {
                    Debug.Log("Cannot get in normal or melee range, attacking at long range");
                    validTargetsWithAttackAndPriority.Add((target, attackID, GetPriority(attackID) - 1)); //Add it with lower priority
                }
                else
                {
                    Debug.Log($"Attack {attackID} cannot get within range of {target.UnitName}");
                    continue;
                }

                hasValidAttack = true;
            }
        }

        return hasValidAttack;
    }

    
    public bool PCsInAdjacentTiles(Tile tile)
    {
        List<Tile> adjacentTiles = GetNeighbors(tile);

        foreach (Tile neighbor in adjacentTiles)
        {
            if(neighbor.OccupiedUnit != null && neighbor.OccupiedUnit.Faction == Faction.PC)
            {
                return true;
            }
        }
        return false;
    }

    public (BaseUnit, int) ChooseTargetAndAttack()
    {
        if(validTargetsWithAttackAndPriority.Count > 0)
        {
            for(int i = validTargetsWithAttackAndPriority.Max(t => t.Item3); i >= 0 ; i--) //Starting with the highest priority in the list
            {
                int numberOfOptions = 0;
                for(int j = 0; j < validTargetsWithAttackAndPriority.Count; j++) //For every item in the list
                {
                    if (validTargetsWithAttackAndPriority[j].Item3 == i) //If the item has the same priority as the priority being searched
                    {
                        numberOfOptions++;
                    }
                }

                if(numberOfOptions > 0) //After all items have been searched, if there is at least one attack of that priority, pick one of them
                {
                    int chosenOption = UnityEngine.Random.Range(0, numberOfOptions);

                    // Debug.Log($"{UnitName} is attacking {validTargetsWithAttackAndPriority[chosenOption].Item1.UnitName} with {validTargetsWithAttackAndPriority[chosenOption].Item2}, which has priority {GetPriority(validTargetsWithAttackAndPriority[chosenOption].Item2)}");
                    
                    return (validTargetsWithAttackAndPriority[chosenOption].Item1, validTargetsWithAttackAndPriority[chosenOption].Item2); //Return that item's target and attack
                }
            }
        }

        Debug.Log("No valid targets found");
        return (null, -1);        

    }

    public IEnumerator AttackTarget(BaseUnit target, int attackID)
    {
        int targetDistance = occupiedTile.CheckDistanceInTiles(target.occupiedTile);

        if(targetDistance <= GetMeleeRange(attackID) || targetDistance <= GetNormalRange(attackID) || targetDistance <= GetLongRange(attackID))
        {
            yield return StartCoroutine(CombatActions.MonsterAttack(this, target, attackID));
        }
        else
        {
            Debug.LogWarning("The attack is not within any of the three ranges, yet was selected as the action");
        }

        // CombatUnitManager.Instance.GetUnitByID(targetID).TakeDamage(damage, false);

    }
    
    public List<Tile> GetilesWithActivePCs()
    {
        List<Tile> validTargetTileList = new List<Tile>();

        for (int i = 0; i < CombatUnitManager.Instance.activePCIDs.Count; i++)
        {
            BaseUnit pc = CombatUnitManager.Instance.GetUnitByID(CombatUnitManager.Instance.activePCIDs[i]);

            if (pc.GetCondition("dying") || pc.GetCondition("dead") || pc.GetCondition("unconscious"))
            {
                // Debug.Log($"{potentialTarget.UnitName} is dying or dead.");
                continue;
            }
            else
            {
                validTargetTileList.Add(pc.occupiedTile);
            }
        }

        return validTargetTileList;
    }

    public IEnumerator MoveToTile(List<Tile> path)
    {
        if (path != null)
        {
            for (int i = 0; i < path.Count; i++)
            {
                path[i].MoveUnit(this);
                // Debug.Log("Monster moved to tile: (" + occupiedTile.tileX + ", " + occupiedTile.tileY + ")");
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    public List<Tile> GetPathToBestAttackTile(Tile targetTile, int attackID)
    {
        var openSet = new List<Tile> { occupiedTile };
        var cameFrom = new Dictionary<Tile, Tile>();

        var gScore = new Dictionary<Tile, int>();
        gScore[occupiedTile] = 0;

        var fScore = new Dictionary<Tile, int>();
        fScore[occupiedTile] = Heuristic(occupiedTile, targetTile);

        int meleeMax = GetMeleeRange(attackID);
        int normalMax = GetNormalRange(attackID);
        int longMax = GetLongRange(attackID);

        Tile bestTile = null;
        int bestScore = int.MinValue;

        // fallback
        Tile closestTile = occupiedTile;
        int closestDistance = Heuristic(occupiedTile, targetTile);

        while (openSet.Count > 0)
        {
            Tile current = openSet.OrderBy(t => fScore.ContainsKey(t) ? fScore[t] : int.MaxValue).First();
            openSet.Remove(current);

            int distToTarget = Heuristic(current, targetTile);

            // fallback tracking
            if (distToTarget < closestDistance)
            {
                closestDistance = distToTarget;
                closestTile = current;
            }

            // classify range
            RangeType? rangeType = null;

            if (distToTarget <= meleeMax)
                rangeType = RangeType.Melee;
            else if (distToTarget <= normalMax)
                rangeType = RangeType.Normal;
            else if (distToTarget <= longMax)
                rangeType = RangeType.Long;

            if (rangeType != null)
            {
                bool safe = !PCsInAdjacentTiles(current);

                int priorityScore = GetRangePriority(rangeType.Value) * 1000;
                int safetyScore = (rangeType == RangeType.Melee) ? 0 : (safe ? 100 : 0);

                int idealDistance = GetIdealDistance(rangeType.Value, meleeMax, normalMax, longMax);
                int distanceScore = -Mathf.Abs(distToTarget - idealDistance);

                int totalScore = priorityScore + safetyScore + distanceScore;

                if (totalScore > bestScore)
                {
                    bestScore = totalScore;
                    bestTile = current;
                }
            }

            foreach (Tile neighbor in GetNeighbors(current))
            {
                if (neighbor == null) continue;
                if ((!neighbor.isWalkable || neighbor.OccupiedUnit != null) && neighbor.OccupiedUnit != this) continue;

                int dx = Mathf.Abs(neighbor.tileX - current.tileX);
                int dy = Mathf.Abs(neighbor.tileY - current.tileY);

                bool isDiagonal = (dx == 1 && dy == 1);

                int moveCost = isDiagonal ? 5 : 5;

                int tentativeG = gScore[current] + moveCost;

                if (tentativeG > GetCurrentSpeed()) continue;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        if (bestTile != null)
            return ReconstructPath(cameFrom, bestTile);

        if (closestTile != occupiedTile)
            return ReconstructPath(cameFrom, closestTile);

        return null;
    }

    int GetIdealDistance(RangeType type, int meleeMax, int normalMax, int longMax)
    {
        switch (type)
        {
            case RangeType.Melee:
                return meleeMax;

            case RangeType.Normal:
                return normalMax;

            case RangeType.Long:
                return longMax;

            default:
                return 0;
        }
    }

    public enum RangeType
    {
        Melee,
        Normal,
        Long
    }

    int GetRangePriority(RangeType type)
    {
        switch (type)
        {
            case RangeType.Melee: return 3;
            case RangeType.Normal: return 2;
            case RangeType.Long: return 1;
            default: return 0;
        }
    }

    int Heuristic(Tile a, Tile b)
    {
        // Manhattan distance (good for grids)
        return Mathf.Abs(a.tileX - b.tileX) + Mathf.Abs(a.tileY - b.tileY);
    }

    static readonly Vector2Int[] Directions = new Vector2Int[]
    {
        new Vector2Int(-1,  0),
        new Vector2Int( 0, -1),
        new Vector2Int( 0,  1),
        new Vector2Int( 1,  0),
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  1),
        new Vector2Int( 1, -1),
        new Vector2Int( 1,  1)
    };

    List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        foreach (var dir in Directions)
        {
            var pos = new Vector2Int(tile.tileX + dir.x, tile.tileY + dir.y);
            Tile t = CombatGridManager.Instance.GetTileAtPosition(pos);

            if (t != null)
                neighbors.Add(t);
        }

        return neighbors;
    }

    List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new List<Tile>();

        while (cameFrom.ContainsKey(current))
        {
            path.Insert(0, current);
            current = cameFrom[current];
        }

        return path;
    }



    public int GetHitMod(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT hit_modifier FROM monster_attacks WHERE id = {attackID}"));
    }

    public int GetDiceNumber(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_number FROM monster_attacks WHERE id = {attackID}"));
    }

    public int GetDiceSize(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_size FROM monster_attacks WHERE id = {attackID}"));
    }

    public int GetDamageBonus(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT damage_bonus FROM monster_attacks WHERE id = {attackID}"));
    }

    public int GetDamageType(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT damage_type FROM monster_attacks WHERE id = {attackID}"));
    }

    public int GetMeleeRange(int attackID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT melee_range FROM monster_attacks WHERE id = {attackID}");
        
        if(range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }

    public int GetNormalRange(int attackID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT normal_range FROM monster_attacks WHERE id = {attackID}");
        
        if(range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }

    public int GetLongRange(int attackID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT long_range FROM monster_attacks WHERE id = {attackID}");

        if (range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }
    
    public int GetPriority(int attackID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT priority FROM monster_attacks WHERE id = {attackID}"));
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseMonster : BaseUnit
{
    public List<(int, int)> actionList = new List<(int, int)>();
    public List<int> validActions;
    public List<(BaseUnit, int, int)> validTargetsWithAttackAndPriority = new List<(BaseUnit, int, int)>();
    // public int attackMod;
    // public int saveDC;
    // public int proficiency;
    // public string BaseName;
    // public string DisplayName;

    public override void Initialize(){}
    
    public void ClearActionList()
    {
        validActions.Clear();
        validTargetsWithAttackAndPriority.Clear();
    }

    public IEnumerator CheckValidActions()
    {
        for (int i = 0; i < actionList.Count; i++)
        {
            int actionID = actionList[i].Item1;

            bool valid = false;

            int actionType = GetActionType(actionID);

            if (actionType == 0)
            {
                valid = true;
                validTargetsWithAttackAndPriority.Add((this, actionID, actionList[i].Item2));
            }
            else if (actionType == 1) //Checks for attacks
            {
                valid = CheckValidAttack(actionID);
            }
            else if (actionType == 2) //Check for saving throws
            {
                
            }

            // Debug.Log($"Action {i} valid? - {valid}");

            if (valid)
            {
                validActions.Add(actionList[i].Item1);
            }
        }
        yield return null;
    }

    private bool CheckValidAttack(int attackID)
    {
        // Debug.Log("Checking action " + attackID);
        bool hasValidAttack = false;

        List<Tile> activePCTiles = GetilesWithActivePCs();
        
        for (int i = 0; i < activePCTiles.Count; i++) //For each tile that has a PC in it
        {
            Tile tile = activePCTiles[i]; //Get the tile
            BaseUnit target = tile.OccupiedUnit; //Get the PC in that tile

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
                    // Debug.Log($"Action {attackID} should have priority {GetPriority(attackID)}");
                    validTargetsWithAttackAndPriority.Add((target, attackID, GetPriority(attackID))); //Add it at normal priority
                }
                else if (destinationTile.CheckDistanceInTiles(tile) <= GetLongRange(attackID)) //If it's only within long range
                {
                    // Debug.Log("Cannot get in normal or melee range, attacking at long range");
                    validTargetsWithAttackAndPriority.Add((target, attackID, GetPriority(attackID)-1)); //Add it with lower priority
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

    public (BaseUnit, int) ChooseTargetAndAction()
    {
        if (validTargetsWithAttackAndPriority.Count > 0)
        {
            List<(BaseUnit, int)> options = new List<(BaseUnit, int)>();

            for (int i = 0; i < validTargetsWithAttackAndPriority.Count; i++)
            {
                // Debug.Log("Checking option " + validTargetsWithAttackAndPriority[j]);
                if (validTargetsWithAttackAndPriority[i].Item3 == validTargetsWithAttackAndPriority.Max(t => t.Item3)) //If the item has the highest priority
                {
                    // Debug.Log($"Option {j} has priority {i}");
                    options.Add((validTargetsWithAttackAndPriority[i].Item1, validTargetsWithAttackAndPriority[i].Item2));
                }
            }

            if (options.Count > 0) //After all items have been searched, if there is at least one attack of that priority, pick one of them
            {
                //For all options that are of the highest priority, only selects those that target the nearest PC, or target a monster
                List<(BaseUnit, int)> selectableOptions = new();
                List<BaseUnit> closestUnits = ListOfClosestPCs();

                foreach ((BaseUnit, int) option in options)
                {
                    if (option.Item1 == this || closestUnits.Contains(option.Item1))
                    {
                        selectableOptions.Add(option);
                    }
                }

                return selectableOptions[UnityEngine.Random.Range(0, selectableOptions.Count)];

                // Debug.Log($"{UnitName} is attacking {validTargetsWithAttackAndPriority[chosenOption].Item1.UnitName} with 
                // {validTargetsWithAttackAndPriority[chosenOption].Item2}, which has priority {GetPriority(validTargetsWithAttackAndPriority[chosenOption].Item2)}");

                //== This selects an attack at random, while the above selects the first option (usually melee) ==

                // BaseUnit closestPC = GetClosestPC(unitList); //Select one of the closest PCs at random

                // List<(BaseUnit, int)> validAtacksOnClosestPC = new();
                // for(int i = 0; i < options.Count; i++)
                // {
                //     if(options[i].Item1 == closestPC)
                //         validAtacksOnClosestPC.Add((options[i].Item1, options[i].Item2)); //Get every attack option that targets that
                // }

                // var result = validAtacksOnClosestPC[UnityEngine.Random.Range(0, validAtacksOnClosestPC.Count)];

                // // Debug.Log($"{UnitName} is attacking {validTargetsWithAttackAndPriority[chosenOption].Item1.UnitName} with 
                // // {validTargetsWithAttackAndPriority[chosenOption].Item2}, which has priority {GetPriority(validTargetsWithAttackAndPriority[chosenOption].Item2)}");

                // return (result.Item1, result.Item2); //Return that item's target and attackID
            }
            // Debug.Log($"There are {validTargetsWithAttackAndPriority.Count} options to check");
            // for (int i = validTargetsWithAttackAndPriority.Max(t => t.Item3); i >= 0; i--) //Starting with the highest priority in the list.
            // {
                // Debug.Log($"Starting at priority {i}");
            // }
        }

        // Debug.Log("No valid targets found");
        return (null, -1);

    }

    public IEnumerator AttackTarget(BaseUnit target, int attackID)
    {
        int targetDistance = occupiedTile.CheckDistanceInTiles(target.occupiedTile);

        if(targetDistance <= GetMeleeRange(attackID) || targetDistance <= GetNormalRange(attackID) || targetDistance <= GetLongRange(attackID))
        {
            yield return StartCoroutine(MonsterActions.Attack(this, target, attackID));
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

            if (!pc.IsActive())
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
        if (path == null) yield break;

        for (int i = 0; i < path.Count; i++)
        {
            yield return StartCoroutine(path[i].MoveUnit(this, false));

            if (TurnUtility.ShouldStop(this))
            {
                // Debug.Log("Turn should stop");
                yield break;

            }
        }
    }
    
    public List<Tile> GetPathToBestAttackTile(Tile targetTile, int attackID)
    {
        return GetPathToBestAttackTile(targetTile, attackID, false);
    }

    public List<Tile> GetPathToBestAttackTile(Tile targetTile, int attackID, bool getAsCloseAsPossible)
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

            // int distToTarget = Heuristic(current, targetTile);
            // int distToTarget = GetPathDistance(current, targetTile);
            Dictionary<Tile, int> distanceMap = GenerateDistanceMap(targetTile);
            int distToTarget = distanceMap.ContainsKey(current) ? distanceMap[current] : int.MaxValue;

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

                int moveCost = neighbor.CostsExtra(this) ? 10 : 5;

                int tentativeG = gScore[current] + moveCost;

                if (tentativeG > GetResource("current_speed") && !getAsCloseAsPossible) continue;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, targetTile);

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

    public List<Tile> GetAsCloseAsPossibleToTarget(Tile targetTile, int attackID)
    {
        return GetPathToBestAttackTile(targetTile, attackID, true);
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

    int GetPathDistance(Tile start, Tile target)
    {
        var openSet = new List<Tile> { start };
        var gScore = new Dictionary<Tile, int> { [start] = 0 };

        while (openSet.Count > 0)
        {
            Tile current = openSet.OrderBy(t => gScore[t]).First();
            openSet.Remove(current);

            if (current == target)
                return gScore[current];

            foreach (Tile neighbor in GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.isWalkable)
                    continue;

                int tentative = gScore[current] + 1;

                if (!gScore.ContainsKey(neighbor) || tentative < gScore[neighbor])
                {
                    gScore[neighbor] = tentative;
                    openSet.Add(neighbor);
                }
            }
        }

        return int.MaxValue; // unreachable
    }

    Dictionary<Tile, int> GenerateDistanceMap(Tile target)
    {
        var distances = new Dictionary<Tile, int>();
        var queue = new Queue<Tile>();

        distances[target] = 0;
        queue.Enqueue(target);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor == null || !neighbor.isWalkable)
                    continue;

                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return distances;
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

    public int GetHitMod(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT hit_modifier FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetDiceNumber(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_number FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetDiceSize(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT dice_size FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetDamageBonus(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT damage_bonus FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetDamageType(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT damage_type FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetMeleeRange(int actionID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT melee_range FROM monster_actions WHERE id = {actionID}");
        
        if(range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }

    public int GetNormalRange(int actionID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT normal_range FROM monster_actions WHERE id = {actionID}");
        
        if(range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }

    public int GetLongRange(int actionID)
    {
        var range = DatabaseManager.Instance.ExecuteScalar($"SELECT long_range FROM monster_actions WHERE id = {actionID}");

        if (range == DBNull.Value)
        {
            return 0;
        }
        else
        {
            return Convert.ToInt32(range) / 5;
        }
    }

    public int GetPriority(int actionID)
    {
        return actionList.FirstOrDefault(x => x.Item1 == actionID).Item2;
        // return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT priority FROM monster_actions WHERE id = {actionID}"));
    }

    public int GetActionType(int actionID)
    {
        return Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT action_type FROM monster_actions WHERE id = {actionID}"));
    }

    public BaseUnit GetClosestPC()
    {
        return GetClosestPC(GetilesWithActivePCs());
    }

    public BaseUnit GetClosestPC(List<BaseUnit> unitList)
    {
        List<Tile> tileList = new();
        foreach (BaseUnit unit in unitList)
        {
            tileList.Add(unit.occupiedTile);
        }

        return GetClosestPC(tileList);
    }

    public BaseUnit GetClosestPC(List<Tile> tileList)
    {
        List<Tile> closestTiles = new(); //List of tiles
        int shortestDistance = 1000;

        foreach (Tile tile in tileList)
        {
            int distance = occupiedTile.CheckDistanceInTiles(tile);
            // Debug.Log($"Distance to {tile.OccupiedUnit.UnitName} is {distance}");
            if (distance <= shortestDistance)
            {
                if (distance < shortestDistance)
                {
                    // Debug.Log($"This is the new shortest distance");
                    closestTiles.Clear(); //If it's closer than any other tile, none of the other options are valid anymore
                    shortestDistance = distance;
                }

                // Debug.Log($"{tile.OccupiedUnit.UnitName} is the closest target");
                closestTiles.Add(tile); //This option becomes valid
            }
        }

        // Debug.Log($"valid tile targets include:");
        // foreach(Tile tile in closestTiles)
        // {
        //     Debug.Log($"{tile.OccupiedUnit.UnitName}");
        // }

        int randomSelection = UnityEngine.Random.Range(0, closestTiles.Count);

        // Debug.Log("Randomly selected unit is " + closestTiles[randomSelection].OccupiedUnit);

        return closestTiles[randomSelection].OccupiedUnit; //Pick a random tile among the closest ones
    }

    public List<BaseUnit> ListOfClosestPCs()
    {
        List<BaseUnit> closestUnits = new(); //List of tiles
        int shortestDistance = 1000;

        foreach (Tile tile in GetilesWithActivePCs())
        {
            int distance = occupiedTile.CheckDistanceInTiles(tile);
            // Debug.Log($"Distance to {tile.OccupiedUnit.UnitName} is {distance}");
            if (distance <= shortestDistance)
            {
                if (distance < shortestDistance)
                {
                    // Debug.Log($"This is the new shortest distance");
                    closestUnits.Clear(); //If it's closer than any other tile, none of the other options are valid anymore
                    shortestDistance = distance;
                }

                // Debug.Log($"{tile.OccupiedUnit.UnitName} is the closest target");
                closestUnits.Add(tile.OccupiedUnit); //This option becomes valid
            }
        }

        return closestUnits;
    }

    public IEnumerator MoveToClosestPC()
    {
        List<Tile> pathToTake;
        if(actionList.Count == 0)
        {
            pathToTake = GetAsCloseAsPossibleToTarget(GetClosestPC().occupiedTile, 0);
        }
        else
        {
            pathToTake = GetAsCloseAsPossibleToTarget(GetClosestPC().occupiedTile, actionList[0].Item1); //Try to get as close as possible to them
        }
        yield return StartCoroutine(MoveToTile(pathToTake));
    }

    public IEnumerator ExecuteAction(int actionID, BaseUnit target)
    {
        if (GetActionType(actionID) == 1)
        {
            var path = GetPathToBestAttackTile(target.occupiedTile, actionID);
            
            yield return StartCoroutine(MoveToTile(path));

            if (TurnUtility.ShouldStop(this))
                yield break;

            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(AttackTarget(target, actionID));

            if (TurnUtility.ShouldStop(this))
                yield break; ;

            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            switch (actionID)
            {
                case 2:
                    Dodge();
                    yield return StartCoroutine(MoveToClosestPC());
                    break;
                case 3:
                    Dash(); //Take the dash action
                    yield return StartCoroutine(MoveToClosestPC());
                    break;

            }
        }
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance;
    private Action<Tile> onTileSelected;

    public void StartTargeting(Action<Tile> callback)
    {
        onTileSelected = callback;
        CombatStateManager.Instance.ChangeState(GameState.SelectTarget);
    }

    public void SelectTile(Tile tile)
    {
        if (CombatStateManager.Instance.GameState != GameState.SelectTarget)
            return;

        onTileSelected?.Invoke(tile);
        onTileSelected = null;

        CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
    }

}
using System;
using System.Diagnostics;
using UnityEngine;

public class OpportunityAttack : Reaction<MoveContext>
{
    public OpportunityAttack()
    {
        Name = "Opportunity Attack";
        Trigger = ReactionTrigger.UnitMoves;
        CostsReaction = true;
    }

    public override bool CanTrigger(MoveContext context, BaseUnit owner)
    {

        if (context.TriggeringUnit.Faction != owner.Faction &&
        owner.occupiedTile.CheckDistanceInTiles(context.originTile) == 1 &&
        owner.occupiedTile.CheckDistanceInTiles(context.destinationTile) > 1)
        {
            // CombatMenuManager.Instance.DisplayText($"{owner.UnitName} can take an opportunity attack");
            return true;
        }
        else
        {
            // CombatMenuManager.Instance.DisplayText($"{owner.UnitName} can NOT take an opportunity attack");
            return false;
        }
    }

    public override void Execute(MoveContext context, BaseUnit owner, Action onComplete)
    {
        owner.UseReaction();
        context.TriggeringUnit.TakeDamage(5, false);
        UnityEngine.Debug.Log($"{context.TriggeringUnit} takes 5 damage");
        onComplete?.Invoke();
    }
}
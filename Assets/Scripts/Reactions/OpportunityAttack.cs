using System;
using System.Collections;
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
        owner.occupiedTile.CheckDistanceInTiles(context.destinationTile) > 1 &&
        !context.TriggeringUnit.GetCondition("disengaging"))
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

    public override IEnumerator Execute(MoveContext context, BaseUnit owner, Action onComplete)
    {
        if (CombatStateManager.Instance.processing)
            yield break;
        
        CombatStateManager.Instance.processing = true;
        // owner.UseResource("reaction");
        // UnityEngine.Debug.LogWarning("About to call TakeDamage()");
        if (owner.Faction == Faction.PC)
        {
            yield return CombatUnitManager.Instance.StartCoroutine(CombatActions.AttackWithWeapon(owner, context.TriggeringUnit, ((BasePC)owner).GetMainhandID(), (success) => { }));
        }
        else
        {
            yield return CombatUnitManager.Instance.StartCoroutine(CombatActions.AttackWithWeapon(owner, context.TriggeringUnit, 0, (success) => { }));
        }
        CombatStateManager.Instance.processing = false;
        // UnityEngine.Debug.LogWarning("Unit took damage, calling OnComplete");
        onComplete?.Invoke();
    }
}
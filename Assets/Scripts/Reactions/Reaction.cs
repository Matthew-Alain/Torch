using System;

public abstract class Reaction<TContext> : IReaction where TContext : BaseContext
{
    public string Name;
    public ReactionTrigger Trigger;
    public bool CostsReaction;

    public abstract bool CanTrigger(TContext context, BaseUnit owner);
    public abstract void Execute(TContext context, BaseUnit owner, Action onComplete);

    // Interface implementations (bridge)
    bool IReaction.CanTrigger(BaseContext context, BaseUnit owner)
    {
        if (context is TContext typedContext)
            return CanTrigger(typedContext, owner);

        return false;
    }

    void IReaction.Execute(BaseContext context, BaseUnit owner, Action onComplete)
    {
        if (context is TContext typedContext)
            Execute(typedContext, owner, onComplete);
        else
            onComplete?.Invoke();
    }

    string IReaction.Name => Name;
    ReactionTrigger IReaction.Trigger => Trigger;
    bool IReaction.CostsReaction => CostsReaction;
}

public enum ReactionTrigger
{
    StartTurn,
    UnitMoves,
    AttackDeclared,
    MissWithAttack,
    HitWithAttack,
    BeforeTakeDamage,
    AfterTakeDamage,
    AllyHitByAttack,
    BeforeAllyTakesDamage,
    AfterAllyTakesDamage,
    Roll1,
    BeforeDamageDealt,
    AfterDamageDealt,
    OnMove,
    FailSave,
    UseInspiration,
    BeforeCastSpell,
    AfterCastSpell,

}
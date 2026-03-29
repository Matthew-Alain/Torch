using System;

public interface IReaction
{
    string Name { get; }
    ReactionTrigger Trigger { get; }
    bool CostsReaction { get; }

    bool CanTrigger(BaseContext context, BaseUnit owner);
    void Execute(BaseContext context, BaseUnit owner, Action onComplete);
}
using System.Linq;

public class Spells
{
    public void Fireball(BaseUnit user)
    {
        CombatStateManager.Instance.StartTileSelection(
            (tile) =>
            {
                int radius = 2;

                var targets = AOEHelper.GetUnitsInRadius(tile, radius);
                // .Where(u => u.Faction == Faction.Monster).ToList(); //If it only affects enemies

                var context = new ActionContext
                {
                    TriggeringUnit = user,
                    Targets = targets,
                    Damage = 10
                };

                ReactionManager.Instance.CheckForReactions(
                    ReactionTrigger.BeforeDamageDealt,
                    context,
                    () =>
                    {
                        foreach (var target in context.Targets)
                        {
                            target.TakeDamage(context.Damage, false);
                        }
                    }
                );

                user.UseResource("major_action");
            },
            (tile) =>
            {
                int maxRange = 6;

                int distance = user.occupiedTile.CheckDistanceInTiles(tile);

                if (distance > maxRange)
                    return (false, "That location is out of range");

                if (user.GetResource("major_action") <= 0)
                    return (false, "No major action available");

                return (true, "");
            }
        );
    }
}
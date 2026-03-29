public class SpeciesFeatures
{
    public void HealingHands(BasePC user)
    {
        if (user.UseResource("major_action"))
        {
            CombatStateManager.Instance.StartTargetSelection(TargetType.PC, (target) =>
            {
                int result = DiceRoller.Roll(user.GetPB(), 4);
                target.RestoreHealth(result);
            });

        }
    }
}
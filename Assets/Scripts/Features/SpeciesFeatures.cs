public class SpeciesFeatures
{
    public static void HealingHands(BasePC user)
    {
        if (user.UseResource("major_action"))
        {
            user.StartCoroutine(CombatStateManager.Instance.StartTargetSelection(TargetType.PC, user, 1, (target) =>
            {
                int result = DiceRoller.Roll(user.GetPB(), 4);
                target.StartCoroutine(target.RestoreHealth(result));
            }));
        }
    }

    public static void CelestialRevelation1(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void CelestialRevelation2(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void CelestialRevelation3(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void DraconicFlight(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void Stonecunning(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void LargeForm(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }

    public static void CloudsJaunt(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {
            
        }
    }
    
    public static void AdrenalineRush(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {
            CombatActions.Dash(user);
            user.SetTempHP(user.GetPB());
        }
    }
}
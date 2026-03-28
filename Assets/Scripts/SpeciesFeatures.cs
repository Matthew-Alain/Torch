
using UnityEngine;

public class SpeciesFeatures: MonoBehaviour
{
    public void HealingHands(BasePC user)
    {
        if (user.UseResource("major_action"))
        {
            StartCoroutine(CombatStateManager.Instance.SelectTarget(TargetType.PC, target =>
            {
                int result = DiceRoller.Roll(user.GetPB(), 4);
                target.RestoreHealth(result);
            }));

        }
    }
}
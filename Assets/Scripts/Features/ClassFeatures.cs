
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEditor.Tilemaps;

public class ClassFeatures
{
    public static void Rage(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {
            if (user.GetSubclass() == 1)
            {
                CombatActions.Dash(user);
                CombatActions.Disengage(user);
            }
        }
    }
    
    public static void MaintainRage(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {
            
        }
    }

    public static void EagleDashDisengage(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {
            CombatActions.Dash(user);
            CombatActions.Disengage(user);
        }
    }

    public static void OpenWarriorOfTheGodsMenu(BasePC user)
    {
        CombatMenuManager.Instance.OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> options = new List<MenuOption>()
            {
                new MenuOption("1", () => WarriorOfTheGods(user, 1), () => true, () => user.GetResource("warrior_of_the_gods_available") >= 1),
                new MenuOption("2", () => WarriorOfTheGods(user, 2), () => true, () => user.GetResource("warrior_of_the_gods_available") >= 2),
                new MenuOption("3", () => WarriorOfTheGods(user, 3), () => true, () => user.GetResource("warrior_of_the_gods_available") >= 3),
                new MenuOption("4", () => WarriorOfTheGods(user, 4), () => true, () => user.GetResource("warrior_of_the_gods_available") >= 4),
                new MenuOption("Back", () => CombatMenuManager.Instance.CloseMenu(), () => true, () => true)
            };

            return options;
        });
    }

    public static void WarriorOfTheGods(BasePC user, int amount)
    {
        if (user.UseResource("minor_action"))
        {
            int total = 0;
            for (int i = 0; i < amount; i++)
            {
                user.UseResource("warrior_of_the_gods_available");
                total += DiceRoller.Roll("1d12");
            }
            user.RestoreHealth(total);
        }
    }
    
    
    public static void BardicInspiration(BasePC user)
    {
        if (user.UseResource("minor_action"))
        {

        }
    }
}
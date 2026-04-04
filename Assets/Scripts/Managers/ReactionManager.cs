using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReactionManager : MonoBehaviour
{
    public static ReactionManager Instance;

    private List<BaseUnit> allUnits = new List<BaseUnit>();

    void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //This just allows manager scripts to be stored in a folder in the editor for organization, but during runtime, get deteached to avoid errors
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from parent
        }

        //Now safe to create a new instance
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void RegisterUnit(BaseUnit unit)
    {
        allUnits.Add(unit);
    }

    public IEnumerator CheckForReactions(ReactionTrigger trigger, BaseContext context)
    {
        foreach (var unit in allUnits)
        {
            if (TurnUtility.ShouldStop(context.TriggeringUnit))
            {
                // Debug.LogWarning($"Triggering unit is null, exiting loop");
                break;
            }

            if (unit == null || unit.GetCondition("unconscious") || unit.GetCondition("dying") || unit.GetCondition("dead"))
            {
                // Debug.LogWarning($"Unit {unit.UnitName} is unable to react");
                continue;
            }

            // Debug.Log("Checking reaction for "+unit.UnitName);

            var validReactions = unit.Reactions
                .Where(r => r.Trigger == trigger && r.CanTrigger(context, unit))
                .ToList();

            if (validReactions.Count == 0)
                continue;

            // Debug.Log($"{unit.UnitName} has a reaction");

            if (unit.Faction == Faction.Monster)
            {
                int option = Random.Range(0, validReactions.Count);
                var reaction = validReactions[option];
                // Debug.Log($"{unit.UnitName} is using reaction {option}");

                if (reaction.CostsReaction && unit.GetResource("reaction") <= 0)
                    continue;

                if (reaction.CostsReaction)
                    unit.UseResource("reaction");

                // Debug.Log($"{unit.UnitName} is using reaction {option}");

                bool finished = false;
                reaction.Execute(context, unit, () => finished = true);
                yield return new WaitUntil(() => finished || TurnUtility.ShouldStop(context.TriggeringUnit));
            }
            else
            {
                bool finished = false;

                List<MenuOption> options = new List<MenuOption>();

                foreach (var reaction in validReactions)
                {
                    if (reaction.CostsReaction && unit.GetResource("reaction") <= 0)
                    {
                        continue;
                    }

                    options.Add(new MenuOption(reaction.Name, () =>
                    {
                        if (reaction.CostsReaction)
                        {
                            unit.UseResource("reaction");
                        }

                        reaction.Execute(context, unit, () => finished = true);
                    }));
                }

                options.Add(new MenuOption("Skip", () => finished = true));

                // Debug.LogWarning("Opening menu to check for reactions");
                CombatMenuManager.Instance.OpenMenu(() => options);

                yield return new WaitUntil(() => finished || TurnUtility.ShouldStop(context.TriggeringUnit));

                // Debug.LogWarning("Reaction selected, closing menu");
                CombatMenuManager.Instance.CloseMenu();
            }
        }
        // Debug.LogWarning($"Finished checking for reactions, breaking");
        yield break;
    }
    
    // public IEnumerator CheckForReactionsCoroutine(ReactionTrigger trigger, BaseContext context)
    // {
    //     bool done = false;

    //     CheckForReactions(trigger, context, () =>
    //     {
    //         done = true;
    //     });

    //     yield return new WaitUntil(() => done);
    // }
}
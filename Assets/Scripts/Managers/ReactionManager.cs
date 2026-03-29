using System;
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

    public void CheckForReactions(ReactionTrigger trigger, BaseContext context, Action onComplete)
    {
        StartCoroutine(HandleReactions(trigger, context, onComplete));
    }

    private IEnumerator HandleReactions(ReactionTrigger trigger, BaseContext context, Action onComplete)
    {
        foreach (var unit in allUnits)
        {
            // Debug.Log("Checking reaction for "+unit.UnitName);


            var validReactions = unit.Reactions
                .Where(r => r.Trigger == trigger && r.CanTrigger(context, unit))
                .ToList();

            if (validReactions.Count > 0)
            {
                // Debug.Log($"{unit.UnitName} has a reaction");

                if (unit.Faction == Faction.Monster)
                {
                    int option = UnityEngine.Random.Range(0, validReactions.Count);
                    // Debug.Log($"{unit.UnitName} is using reaction {option}");

                    if (validReactions[option].CostsReaction)
                    {
                        // Debug.Log($"Reaction costs a reaction");
                        if (unit.GetResource("reaction") <= 0)
                        {
                            // Debug.Log($"But {unit.UnitName} has no reaction");
                            continue;
                        }
                        else
                        {
                            // Debug.Log($"And {unit.UnitName} has a reaction");
                            unit.UseResource("reaction");
                        }
                    }

                    Debug.Log($"{unit.UnitName} is using reaction {option}");
                    validReactions[option].Execute(context, unit, () => { Debug.Log("Exectuted Reaction"); });
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

                            reaction.Execute(context, unit, () =>
                            {
                                finished = true;
                            });
                        }));
                    }

                    options.Add(new MenuOption("Skip", () => finished = true));

                    CombatMenuManager.Instance.OpenMenu(options);

                    yield return new WaitUntil(() => finished);

                    CombatMenuManager.Instance.CloseAllMenus();
                }
            }
            else
            {
                // Debug.Log($"{unit.UnitName} has no reactions");
            }
        }

        onComplete?.Invoke();
    }
    
    public IEnumerator CheckForReactionsCoroutine(ReactionTrigger trigger, BaseContext context)
    {
        bool done = false;

        CheckForReactions(trigger, context, () =>
        {
            done = true;
        });

        yield return new WaitUntil(() => done);
    }
}
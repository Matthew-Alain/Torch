using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuManager : MonoBehaviour
{
    public static CombatMenuManager Instance;
    [SerializeField] private GameObject selectedPCObject, tileObject, tileUnitObject;
    [SerializeField] public GameObject turnMenuPanel;
    [SerializeField] private ScrollRect scrollRect;
    public Transform buttonContainer;
    public GameObject buttonPrefab;
    public GameObject displayText;

    private Stack<Func<List<MenuOption>>> menuStack = new();

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
    }

    public void ShowSelectedPC(BasePC pc)
    {
        if (pc == null)
        {
            selectedPCObject.SetActive(false);
            return;
        }

        selectedPCObject.GetComponentInChildren<TMP_Text>().text = pc.GetName();
        selectedPCObject.SetActive(true);
    }

    public void ShowTileInfo(Tile tile)
    {

        if (tileObject == null || tileUnitObject == null)
        return;

        if (tile == null)
        {
            tileObject.SetActive(false);
            tileUnitObject.SetActive(false);
            return;
        }

        tileObject.GetComponentInChildren<TMP_Text>().text = tile.TileName;
        tileObject.SetActive(true);

        if (tile.OccupiedUnit) //If the tile has a unit on it
        {
            tileUnitObject.GetComponentInChildren<TMP_Text>().text = tile.OccupiedUnit.UnitName;
            tileUnitObject.SetActive(true);

        }
    }

    public void OpenMenu(Func<List<MenuOption>> menuBuilder)
    {
        menuStack.Push(menuBuilder);
        RenderMenu(menuBuilder());
    }

    public void CloseMenu()
    {
        if (menuStack.Count > 1)
        {
            menuStack.Pop();
            RenderMenu(menuStack.Peek()()); // rebuild previous menu
        }
        else
        {
            turnMenuPanel.SetActive(false);
            menuStack.Clear();
            
            StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));
            // CombatUnitManager.Instance.SetSelectedPC(null);
        }
    }

    public void CloseAllMenus()
    {
        turnMenuPanel.SetActive(false);
        menuStack.Clear();
        // CombatUnitManager.Instance.SetSelectedPC(null);
    }

    public void ReRenderMenu()
    {
        if (menuStack.Count > 0)
        {
            RenderMenu(menuStack.Peek()());
        }
    }

    public void RenderMenu(List<MenuOption> options)
    {
        turnMenuPanel.SetActive(true);

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        // foreach (var option in options)
        // {
        //     if (!option.IsAvailable())
        //         continue;

        //     GameObject btn = Instantiate(buttonPrefab, buttonContainer);
        //     btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Label;
        //     btn.GetComponent<Button>().onClick.AddListener(() => option.Action());
        // }

        foreach (var option in options)
        {
            if (!option.IsVisible())
                continue;

            GameObject btn = Instantiate(buttonPrefab, buttonContainer);

            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            var button = btn.GetComponent<Button>();
            var image = btn.GetComponent<Image>();

            text.text = option.Label;

            bool isEnabled = option.IsEnabled();

            if (isEnabled)
            {
                button.onClick.AddListener(() => option.Action());
                // button.interactable = true;
                // image.color = Color.white;
            }
            else
            {
                button.interactable = false;

                if (image != null)
                    image.color = Color.grey;

                // text.color = Color.gray;
            }
        }

        StartCoroutine(ResetScrollPosition());
    }

    IEnumerator ResetScrollPosition()
    {
        yield return null; // wait 1 frame for layout groups

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }

    public void OpenRootMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> rootOptions = new List<MenuOption>()
            {
                new MenuOption("Major", OpenMajorMenu, () => true, () => pc.GetResource("major_action") > 0 ||
                    (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)),
                new MenuOption("Minor", OpenMinorMenu, () => true, () => pc.GetResource("minor_action") > 0),
                new MenuOption($"Move {pc.GetResource("current_speed")}", () => StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.MovingPC)),
                    () => true, () => pc.GetResource("current_speed") > 0),
                new MenuOption("Free", OpenFreeActionMenu, () => true, () => true), //TODO: Add a condition to check for when there are free actions
                new MenuOption("End Turn", () => pc.EndTurn(), () => true, () => true),
                // new MenuOption("Cancel", () => CloseMenu())
            };

            return rootOptions;
        });
    }

    public void OpenMajorMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> majorOptions = new List<MenuOption>()
            {
                new MenuOption($"Attack ({pc.GetResource("current_number_of_attacks")})", OpenAttackMenu,
                () => true, () => pc.GetResource("major_action") > 0 ||
                (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)),

                new MenuOption("Magic", OpenSpellMenu, pc.IsSpellcaster, () => true)
            };

            pc.PopulateMajorActions(majorOptions);

            majorOptions.Add(new MenuOption("Dash", () => pc.Dash(), () => true, () => pc.GetResource("major_action") > 0));
            majorOptions.Add(new MenuOption("Disengage", () => pc.Disengage(), () => true, () => pc.GetResource("major_action") > 0));
            majorOptions.Add(new MenuOption("Dodge", () => pc.Dodge(), () => true, () => pc.GetResource("major_action") > 0));
            majorOptions.Add(new MenuOption("Help", () => pc.Help(), () => true, () => pc.GetResource("major_action") > 0));
            majorOptions.Add(new MenuOption("Hide", () => pc.Hide(), () => true, () => pc.GetResource("major_action") > 0));
            majorOptions.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return majorOptions;
        });
    }

    public void OpenAttackMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> attackOptions = new List<MenuOption>();

            pc.PopulateAttacks(attackOptions);

            attackOptions.Add(new MenuOption("Unarmed Strike", OpenUnarmedStrikeMenu, () => true, () => true));
            attackOptions.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return attackOptions;
        });
    }

    public void OpenUnarmedStrikeMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;

            List<MenuOption> unarmedStrikeOptions = new List<MenuOption>()
            {
                new MenuOption($"Damage", () => pc.Attack(0), () => true, () => true),
                new MenuOption("Grapple", () => pc.Attack(0), () => true, () => true), //TODO: Add grappling mechanics
                new MenuOption("Shove Backwards", () => pc.ShoveBack(), () => true, () => true),
                new MenuOption("Shove Prone", () => pc.ShoveProne(), () => true, () => true),
                new MenuOption("Back", () => CloseMenu(), () => true, () => true)
            };

            return unarmedStrikeOptions;
        });
    }

    public void OpenSpellMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;

            List<MenuOption> spellOptions = new List<MenuOption>()
            {
                new MenuOption($"Cantrips", () => pc.Attack(0), () => true, () => true),
                new MenuOption($"Level 1 Spells ({pc.GetResource("level_1_slots")})", () => pc.Attack(0), () => true, () => pc.GetResource("level_1_slots") > 0),
                new MenuOption($"Level 2 Spells ({pc.GetResource("level_2_slots")})", () => pc.Attack(0), () => true, () => pc.GetResource("level_2_slots") > 0),
                new MenuOption($"Level 3 Spells ({pc.GetResource("level_3_slots")})", () => pc.Attack(0), () => true, () => pc.GetResource("level_3_slots") > 0),
                new MenuOption("Back", () => CloseMenu(), () => true, () => true)
            };

            return spellOptions;
        });
    }

    public void OpenMinorMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> minorOptions = new List<MenuOption>()
            {
                //Put default options here
            };

            pc.PopulateMinorActions(minorOptions);

            // Back option
            minorOptions.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return minorOptions;
        });
    }

    public void OpenFreeActionMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> freeActions = new List<MenuOption>()
            {
                //Put default options here
            };

            pc.PopulateFreeActions(freeActions);

            // Back option
            freeActions.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return freeActions;
        });
    }

    public void DisplayText(string message)
    {
        displayText.SetActive(true);
        displayText.GetComponentInChildren<TMP_Text>().text = message;
    }
}

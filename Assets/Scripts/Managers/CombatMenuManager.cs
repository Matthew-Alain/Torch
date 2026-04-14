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
    public GameObject displayDice;
    public List<string> displayDiceText;

    public Button btnShowChat;
    [SerializeField] private ScrollRect chatPanelScrollRect;
    public GameObject chatPrefab;
    public Transform contentParent;

    [SerializeField] public GameObject UnitInfoCanvas;
    public Button btnCloseUnitInfo;
    [SerializeField] private ScrollRect unitInfoConditionScrollRect;
    public Transform conditionInfoContentParent;
    public TMP_Text STR_score, DEX_score, CON_score, INT_score, WIS_score, CHA_score;
    public TMP_Text STR_mod, DEX_mod, CON_mod, INT_mod, WIS_mod, CHA_mod;
    public TMP_Text STR_save, DEX_save, CON_save, INT_save, WIS_save, CHA_save;
    public TMP_Text unitName, currentHP, maxHP, tempHP;

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

    void Start()
    {
        btnShowChat.onClick.AddListener(ShowHideChat);
        btnCloseUnitInfo.onClick.AddListener(CloseUnitInfo);
    }

    public void ShowHideChat()
    {
        chatPanelScrollRect.gameObject.SetActive(!chatPanelScrollRect.gameObject.activeSelf);

        if (chatPanelScrollRect.enabled)
            StartCoroutine(ResetScrollPosition(chatPanelScrollRect));
    }

    IEnumerator ResetScrollPosition(ScrollRect obj)
    {
        yield return null; // wait 1 frame for layout groups

        Canvas.ForceUpdateCanvases();
        obj.verticalNormalizedPosition = 0f;
    }
    
    public void AddItemToChat(string text)
    {
        GameObject obj = Instantiate(chatPrefab, contentParent);

        var ui = obj.GetComponent<TMP_Text>();

        ui.text = text;
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
        CombatStateManager.Instance.CancelSelection();
        StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));
        
        if (menuStack.Count > 1)
        {
            menuStack.Pop();
            RenderMenu(menuStack.Peek()()); // rebuild previous menu
        }
        else
        {
            turnMenuPanel.SetActive(false);
            menuStack.Clear();
            
            // CombatUnitManager.Instance.SetSelectedPC(null);
        }
    }

    public void CloseAllMenus()
    {
        for(int i = 0; i < menuStack.Count; i++)
        {
            CloseMenu();
        }
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
        // StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.PlayerTurn));

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

        bool hasValidOption = false;

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
                if (option.Label != "Back")
                {
                    hasValidOption = true;
                }
            }
            else
            {
                button.interactable = false;

                if (image != null)
                    image.color = Color.grey;

                // text.color = Color.gray;
            }
        }

        if (!hasValidOption)
        {
            CloseMenu();
        }
        else
        {
            StartCoroutine(ResetScrollPosition());
        }
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
                new MenuOption($"Move ({pc.GetResource("current_speed")}ft left)", () => StartCoroutine(CombatStateManager.Instance.ChangeState(GameState.MovingPC)),
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

                new MenuOption("Magic", () => OpenSpellMenu("major_action"), pc.IsSpellcaster, () => pc.GetResource("major_action") > 0)
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

            attackOptions.Add(new MenuOption("Unarmed Strike", OpenUnarmedStrikeMenu, () => true, () => pc.GetResource("major_action") > 0 ||
                (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)));
            
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
                new MenuOption($"Damage", () => StartCoroutine(pc.Attack(0)), () => true, () => pc.GetResource("major_action") > 0 ||
                    (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)),

                new MenuOption("Grapple", () => StartCoroutine(pc.Attack(0)), () => true, () => pc.GetResource("major_action") > 0 ||
                    (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)), //TODO: Add grappling mechanics
                
                new MenuOption("Shove Backwards", () => StartCoroutine(pc.ShoveBack()), () => true, () => pc.GetResource("major_action") > 0 ||
                    (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)),

                new MenuOption("Shove Prone", () => pc.ShoveProne(), () => true, () => pc.GetResource("major_action") > 0 ||
                    (pc.GetResource("current_number_of_attacks") < pc.GetResource("max_number_of_attacks") && pc.GetResource("current_number_of_attacks") > 0)),

                new MenuOption("Back", () => CloseMenu(), () => true, () => true)
            };

            return unarmedStrikeOptions;
        });
    }

    public void OpenSpellMenu(string actionCost)
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;

            List<MenuOption> spellOptions = new List<MenuOption>()
            {
                new MenuOption($"Cantrips", () => OpenSpellList(actionCost, 0), () => true, () => pc.GetResource(actionCost) > 0),

                new MenuOption($"Level 1 Spells ({pc.GetResource("level_1_slots")})", () => OpenSpellList(actionCost, 1),
                    () => true,
                    () => pc.GetResource("level_1_slots") > 0 && pc.GetResource(actionCost) > 0),

                new MenuOption($"Level 2 Spells ({pc.GetResource("level_2_slots")})", () => OpenSpellList(actionCost, 2),
                    () => true,
                    () => pc.GetResource("level_2_slots") > 0 && pc.GetResource(actionCost) > 0),

                new MenuOption($"Level 3 Spells ({pc.GetResource("level_3_slots")})", () => OpenSpellList(actionCost, 3),
                    () => true,
                    () => pc.GetResource("level_3_slots") > 0 && pc.GetResource(actionCost) > 0),

                new MenuOption("Back", () => CloseMenu(), () => true, () => true)
            };

            return spellOptions;
        });
    }

    public void OpenSpellList(string actionCost, int spellLevel)
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;

            List<MenuOption> options = new List<MenuOption>();

            pc.PopulateSpells(options, actionCost, spellLevel);

            options.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return options;
        });
    }

    public void OpenMinorMenu()
    {
        OpenMenu(() =>
        {
            BasePC pc = CombatUnitManager.Instance.SelectedPC;
            List<MenuOption> minorOptions = new List<MenuOption>()
            {
                new MenuOption("Magic", () => OpenSpellMenu("minor_action"), pc.IsSpellcaster, () => pc.GetResource("minor_action") > 0)
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
                new MenuOption($"Fall Prone", () => pc.FallProne(), () => true, () => !pc.GetCondition("prone")),
                new MenuOption($"Stand Up (Costs {pc.GetResource("base_speed") / 2}ft of speed)", () => pc.StandUp(),
                () => true, () => pc.GetCondition("prone") && pc.GetResource("current_speed") >= pc.GetResource("base_speed") / 2)
            };

            pc.PopulateFreeActions(freeActions);

            // Back option
            freeActions.Add(new MenuOption("Back", () => CloseMenu(), () => true, () => true));

            return freeActions;
        });
    }

    public IEnumerator DisplayText(string message)
    {
        displayText.SetActive(true);
        displayText.GetComponentInChildren<TMP_Text>().text = message;
        AddItemToChat(message);
        StartCoroutine(ResetScrollPosition(chatPanelScrollRect));
        yield return new WaitForSeconds(0.5f);
        displayText.SetActive(false);
    }

    public IEnumerator DisplayDiceRoll(BaseUnit roller, int dieResult, int modifier, int PB, bool result)
    {
        displayDice.SetActive(true);
        int total = dieResult + modifier;

        string message = $"Dice roll: {dieResult}. ";

        displayDice.GetComponentInChildren<TMP_Text>().text = message;
        yield return new WaitForSeconds(1f);

        message += "Modifier: ";
        if (modifier < 0)
            message += "-";
        else
            message += "+";
        message += modifier.ToString() + ". ";

        displayDice.GetComponentInChildren<TMP_Text>().text = message;
        yield return new WaitForSeconds(1f);

        if (PB > 0)
        {
            message += $"Proficiency: {roller.GetPB()}. ";
            total += roller.GetPB();
            displayDice.GetComponentInChildren<TMP_Text>().text = message;
            yield return new WaitForSeconds(1f);
        }

        message += $"Total: {total}. ";

        displayDice.GetComponentInChildren<TMP_Text>().text = message;
        yield return new WaitForSeconds(1f);

        if (result)
            message += $"Result: Success";
        else
            message += $"Result: Fail";

        displayDice.GetComponentInChildren<TMP_Text>().text = message;
        yield return new WaitForSeconds(2f);

        displayDice.SetActive(false);
    }

    public void ShowUnitInfo(BaseUnit unit)
    {
        UnitInfoCanvas.SetActive(true);
        unitName.text = unit.UnitName;
        currentHP.text = unit.GetCurrentHP().ToString();
        maxHP.text = unit.GetMaxHP().ToString();
        tempHP.text = unit.GetTempHP().ToString();
        STR_score.text = unit.GetStat("strength").ToString();
        DEX_score.text = unit.GetStat("dexterity").ToString();
        CON_score.text = unit.GetStat("constitution").ToString();
        INT_score.text = unit.GetStat("intelligence").ToString();
        WIS_score.text = unit.GetStat("wisdom").ToString();
        CHA_score.text = unit.GetStat("charisma").ToString();
        STR_mod.text = "(" + unit.GetStat("mSTR").ToString() + ")";
        DEX_mod.text = "(" + unit.GetStat("mDEX").ToString() + ")";
        CON_mod.text = "(" + unit.GetStat("mCON").ToString() + ")";
        INT_mod.text = "(" + unit.GetStat("mINT").ToString() + ")";
        WIS_mod.text = "(" + unit.GetStat("mWIS").ToString() + ")";
        CHA_mod.text = "(" + unit.GetStat("mCHA").ToString() + ")";
        STR_save.text = "(" + unit.GetStat("str_save").ToString() + ")";
        DEX_save.text = "(" + unit.GetStat("dex_save").ToString() + ")";
        CON_save.text = "(" + unit.GetStat("con_save").ToString() + ")";
        INT_save.text = "(" + unit.GetStat("int_save").ToString() + ")";
        WIS_save.text = "(" + unit.GetStat("wis_save").ToString() + ")";
        CHA_save.text = "(" + unit.GetStat("cha_save").ToString() + ")";

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT condition_name FROM active_conditions WHERE target_unit_id = {unit.UnitID}",
            reader =>
            {
                while (reader.Read())
                {
                    AddConditionToInfo(Convert.ToString(reader["condition_name"]));
                }
            }
        );
    }
    
    public void AddConditionToInfo(string text)
    {
        GameObject obj = Instantiate(chatPrefab, conditionInfoContentParent);

        var ui = obj.GetComponent<TMP_Text>();

        ui.text = text;
    }
    
    public void CloseUnitInfo()
    {
        foreach (Transform child in conditionInfoContentParent)
        {
            Destroy(child.gameObject);
        }
        UnitInfoCanvas.SetActive(false);
    }
}

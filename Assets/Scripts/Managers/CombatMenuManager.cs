using System;
using System.Collections;
using System.Collections.Generic;
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

    private Stack<List<MenuOption>> menuStack = new Stack<List<MenuOption>>();

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

    public void OpenMenu(List<MenuOption> options)
    {
        menuStack.Push(options);
        RenderMenu(options);
    }

    public void CloseMenu()
    {
        if (menuStack.Count > 1)
        {
            menuStack.Pop();
            RenderMenu(menuStack.Peek());
        }
        else
        {
            turnMenuPanel.SetActive(false);
            menuStack.Clear();
            CombatUnitManager.Instance.SetSelectedPC(null);
            CombatStateManager.Instance.ChangeState(GameState.PlayerTurn);
        }
    }

    public void CloseAllMenus()
    {
        turnMenuPanel.SetActive(false);
        menuStack.Clear();
        CombatUnitManager.Instance.SetSelectedPC(null);
    }

    void RenderMenu(List<MenuOption> options)
    {
        turnMenuPanel.SetActive(true);

        foreach (Transform child in buttonContainer)
            Destroy(child.gameObject);

        foreach (var option in options)
        {
            if (!option.IsAvailable())
                continue;

            GameObject btn = Instantiate(buttonPrefab, buttonContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Label;
            btn.GetComponent<Button>().onClick.AddListener(() => option.Action());
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
        List<MenuOption> rootOptions = new List<MenuOption>()
        {
            new MenuOption("Major", OpenMajorMenu),
            new MenuOption("Minor", OpenMinorMenu),
            new MenuOption("Move", () => CombatStateManager.Instance.ChangeState(GameState.MovingPC)),
            new MenuOption("End Turn", () => CombatStateManager.Instance.EndPlayerTurn()),
            new MenuOption("Cancel", () => CloseMenu())
        };

        OpenMenu(rootOptions);
    }

    public void OpenMajorMenu()
    {
        List<MenuOption> majorOptions = new List<MenuOption>()
        {
            new MenuOption("Attack", OpenAttackMenu),
            new MenuOption("Dash", () => CombatActions.Dash(CombatUnitManager.Instance.SelectedPC)),
            new MenuOption("Disengage", () => Debug.Log("Disengage")),
            new MenuOption("Dodge", () => Debug.Log("Dodge")),
            new MenuOption("Help", () => Debug.Log("Help")),
            new MenuOption("Hide", () => Debug.Log("Hide"))
        };

        // Conditional option
        if (CombatUnitManager.Instance.SelectedPC.GetClassName() == "Wizard")
        {
            majorOptions.Add(new MenuOption("Magic", () => Debug.Log("Magic")));
        }

        // Back option
        majorOptions.Add(new MenuOption("Back", () => CloseMenu()));

        OpenMenu(majorOptions);
    }

    public void OpenAttackMenu()
    {
        int mainHand = 0;
        int offHand = 0;
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT main_hand_item, off_hand_item FROM saved_pcs WHERE id = {CombatUnitManager.Instance.SelectedPC.UnitID}",
            reader =>
            {
                while (reader.Read())
                {
                    mainHand = Convert.ToInt32(reader["main_hand_item"]);
                    offHand = Convert.ToInt32(reader["off_hand_item"]);
                }
            }
        );

        List<MenuOption> attackOptions = new List<MenuOption>();

        string mainHandText = CombatUnitManager.Instance.SelectedPC.GetMainhandName();
        if (mainHandText != "Shield")
        {
            attackOptions.Add(new MenuOption($"Mainhand ({mainHandText})", () => CombatStateManager.Instance.DeclareAttack(mainHand)));
        }
        
        string offHandText = CombatUnitManager.Instance.SelectedPC.GetOffhandName();
        if(offHandText != "Shield")
        {
            attackOptions.Add(new MenuOption($"Offhand ({offHandText})", () => CombatStateManager.Instance.DeclareAttack(offHand)));
        }

        // Back option
        attackOptions.Add(new MenuOption("Back", () => CloseMenu()));

        OpenMenu(attackOptions);
    }

    public void OpenMinorMenu()
    {
        List<MenuOption> minorOptions = new List<MenuOption>()
        {
            //Put default options here
        };

        // Conditional option
        if (CombatUnitManager.Instance.SelectedPC.GetClassName() == "Barbarian")
        {
            minorOptions.Add(new MenuOption("Rage", () => CombatUnitManager.Instance.SelectedPC.RestoreHealth(1)));
        }

        // Back option
        minorOptions.Add(new MenuOption("Back", () => CloseMenu()));

        OpenMenu(minorOptions);
    }

    public void DisplayText(string message)
    {
        displayText.SetActive(true);
        displayText.GetComponentInChildren<TMP_Text>().text = message;
    }
}

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatMenuManager : MonoBehaviour
{
    public static CombatMenuManager Instance;
    [SerializeField] private GameObject selectedPCObject, tileObject, tileUnitObject;
    [SerializeField] public GameObject pcTurnMenu;
    
    public GameObject menuPanel;
    public Transform buttonContainer;
    public GameObject buttonPrefab;
    

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

        selectedPCObject.GetComponentInChildren<TMP_Text>().text = pc.UnitName;
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
            menuPanel.SetActive(false);
            menuStack.Clear();
            CombatUnitManager.Instance.SetSelectedPC(null);
        }
    }

    public void CloseAllMenus()
    {
        menuPanel.SetActive(false);
        menuStack.Clear();
        CombatUnitManager.Instance.SetSelectedPC(null);
    }

    void RenderMenu(List<MenuOption> options)
    {
        menuPanel.SetActive(true);

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
    }

    public void OpenRootMenu()
    {
        List<MenuOption> rootOptions = new List<MenuOption>()
        {
            new MenuOption("Major", OpenMajorMenu),
            new MenuOption("Minor", OpenMinorMenu),
            new MenuOption("Move", () => Debug.Log("Move action")),
            new MenuOption("End Turn", () => CombatStateManager.Instance.EndPlayerTurn()),
            new MenuOption("Cancel", () => CloseMenu())
        };

        OpenMenu(rootOptions);
    }

    public void OpenMajorMenu()
    {
        List<MenuOption> majorOptions = new List<MenuOption>()
        {
            new MenuOption("Attack", () => Debug.Log("Attack")),
            new MenuOption("Dash", () => Debug.Log("Dash")),
            new MenuOption("Disengage", () => Debug.Log("Disengage")),
            new MenuOption("Dodge", () => Debug.Log("Dodge")),
            new MenuOption("Help", () => Debug.Log("Help")),
            new MenuOption("Hide", () => Debug.Log("Hide"))
        };

        // Conditional option
        if (CombatUnitManager.Instance.SelectedPC.GetClass() == "Wizard")
        {
            majorOptions.Add(new MenuOption("Magic", () => Debug.Log("Magic")));
        }

        // Back option
        majorOptions.Add(new MenuOption("Back", () => CloseMenu()));

        OpenMenu(majorOptions);
    }

        public void OpenMinorMenu()
    {
        List<MenuOption> minorOptions = new List<MenuOption>()
        {
            //Put default options here
        };

        // Conditional option
        if (CombatUnitManager.Instance.SelectedPC.GetClass() == "Barbarian")
        {
            minorOptions.Add(new MenuOption("Rage", () => CombatUnitManager.Instance.HealUnit(CombatUnitManager.Instance.SelectedPC.UnitID, 1)));
        }

        // Back option
        minorOptions.Add(new MenuOption("Back", () => CloseMenu()));

        OpenMenu(minorOptions);
    }
}

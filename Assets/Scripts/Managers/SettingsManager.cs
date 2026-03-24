using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SettingsManager : MonoBehaviour
{

    public static SettingsManager Instance;
    public GameObject settingsCanvas;
    public GameObject panelMain;
    public GameObject panelVolume;
    public GameObject panelTutorials;
    public Button markAllTutorialsRead;
    public GameObject panelUnsavedWarning;

    private bool hasUnsavedChanges = false;

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
        DontDestroyOnLoad(gameObject);

        if (settingsCanvas != null)
        {
            DontDestroyOnLoad(settingsCanvas);
        }

        settingsCanvas.SetActive(false);
        markAllTutorialsRead.onClick.AddListener(TutorialManager.MarkAllTutorialsRead);
    }

    void Update()
    {
        // If the user is on a non-setting scene and presses escape...
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleSettings();
        }

    }

    public void ToggleSettings()
    {
        if (settingsCanvas != null)
        {
            if (!settingsCanvas.activeSelf)
            {
                settingsCanvas.SetActive(!settingsCanvas.activeSelf);
                ShowPanel(panelMain);
            }
            else if (panelMain.activeSelf)
            {
                settingsCanvas.SetActive(!settingsCanvas.activeSelf);
            }
            else
            {
                ShowPanel(panelMain);
            }
        }
    }

    public void ShowPanel(GameObject panel)
    {
        panelMain.SetActive(false);
        panelVolume.SetActive(false);
        panelTutorials.SetActive(false);
        panelUnsavedWarning.SetActive(false);

        panel.SetActive(true);
    }

    public void MarkAsUnsavedChanges()
    {
        // Debug.Log("Scene has unsaved changes");
        hasUnsavedChanges = true;
    }
    
    public void OpenVolume() => ShowPanel(panelVolume);
    public void OpenTutorial() => ShowPanel(panelTutorials);
    public void BackToMain() => ShowPanel(panelMain);

    public void ReturnToMainMenuScene()
    {
        if (hasUnsavedChanges && !panelUnsavedWarning.activeSelf)
        {
            ShowPanel(panelUnsavedWarning);
        }
        else
        {
            SceneManager.LoadScene(0);
            settingsCanvas.SetActive(false);
        }
    }

    public void SaveChanges()
    {
        hasUnsavedChanges = false;
        // Debug.Log("No longer have unsaved changes");
    }

}

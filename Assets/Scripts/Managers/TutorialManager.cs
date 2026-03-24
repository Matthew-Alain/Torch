using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    public TMP_Text title, text;
    public Button btnShowTutorials, btnNext, btnBack, btnMarkAllAsRead;
    public GameObject tutorialPanel, tutorialCanvas;

    private int currentTutorialIndex = 0;
    private List<int> tutorialIDList = new List<int>();

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

        if (tutorialCanvas != null)
        {
            tutorialCanvas.transform.SetParent(transform);
        }

        btnShowTutorials.onClick.AddListener(OpenCloseTutorialWindow);
        btnNext.onClick.AddListener(NextTutorial);
        btnBack.onClick.AddListener(BackTutorial);
        btnMarkAllAsRead.onClick.AddListener(MarkAllTutorialsReadThisScene);
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GetSceneTutorials();
    }

    private void GetSceneTutorials()
    {
        // Debug.Log($"Populating list with scene {SceneManager.GetActiveScene().buildIndex}");

        tutorialIDList.Clear();
        currentTutorialIndex = 0;

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT id FROM tutorials WHERE scene = {SceneManager.GetActiveScene().buildIndex}",
            reader =>
            {
                while (reader.Read())
                {
                    tutorialIDList.Add(Convert.ToInt32(reader["id"]));
                    // Debug.Log($"Added tutorial {Convert.ToInt32(reader["id"])} to the list");

                    // Debug.Log("Added tutorial id "+Convert.ToInt32(reader["id"]));
                }
            }
        );

        CheckForUnreadTutorial();
    }

    private void OpenCloseTutorialWindow()
    {
        // Debug.Log("Count: "+tutorialIDList.Count);
        if (tutorialIDList.Count == 0)
            return;

        // Debug.Log($"Panel active? {tutorialPanel.activeSelf}");
        tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        // Debug.Log($"Now it flipped");

        if (tutorialPanel.activeSelf)
        {
            // Debug.Log($"Index is now 0");
            currentTutorialIndex = 0;
            // Debug.Log($"Get tutorial at index 0");
            GetTutorialByID(tutorialIDList[currentTutorialIndex]);
        }

    }

    private void NextTutorial()
    {
        if (tutorialIDList.Count == 0)
            return;

        MarkTutorialAsRead(tutorialIDList[currentTutorialIndex]);

        if (currentTutorialIndex < tutorialIDList.Count - 1)
        {
            // Debug.Log("NextTutorial: Current index: " + currentTutorialIndex);
            currentTutorialIndex++;
            GetTutorialByID(tutorialIDList[currentTutorialIndex]);
        }
        else
        {
            tutorialPanel.SetActive(false);
            currentTutorialIndex = 0;
        }
    }

    private void BackTutorial()
    {
        if (currentTutorialIndex > 0)
        {
            currentTutorialIndex--;
            GetTutorialByID(tutorialIDList[currentTutorialIndex]);
        }
    }
    
    private void UpdateButtons()
    {
        // Debug.Log("Index when updating buttons: " + currentTutorialIndex);
        if(currentTutorialIndex == 0)
        {
            btnBack.gameObject.SetActive(false);
        }
        else
        {
            btnBack.gameObject.SetActive(true);
        }
    }

    public void GetTutorialByID(int id)
    {
        // Debug.Log($"Index passed: {id}");
        // if (id < 0 || id >= tutorialIDList.Count) return; <<==== THIS LINE WAS THE ISSUE

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT name, description FROM tutorials WHERE id = {id}",
            reader =>
            {
                while (reader.Read())
                {
                    title.text = Convert.ToString(reader["name"]);
                    text.text = Convert.ToString(reader["description"]);
                }
            }
        );

        // Debug.Log($"Now updating buttons");

        UpdateButtons();
    }

    public bool TutorialIsRead(int id)
    {
        return Convert.ToBoolean(DatabaseManager.Instance.ExecuteScalar($"SELECT read FROM tutorials WHERE id = {id}"));
    }

    public void MarkTutorialAsRead(int id)
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tutorials SET read = 1 WHERE id = {id}");
    }

    public void MarkAllTutorialsReadThisScene()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tutorials SET read = 1 WHERE scene = {SceneManager.GetActiveScene().buildIndex}");
        OpenCloseTutorialWindow();
    }

    public static void MarkAllTutorialsRead()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE tutorials SET read = 1");
    }

    public void CheckForUnreadTutorial()
    {
        if (tutorialIDList.Count <= 0)
            return;

        for (int i = 0; i < tutorialIDList.Count; i++)
        {
            if (!TutorialIsRead(tutorialIDList[i]))
            {
                currentTutorialIndex = i;
                tutorialPanel.SetActive(true);
                GetTutorialByID(tutorialIDList[i]);
                return;
            }
        }
        currentTutorialIndex = 0;
        GetTutorialByID(tutorialIDList[currentTutorialIndex]);
    }

}

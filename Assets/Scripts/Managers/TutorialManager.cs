using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public TMP_Text title, text;
    public Button btnShowTutorials, btnNext;
    public GameObject tutorialPanel;

    private int currentTutorial = 0;
    private List<int> tutorialIDList = new List<int>();

    void Awake()
    {
        GetSceneTutorials();
        btnShowTutorials.onClick.AddListener(OpenCloseTutorialWindow);
        btnNext.onClick.AddListener(NextTutorial);
    }

    private void GetSceneTutorials()
    {
        DatabaseManager.Instance.ExecuteReader(
            "SELECT id FROM tutorials WHERE scene = @current_scene",
            reader =>
            {
                while (reader.Read())
                {
                    tutorialIDList.Add(Convert.ToInt32(reader["id"]));
                    // Debug.Log("Added tutorial id "+Convert.ToInt32(reader["id"]));
                }
            },
            ("@current_scene", SceneManager.GetActiveScene().buildIndex)
        );

    }

    private void OpenCloseTutorialWindow()
    {
        if (!tutorialPanel.activeSelf)
        {
            tutorialPanel.SetActive(true);
            NextTutorial();
        }
        else
        {
            tutorialPanel.SetActive(false);
            currentTutorial = tutorialIDList[0];
        }
    }
    
    private void NextTutorial()
    {
        if(currentTutorial >= tutorialIDList.Count)
        {
            OpenCloseTutorialWindow();
        }
        else
        {
            GetTutorialByID(tutorialIDList[currentTutorial]);
            currentTutorial += 1;
        }
    }
    
    public void GetTutorialByID(int id)
    {
        string returnedTitle = Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT name FROM tutorials WHERE id = (@tutorialID)",
            ("@tutorialID", id)
        ));

        string returnedText = Convert.ToString(DatabaseManager.Instance.ExecuteScalar( //Get the character's species
            "SELECT description FROM tutorials WHERE id = (@tutorialID)",
            ("@tutorialID", id)
        ));

        title.text = returnedTitle;
        text.text = returnedText;
    }
}

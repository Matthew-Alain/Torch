using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class EncounterSelectManager : MonoBehaviour
{
    public TMP_Dropdown encounterList;
    public Button btnStartEncounter;
    public TMP_Text txtDifficulty, txtDescription;
    public Image tutorial_1, tutorial_2, tutorial_3, tutorial_4, encounter_1, encounter_2, encounter_3, encounter_4, encounter_5;


    void Awake()
    {
        encounterList.onValueChanged.AddListener(ChangeEncounter);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DatabaseManager.Instance.currentEncounter = 0;
        btnStartEncounter.onClick.AddListener(StartEncounter);
    }

    void ChangeEncounter(int index)
    {
        DatabaseManager.Instance.currentEncounter = index;
        DatabaseManager.Instance.ExecuteReader(
            $"SELECT difficulty, description FROM encounters WHERE id = {index}",
            reader =>
            {
                txtDifficulty.text = Convert.ToString(reader["difficulty"]);
                txtDescription.text = Convert.ToString(reader["description"]);
            }
        );

        tutorial_1.gameObject.SetActive(false);
        tutorial_2.gameObject.SetActive(false);
        tutorial_3.gameObject.SetActive(false);
        tutorial_4.gameObject.SetActive(false);
        encounter_1.gameObject.SetActive(false);
        encounter_2.gameObject.SetActive(false);
        encounter_3.gameObject.SetActive(false);
        encounter_4.gameObject.SetActive(false);
        encounter_5.gameObject.SetActive(false);

        switch (index)
        {
            case 0:
                tutorial_1.gameObject.SetActive(true);
                break;
            case 1:
                tutorial_2.gameObject.SetActive(true);
                break;
            case 2:
                tutorial_3.gameObject.SetActive(true);
                break;
            case 3:
                tutorial_4.gameObject.SetActive(true);
                break;
            case 4:
                encounter_1.gameObject.SetActive(true);
                break;
            case 5:
                encounter_2.gameObject.SetActive(true);
                break;
            case 6:
                encounter_3.gameObject.SetActive(true);
                break;
            case 7:
                encounter_4.gameObject.SetActive(true);
                break;
            case 8:
                encounter_5.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    void StartEncounter()
    {
        DatabaseManager.Instance.CreateEncounterDatabase(DatabaseManager.Instance.currentEncounter);

        SceneManager.LoadScene(DatabaseManager.Instance.currentEncounter + 24);
    }


}

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

    }

    void StartEncounter()
    {
        DatabaseManager.Instance.CreateEncounterDatabase(DatabaseManager.Instance.currentEncounter);

        SceneManager.LoadScene(DatabaseManager.Instance.currentEncounter + 24);
    }


}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;

public class EncounterSelectManager : MonoBehaviour
{
    public TMP_Dropdown encounterList;
    public Button btnStartEncounter;

    void Awake()
    {
        encounterList.onValueChanged.AddListener(ChangeEncounter);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DatabaseManager.Instance.encounterToLoad = 0;
        btnStartEncounter.onClick.AddListener(() => SceneManager.LoadScene(DatabaseManager.Instance.encounterToLoad + 25));
    }

    void ChangeEncounter(int index)
    {
        DatabaseManager.Instance.encounterToLoad = index;
    }


}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class OriginManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown species, originFeat;
    public Button btnBack;
    public GameObject featureButtonRow, featurePanel;
    public Button closePanel, originFeature;
    public TMP_Text featureName, featureText;

    private List<Button> originButtons = new List<Button>();

    //Current PC information
    public BasePC currentPC;

    void Awake()
    {
        currentPC = DatabaseManager.Instance.lastPCEdited;
        originButtons.AddRange(featureButtonRow.GetComponentsInChildren<Button>());
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetDefaultInfo();
        UpdateSpeciesFeatures(currentPC.GetSpecies());

        species.onValueChanged.AddListener(UpdateSpeciesFeatures);
        originFeat.onValueChanged.AddListener(UpdateOriginFeat);

        originFeature.onClick.AddListener(() => OpenOriginFeatureWindow(currentPC.GetOriginFeat()));
        closePanel.onClick.AddListener(CloseFeatureWindow);
        btnBack.onClick.AddListener(SaveCharacter);
    }

    void GetDefaultInfo()
    {
        species.SetValueWithoutNotify(currentPC.GetSpecies());
        originFeat.SetValueWithoutNotify(currentPC.GetOriginFeat());
    }

    void UpdateSpeciesFeatures(int index)
    {
        int numberOfFeatures = Convert.ToInt32(DatabaseManager.Instance.ExecuteScalar($"SELECT COUNT(*) FROM species_features WHERE species = {index}"));

        List<string> featureNames = new List<string>();

        DatabaseManager.Instance.ExecuteReader($"SELECT name FROM species_features WHERE species = {index}",
            reader =>
            {
                while (reader.Read())
                {
                    featureNames.Add(Convert.ToString(reader["name"]));
                }
            }
        );

        for (int column = 0; column < 5; column++)
        {
            int currentColumn = column;
            Button btn = originButtons[currentColumn];
            btn.onClick.RemoveAllListeners();
            if (currentColumn <= numberOfFeatures - 1)
            {
                btn.gameObject.SetActive(true);
                btn.GetComponentInChildren<TMP_Text>().text = featureNames[currentColumn];
                btn.onClick.AddListener(() => OpenFeatureWindow(currentColumn));
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    void UpdateOriginFeat(int index)
    {
        originFeature.onClick.RemoveAllListeners();
        originFeature.GetComponentInChildren<TMP_Text>().text = Convert.ToString(DatabaseManager.Instance.ExecuteScalar($"SELECT name FROM feats WHERE id = {index}"));
        originFeature.onClick.AddListener(() => OpenOriginFeatureWindow(index));
    }

    public void OpenFeatureWindow(int columnNumber)
    {
        featurePanel.SetActive(true);

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT name, description FROM species_features WHERE species = {species.value} LIMIT 1 OFFSET {columnNumber}",
            reader =>
            {
                while (reader.Read())
                {
                    featureName.text = Convert.ToString(reader["name"]);
                    featureText.text = Convert.ToString(reader["description"]);
                }
            }
        );
    }

    public void OpenOriginFeatureWindow(int featID)
    {
        featurePanel.SetActive(true);

        DatabaseManager.Instance.ExecuteReader(
            $"SELECT name, description FROM feats WHERE id = {featID}",
            reader =>
            {
                while (reader.Read())
                {
                    featureName.text = Convert.ToString(reader["name"]);

                    featureText.text = Convert.ToString(reader["description"]);
                }
            }
        );
    }
    
    private void CloseFeatureWindow()
    {
        featurePanel.SetActive(false);
    }
    
    void SaveCharacter()
    {
        DatabaseManager.Instance.ExecuteNonQuery($"UPDATE saved_pcs SET species = {species.value}, origin_feat = {originFeat.value} WHERE id = {currentPC.UnitID}");
    }
}

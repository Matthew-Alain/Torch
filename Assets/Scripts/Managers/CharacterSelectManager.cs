using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class CharacterSelectManager : MonoBehaviour
{
    public TMP_Text characterLabel1, characterLabel2, characterLabel3, characterLabel4, characterLabel5;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        List<string> nameList = new List<string>();//Create the list of strings to hold the names

        DatabaseManager.Instance.ExecuteReader(
        "SELECT name FROM saved_pcs",             //Get the names from the database
        reader =>
        {
            while (reader.Read())
            {
                nameList.Add(reader["name"] as string); //Go through each name, and add it to the list of strings
            }
        });

        characterLabel1.text = nameList[0];
        characterLabel2.text = nameList[1];
        characterLabel3.text = nameList[2];
        characterLabel4.text = nameList[3];
        characterLabel5.text = nameList[4];
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void SetPCToEdit(int PCID)
    {
        DatabaseManager.Instance.lastPCEdited = PCID;
    }
}

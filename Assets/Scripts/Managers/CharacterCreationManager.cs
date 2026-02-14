using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CharacterCreationManager : MonoBehaviour
{

    //Create the dropdown objects to be assigned in the Unity editor
    public TMP_Dropdown species, dndclasses;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if(species == null)
        {
            Debug.Log("No species dropdown assigned");
            return;
        }
        species.ClearOptions(); //Clear the old options
        getSpecies(); //Add the new options

        if(dndclasses == null)
        {
            Debug.Log("No dndclass dropdown assigned");
            return;
        }
        dndclasses.ClearOptions(); //Clear the old options
        getDndClasses(); //Add the new options
    }

    // Update is called once per frame
    void Update()
    {

    }

    void getSpecies()
    {
        List<string> speciesList = new List<string>();//Create the list of strings to hold the names

        DatabaseManager.Instance.ExecuteReader(
        "SELECT name FROM species",             //Get the names from the database
        reader =>
        {
            while (reader.Read())
            {
                speciesList.Add(reader["name"] as string); //Go through each name, and add it to the list of strings
            }
        });

        species.AddOptions(speciesList); //Add the list of strings to the dropdown object
    }

    void getDndClasses()
    {
        List<string> dndClassList = new List<string>();

        DatabaseManager.Instance.ExecuteReader(
        "SELECT name FROM dndclasses",
        reader =>
        {
            while (reader.Read())
            {
                dndClassList.Add(reader["name"] as string);
            }
        });

        dndclasses.AddOptions(dndClassList);
    }
}

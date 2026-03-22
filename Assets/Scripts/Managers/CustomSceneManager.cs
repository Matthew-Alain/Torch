using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager Instance;

    private void Awake()
    {
        // //Check if an instance already exists that isn't this
        // if (Instance != null && Instance != this)
        // {
        //     //If it does, destroy it
        //     Destroy(gameObject);
        //     return;
        // }

        // //This just allows manager scripts to be stored in a folder in the editor for organization, but during runtime, get deteached to avoid errors
        // if (transform.parent != null)
        // {
        //     transform.parent = null; // Detach from parent
        // }

        // //Now safe to create a new instance
        // Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // // If the user is on a non-setting scene and presses escape...
        // if (SceneManager.GetActiveScene().buildIndex != 1 && Keyboard.current.escapeKey.wasPressedThisFrame)
        // {
        //     if(SceneManager.GetActiveScene().buildIndex != 2) //If you're on the tutorials scene, just go back to settings, don't update last scene
        //     {
        //         DatabaseManager.Instance.lastScene = SceneManager.GetActiveScene().buildIndex;
        //     }
        //     // Then load the setting scene
        //     LoadScene(Scene.Settings);
        // }
        // else if (SceneManager.GetActiveScene().buildIndex == 1 && Keyboard.current.escapeKey.wasPressedThisFrame)
        // {
        //     LoadLastScene();
        // }
    }
    
    // public void LoadLastScene()
    // {
    //     if(DatabaseManager.Instance.lastScene == 1)
    //     {
    //         LoadScene(Scene.Main_Menu);
    //     }
    //     else
    //     {        
    //         SceneManager.LoadScene(DatabaseManager.Instance.lastScene);
    //     }
    // }

    // private void LoadScene(Scene sceneName)
    // {
        
    //     SceneManager.LoadScene((int)sceneName);
    // }
    
    // public enum Scene
    // {
    //     Main_Menu,          //0
    //     Settings,           //1
    //     Tutorials,          //2
    //     Character_Creation, //3
    //     Origin_Select,      //4
    //     Class_Select,       //5
    //     Equipment_Select,   //6
    //     Character_Select,   //7
    //     Select_Encounter,   //8
    //     Encounter_1,        //9
    //     Encounter_2         //10
    // }
}

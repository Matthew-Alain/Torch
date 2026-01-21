using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager instance;

    private void Awake()
    {
        //Check if instance exists
        if (instance == null)
        {
            //If not, it does now
            instance = this;
        }
        else if (instance != this)
        {
            //If it does, and it's not this, destroy this to enforce singleton
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If the user is on a non-setting scene and presses escape...
        if (SceneManager.GetActiveScene().buildIndex != 1 && Input.GetKeyDown(KeyCode.Escape))
        {
            // Then load the setting scene
            LoadScene(1);
        }
    }

    private void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}

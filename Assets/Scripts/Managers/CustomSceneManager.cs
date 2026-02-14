using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{
    public static CustomSceneManager Instance;

    private void Awake()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //Now safe to create a new instance
        Instance = this;
        DontDestroyOnLoad(gameObject);
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

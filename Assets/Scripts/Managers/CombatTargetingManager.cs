using UnityEngine;

public class CombatTargetingManager : MonoBehaviour
{
    public static CombatTargetingManager Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Check if an instance already exists that isn't this
        if (Instance != null && Instance != this)
        {
            //If it does, destroy it
            Destroy(gameObject);
            return;
        }

        //This just allows manager scripts to be stored in a folder in the editor for organization, but during runtime, get deteached to avoid errors
        if (transform.parent != null)
        {
            transform.parent = null; // Detach from parent
        }

        //Now safe to create a new instance
        Instance = this;
    }

    

}

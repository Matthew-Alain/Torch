using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    
    private void Awake()
    {
        
    }

    public void LoadLastScene()
    {
        CustomSceneManager.Instance.LoadLastScene();
    }

}

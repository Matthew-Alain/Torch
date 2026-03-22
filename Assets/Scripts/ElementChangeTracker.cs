using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElementChangeTracker : MonoBehaviour
{
    void Awake()
    {
        // Slider
        var slider = GetComponent<Slider>();
        if (slider != null)
        {
            slider.onValueChanged.AddListener(_ => SettingsManager.Instance.MarkAsUnsavedChanges());
        }

        // Toggle
        var toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(_ => SettingsManager.Instance.MarkAsUnsavedChanges());
        }

        // TMP Dropdown
        var dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(_ => SettingsManager.Instance.MarkAsUnsavedChanges());
        }

        // TMP Input Field
        var input = GetComponent<TMP_InputField>();
        if (input != null)
        {
            input.onValueChanged.AddListener(_ => SettingsManager.Instance.MarkAsUnsavedChanges());
        }

        // Button
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {SettingsManager.Instance.MarkDirty(); });
        }
    }
}
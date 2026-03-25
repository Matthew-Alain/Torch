using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialItem : MonoBehaviour
{
    public TMP_Text titleText;
    public Button btnShowText;
    public Button btnIsRead;
    private int id;

    public void Setup(string title, bool isRead, int passedID)
    {
        titleText.SetText(title);
        btnIsRead.gameObject.SetActive(!isRead);

        id = passedID;

        btnShowText.onClick.AddListener(DisplayText);

    }

    public void DisplayText()
    {
        TutorialManager.Instance.UpdateDisplayText(id);
        btnIsRead.gameObject.SetActive(false);
        btnShowText.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    }

}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Popup : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text messageText;
    public Button closeButton;
    public TMP_InputField outputText;

    void Start ()
    {
        outputText.readOnly = true;
        closeButton.onClick.AddListener(()=>gameObject.SetActive(false));   
    }
    
    public void ShowPopup(string title, string message, string outputText = null)
    {
        gameObject.SetActive(true);
        titleText.text = title;
        messageText.text = message;
        if (!String.IsNullOrEmpty(outputText))
        {
            this.outputText.text = outputText;
            this.outputText.gameObject.SetActive(true);
        }
        else
        {
            this.outputText.gameObject.SetActive(false);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(this.gameObject.GetComponent<RectTransform>());
    }
}

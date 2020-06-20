using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class SignCanvasController : MonoBehaviour
{
    private string LastMessage = "";
    private TextMeshProUGUI MessageText;

    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI t = transform.GetComponentInChildren<TextMeshProUGUI>();
        if (t != null)
        {
            MessageText = t;
            t.text = LastMessage;
        }
    }

    public void ShowMessage(string text)
    {
        if (MessageText != null)
            MessageText.text = text;
        else
            LastMessage = text;
    }
}

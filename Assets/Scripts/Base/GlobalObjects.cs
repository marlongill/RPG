using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalObjects : MonoBehaviour
{
    public Canvas DialogueCanvas;
    public DialogueCanvasController DialogueController;

    public Canvas SignCanvas;
    public SignCanvasController SignController;

    public bool Paused = false;
    public bool SignActive = false;

    // Start is called before the first frame update
    void Start()
    {
        var canvasList = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (Canvas canvas in canvasList)
        {
            if (canvas.name == "DialogueCanvas")
            {
                DialogueCanvas = canvas;
                DialogueController = canvas.GetComponent<DialogueCanvasController>();
            }
            else if (canvas.name == "SignCanvas")
            {
                SignCanvas = canvas;
                SignController = canvas.GetComponent<SignCanvasController>();
            }
        }
    }

    public void ShowSign(string text)
    {
        SignCanvas.gameObject.SetActive(true);
        SignController.ShowMessage(text);
        SignActive = true;
        Paused = true;
    }    

    public void HideSign()
    {
        SignCanvas.gameObject.SetActive(false);
        SignActive = false;
        Paused = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GlobalObjects : MonoBehaviour
{
    public Canvas DialogueCanvas;
    public DialogueCanvasController DialogueCanvasController;

    public Canvas SignCanvas;
    public SignCanvasController SignCanvasController;

    private DialogueController DialogueController;

    public GameStates GameState = GameStates.Playing;

    // Start is called before the first frame update
    void Start()
    {
        DialogueCanvasController = GetComponent<DialogueCanvasController>();
        SignCanvasController = GetComponent<SignCanvasController>();
        DialogueController = GetComponent<DialogueController>();
    }

    public void ShowSign(string text)
    {
        SignCanvas.gameObject.SetActive(true);
        SignCanvasController.ShowMessage(text);
        GameState = GameStates.InSign;
    }    

    public void HideSign()
    {
        SignCanvas.gameObject.SetActive(false);
        GameState = GameStates.Playing;
    }
}

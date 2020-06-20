using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueCanvasController : MonoBehaviour
{
    private Text playerName;
    private Text npcName;
    private Text dialogueText;
    private Image playerBust;
    private Image npcBust;

    void Start()
    {
        playerName = transform.Find("PlayerName").GetComponent<Text>();
        npcName = transform.Find("NPCName").GetComponent<Text>();
        dialogueText = transform.Find("DialogueText").GetComponent<Text>();
        playerBust = transform.Find("PlayerBust").GetComponent<Image>();
        npcBust = transform.Find("NPCBust").GetComponent<Image>();
    }

    public void Initialise(string pName, string nName)
    {
        playerName.text = pName;
        npcName.text = nName;

        npcBust = Resources.Load("Characters/Busts/" + nName) as Image;
        if (npcBust == null)
            npcBust = Resources.Load("Characters/Busts/Default") as Image;

        playerBust = Resources.Load("Characters/Busts/Player") as Image;

        enabled = true;
    }

    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        GlobalObjects go = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalObjects>();

        playerName = go.DialogueCanvas.transform.Find("Panel").transform.Find("PlayerName").GetComponent<Text>();
        npcName = go.DialogueCanvas.transform.Find("Panel").transform.Find("NPCName").GetComponent<Text>();
        dialogueText = go.DialogueCanvas.transform.Find("Panel").transform.Find("DialogueText").GetComponent<Text>();
        playerBust = go.DialogueCanvas.transform.Find("Panel").transform.Find("PlayerBust").GetComponent<Image>();
        npcBust = go.DialogueCanvas.transform.Find("Panel").transform.Find("NPCBust").GetComponent<Image>();
    }

    public void Initialise(string pName, string nName)
    {
        playerName.text = pName;
        npcName.text = nName;

        Sprite[] sprites = Resources.LoadAll<Sprite>("Characters/Busts/Busts");
        Sprite npcSprite = null;
        Sprite playerSprite = null;

        for (int i = 0; i < sprites.Count(); i++)
        {
            if (sprites[i].name == nName)
                npcSprite = sprites[i];
            else if (sprites[i].name == "Player")
                playerSprite = sprites[i];
            else if (sprites[i].name == "Default" && npcSprite == null)
                npcSprite = sprites[i];
        }

        npcBust.sprite = npcSprite;
        playerBust.sprite = playerSprite;

        enabled = true;
    }

    public void SetText(string text)
    {
        dialogueText.text = text;
    }

    public void AdjustBustsHighlight(bool playerLit)
    {
        playerBust.color = playerLit ? Color.white : Color.grey;
        npcBust.color = playerLit ? Color.grey : Color.white;
    }

    void Update()
    {
        
    }
}

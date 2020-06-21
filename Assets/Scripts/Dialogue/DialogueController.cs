using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;

using Newtonsoft.Json;

public class DialogueController : MonoBehaviour
{
    private GlobalObjects Globals;
    private GameObject Player;
    private Dictionary<string, Dialogue> StoredDialogue = new Dictionary<string, Dialogue>();
    
    private DialogueTree CurrentTree;
    int CurrentTreeIndex;
    private DialogueEntry CurrentDialogue;
    private string CurrentNPC;
    private int CurrentStep;
    private int CurrentChoice = 0;

    void Start()
    {
        Globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalObjects>();
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    public void LoadNPCConversation(string npcName)
    {
        TextAsset jsonText = Resources.Load<TextAsset>("Dialogue/" + npcName);
        if (jsonText != null)
        {
            Dialogue npcDialogue = JsonConvert.DeserializeObject<Dialogue>(jsonText.ToString());
            StoredDialogue.Add(npcName, npcDialogue);
        }
    }

    private bool CheckCondition(string condition)
    {
        if (String.IsNullOrEmpty(condition))
            return true;

        // Conditions should be in the format ObjectName.Method(Param1, Param2) operand RequiredResult
        var re = new Regex(@"\s*(?<object>[^.]+).(?<method>[^\(]+)\((?<parameters>[^\)]*)\)\s(?<operand>[^\s]*)\s(?<value>.*)");
        Match match = re.Match(condition);
        if (!match.Success)
            throw new Exception();

        string objectName = match.Groups["object"].Value;
        string methodName = match.Groups["method"].Value;
        string parameters = match.Groups["parameters"].Value;
        string operand = match.Groups["operand"].Value;
        string desiredResult = match.Groups["value"].Value;

        // Build Parameters and return value object
        SendMessageCallParams methodCallParams = new SendMessageCallParams()
        {
            Parameters = parameters.Split(','),
            Result = ""
        };

        // Call the method on the object specified
        if (objectName == "Player")
            Player.SendMessage(methodName, methodCallParams);
        else
            GameObject.Find(objectName).SendMessage(methodName, methodCallParams);

        // Check the result
        switch (operand)
        {
            case "==": return methodCallParams.Result == desiredResult;
            case "!=": return methodCallParams.Result != desiredResult;
            case ">=": return Convert.ToDouble(methodCallParams.Result) >= Convert.ToDouble(desiredResult);
            case "<=": return Convert.ToDouble(methodCallParams.Result) <= Convert.ToDouble(desiredResult);
            case ">": return Convert.ToDouble(methodCallParams.Result) > Convert.ToDouble(desiredResult);
            case "<": return Convert.ToDouble(methodCallParams.Result) < Convert.ToDouble(desiredResult);
            default: return false;
        }
    }

    private void ParseActions(string action)
    {
        if (action == "")
            return;

        // Actions should be in the format ObjectName.Method(Param1, Param2)
        var re = new Regex(@"\s*(?<object>[^.]+).(?<method>[^\(]+)\((?<parameters>[^\)]*)\)");
        Match match = re.Match(action);
        if (!match.Success)
            throw new Exception();

        string objectName = match.Groups["object"].Value;
        string methodName = match.Groups["method"].Value;
        string[] parameters = match.Groups["parameters"].Value.Split(',');

        SendMessageCallParams methodCallParams = new SendMessageCallParams()
        {
            Parameters = parameters,
            Result = ""
        };

        switch (objectName)
        {
            case "Dialogue":
                switch (methodName)
                {
                    case "Go":
                        CurrentStep = Convert.ToInt32(parameters[0]) - 1;
                        CurrentChoice = 0;
                        break;
                    case "End":
                        EndConversation();
                        break;
                }
                break;
            case "Player": Player.SendMessage(methodName, methodCallParams); break;
            default: GameObject.Find(objectName).SendMessage(methodName, methodCallParams); break;
        }
    }

    private void ShowCurrentConversationStep()
    {
        CurrentDialogue = CurrentTree.dialogue[CurrentStep];
        string text = CurrentDialogue.text;
        if (CurrentDialogue.choices != null && CurrentDialogue.choices.Count > 0)
        {
            text += "\n";
            for (int i = 0; i < CurrentDialogue.choices.Count; i++)
            {
                DialogueChoice choice = CurrentDialogue.choices[i];
                text += "\n " + (i + 1).ToString() + ". <color=" + (i == CurrentChoice ? "Blue" : choice.visited ? "DarkBlue" : "Green") + ">" + choice.text + "</color>";
            }
        }
        Globals.DialogueCanvasController.SetText(text);
        Globals.DialogueCanvasController.AdjustBustsHighlight(CurrentDialogue.npc == "N");
    }

    public void StartConversation(string npcName)
    {
        if (StoredDialogue.ContainsKey(npcName))
        {
            Dialogue dialogue = StoredDialogue[npcName];
            List<DialogueTree> trees = dialogue.trees.Where(x => x.read == false).OrderBy(x => x.treeid).ToList();
            for (int i=0; i < trees.Count; i++)
            {
                bool result = true;
                try
                {
                    result = CheckCondition(trees[i].condition);
                }
                catch
                {
                    string message = String.Format("Could not parse condition '{0}' conversation for '{1}' in tree {2}",
                        trees[i].condition, npcName, i);
                    Debug.LogWarning(message);
                }

                if (result)
                {
                    CurrentTree = trees[i];
                    CurrentTreeIndex = i;
                    CurrentStep = 0;
                    CurrentNPC = npcName;
                    Globals.DialogueCanvasController.Initialise(Player.GetComponent<PlayerDetails>().PlayerName, npcName);
                    Globals.GameState = GameStates.InDialogue;
                    ShowCurrentConversationStep();

                    break;
                }
            }
        }
    }

    public void EndConversation()
    {
        StoredDialogue[CurrentNPC].trees[CurrentTreeIndex].read = true;
        Globals.DialogueCanvas.gameObject.SetActive(false);
        Globals.GameState = GameStates.Playing;
    }

    public void NextStep()
    {
        if (CurrentDialogue.choices != null)
        {
            CurrentDialogue.choices[CurrentChoice].visited = true;
            for (int i = 0; i < CurrentDialogue.choices[CurrentChoice].commands.Count; i++)
                ParseActions(CurrentDialogue.choices[CurrentChoice].commands[i]);
            ShowCurrentConversationStep();
        }
        else
        {
            for (int i = 0; i < CurrentDialogue.commands.Count; i++)
                ParseActions(CurrentDialogue.commands[i]);
            ShowCurrentConversationStep();
        }
    }

    public void NextOption()
    {
        if (CurrentDialogue.choices == null)
            return;

        if (CurrentChoice < CurrentDialogue.choices.Count - 1)
        {
            CurrentChoice++;
            ShowCurrentConversationStep();
        }
    }

    public void PreviousOption()
    {
        if (CurrentDialogue.choices == null)
            return;

        if (CurrentChoice > 0)
        {
            CurrentChoice--;
            ShowCurrentConversationStep();
        }
    }
}

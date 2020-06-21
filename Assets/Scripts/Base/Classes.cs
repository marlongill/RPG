using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Map Point of Interest 
[System.Serializable]
public struct POI
{
    [SerializeField] public string Name;
    [SerializeField] public Vector2Int Location;
}

// Dialogue Classes
public class DialogueChoice
{
    public string text { get; set; }
    public List<string> commands { get; set; }
    public bool visited { get; set; }

    public DialogueChoice()
    {
        visited = false;
    }
}

public class DialogueEntry
{
    public int id { get; set; }
    public string condition { get; set; }
    public string npc { get; set; }
    public string text { get; set; }
    public List<DialogueChoice> choices { get; set; }
    public List<string> commands { get; set; }
}

public class DialogueTree
{
    public int treeid { get; set; }
    public string condition { get; set; }
    public List<DialogueEntry> dialogue { get; set; }
    public bool read { get; set; }
}

public class Dialogue
{
    public List<DialogueTree> trees;
}

// Class for passing parameters to and receiving results from methods called using SendMessage
public class SendMessageCallParams
{
    public string[] Parameters;
    public string Result;
}


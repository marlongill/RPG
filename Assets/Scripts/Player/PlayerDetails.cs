using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class PlayerDetails : MonoBehaviour
{
    public string PlayerName;

    public List<int> CurrentQuests;
    public List<int> CompletedQuests;

    public void QuestCount(SendMessageCallParams parameters)
    {
        parameters.Result = CurrentQuests.Count.ToString();
    }

    public void OnQuest(SendMessageCallParams parameters)
    {
        parameters.Result = "True";
    }

    public void ReceiveAsset(SendMessageCallParams parameters)
    {
        Debug.Log("Player Received Asset #" + parameters.Parameters[0]);
    }    
}

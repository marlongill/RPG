using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.StartsWith("NPC") || collision.gameObject.tag == "Player")
        {
            // Get Reference to the NPC object for this character
            string npcName = collision.otherCollider.gameObject.name.Replace("NPC_", "");
            MapObject currentMap = GameObject.FindGameObjectWithTag("Grid").GetComponent<MapController>().CurrentMap;
            NPC npc = currentMap.NPCs[currentMap.NPCNames.IndexOf(npcName)];
            npc.RecalculatePath(collision.gameObject.tag);            
        }
    }
}

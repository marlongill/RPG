using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "NPC" || other.gameObject.tag == "Player")
        {
            if (this.gameObject.tag != "Player")
            {
                // Get Reference to the NPC object for this character
                string npcName = this.gameObject.name.Replace("NPC_", "");
                MapObject currentMap = GameObject.FindGameObjectWithTag("Grid").GetComponent<MapController>().CurrentMap;
                NPC npc = currentMap.NPCs[currentMap.NPCNames.IndexOf(npcName)];
                if (npc.CanMove)
                {
                    npc.RecalculatePath(other.gameObject);
                }
            }
        }
    }
}

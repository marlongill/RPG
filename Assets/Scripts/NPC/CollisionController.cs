using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionController : MonoBehaviour
{
    private GlobalObjects Globals;

    private void Start()
    {
        Globals = GameObject.FindGameObjectWithTag("Globals").GetComponent<GlobalObjects>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "NPC" || other.gameObject.tag == "Player")
        {
            // Box Collider is for interactions
            if (other is BoxCollider2D && other.gameObject.tag == "Player")
            {
                NPC npc = Globals.GetComponent<MapController>().CurrentMap.GetNPC(this.gameObject.name.Replace("NPC_", ""));
                other.gameObject.GetComponent<InputController>().SetInteractableNPC(npc);
            }
            else
            {
                // Polygon collider is for actual collisions
                if (this.gameObject.tag != "Player")
                {
                    // Get Reference to the NPC object for this character
                    string npcName = this.gameObject.name.Replace("NPC_", "");
                    MapObject currentMap = Globals.GetComponent<MapController>().CurrentMap;
                    NPC npc = currentMap.NPCs[currentMap.NPCNames.IndexOf(npcName)];
                    if (npc.CanMove)
                    {
                        npc.RecalculatePath(other.gameObject);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Box Collider is for interactions
        if (other is BoxCollider2D && other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<InputController>().SetInteractableNPC(null);
        }
    }
}

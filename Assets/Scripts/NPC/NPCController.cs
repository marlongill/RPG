using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class NPCController : MonoBehaviour
{
    public static NPC CreateNPC(string npcName, string modelName, float x, float y, MapObject map)
    {
        GameObject sprite = Instantiate(Resources.Load<GameObject>("Characters/" + modelName));
        sprite.tag = "NPC";
        sprite.name = "NPC_" + npcName;
        sprite.layer = LayerMask.NameToLayer("Player & NPC");

        // Add Animation Sprites to NPC object
        SpriteAnimator anim = sprite.GetComponent<SpriteAnimator>();
        anim.Initialise();

        SpriteRenderer renderer = sprite.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = (int)((map.MapHeight - y) * 100);
        renderer.transform.position = new Vector3(x, y, -0.09f);
        renderer.transform.localScale = new Vector3(1, 1, 1);
        renderer.sprite = anim.Sprites[1];

        // Add Pathfinder to NPC
        sprite.AddComponent<PathFinder>();

        // Add Polygon Collider and Rigid Body
        Rigidbody2D rb = sprite.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        
        PolygonCollider2D pc = sprite.AddComponent<PolygonCollider2D>();
        
        // Add Collision Controller
        sprite.AddComponent<CollisionController>();

        // Return NPC
        return new NPC(npcName, sprite, map);
    }
}
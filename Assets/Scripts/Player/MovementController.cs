using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]

public class MovementController : MonoBehaviour
{
    public LayerMask collisionMask;

    BoxCollider2D collisionBox;
    Grid grid;
    MapController mapController;

    void Start()
    {
        collisionBox = GetComponent<BoxCollider2D>();

        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        mapController = grid.GetComponent<MapController>();
    }

    public TileMetaData GetTileAttributesAtPoint(Vector2 point, int layerIndex)
    {

        Vector3Int cell = grid.WorldToCell(point);
        Tilemap map = grid.gameObject.GetComponentsInChildren<Tilemap>()[layerIndex];
        int tileID = mapController.CurrentMap.GetCell("Collisions", cell.x, cell.y);

        return mapController.CurrentMap.TileAttributes[tileID];
    }

    public TileMetaData GetTileBelowPlayer(Vector2 point)
    {
        Vector3Int cell = grid.WorldToCell(point);
        int tileID = 0;
        int layer = 1;
        while (layer >= 0 && tileID == 0)
        {
            string layerName = "";
            switch (layer)
            {
                case 0: layerName = "Background"; break;
                case 1: layerName = "Collisions"; break;
            }
            Tilemap map = grid.gameObject.GetComponentsInChildren<Tilemap>()[layer];
            tileID = mapController.CurrentMap.GetCell(layerName, cell.x, cell.y);
            layer--;
        }
        return mapController.CurrentMap.TileAttributes[tileID];
    }

    public void Move(ref Vector2 velocity)
    {
        RaycastHit2D[] hits = new RaycastHit2D[10];
        ContactFilter2D filter = new ContactFilter2D() { layerMask = collisionMask };

        int numHits = collisionBox.Cast(velocity, filter, hits, velocity.magnitude);

        if (numHits > 0)
        {
            // Reset Velocity Vector
            velocity = Vector2.zero;

            // Check the cell we have collided with
            TileMetaData tileMeta = GetTileAttributesAtPoint(hits[0].point, 1);

            if (tileMeta.Action != "")
            {
                string action = tileMeta.Action;
                if (action.StartsWith("Teleport"))
                {
                    string[] aParts = action.Split(';');
                    string mapName = aParts[1];
                    float playerX = (float)Convert.ToDouble(aParts[2]);
                    float playerY = (float)Convert.ToDouble(aParts[3]);
                    mapController.ChangeMap(mapName, playerX, playerY);
                }
            }
        }

        transform.Translate(velocity);
    }
}

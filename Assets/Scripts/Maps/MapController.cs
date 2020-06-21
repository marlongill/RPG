using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D;

public class MapController : MonoBehaviour
{
    private Grid grid;

    public Dictionary<string, MapObject> LoadedMaps = new Dictionary<string, MapObject>();
    public MapObject CurrentMap;
    
    public MapObject LoadMap(string mapName)
    {
        MapObject map = Instantiate(Resources.Load<MapObject>("Maps/" + mapName));
        map.Initialise();

        // Add map to list of maps
        LoadedMaps.Add(mapName, map);

        return map;
    }

    public void ChangeMap(string mapName, float playerX, float playerY)
    {
        if (CurrentMap != null)
            CurrentMap.Deactivate();

        // Get Map Object
        MapObject map;
        if (LoadedMaps.ContainsKey(mapName))
        {
            map = LoadedMaps[mapName];
            CurrentMap = map;
            CurrentMap.Render(grid);
            CurrentMap.Activate();
        }
        else
        {
            map = LoadMap(mapName);

            // Render the map
            CurrentMap = map;
            CurrentMap.Render(grid);
        }

        // Change Player Position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(playerX, playerY, -0.1f);
    }

    public List<Vector2Int> GetBlockedCells(GameObject objectToAvoid)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Grid grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        Vector3Int cell = Vector3Int.zero;

        cell = grid.WorldToCell(objectToAvoid.transform.position);

        result.Add(new Vector2Int(cell.x, cell.y));
        if (cell.x > 0) result.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < CurrentMap.MapWidth - 1) result.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0) result.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < CurrentMap.MapHeight - 1) result.Add(new Vector2Int(cell.x, cell.y + 1));

        return result;
    }

    public void Start()
    {
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid>();
        ChangeMap("Mayors_House", 15.5f, 11.5f);
    }

    public void Update()
    {
        CurrentMap.Update_Map();
    }
}

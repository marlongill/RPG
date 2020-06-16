using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.U2D;

public class MapController : MonoBehaviour
{
    private Dictionary<string, MapObject> LoadedMaps = new Dictionary<string, MapObject>();

    public MapObject CurrentMap;
    
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
            CurrentMap.Render(this.GetComponentInParent<Grid>());
            CurrentMap.Activate();
        }
        else
        {
            map = Instantiate(Resources.Load<MapObject>("Maps/" + mapName));

            // Render the map
            CurrentMap = map;
            CurrentMap.Initialise();
            CurrentMap.Render(this.GetComponentInParent<Grid>());

            // Add map to list of maps
            LoadedMaps.Add(mapName, map);
        }

        // Change Player Position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = new Vector3(playerX, playerY, -0.1f);
    }

    public List<Vector2Int> GetBlockedCells(string character)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Grid grid = GetComponent<Grid>();
        Vector3Int cell = Vector3Int.zero;

        if (character == "Player")
            cell = grid.WorldToCell(GameObject.FindGameObjectWithTag(character).transform.position);
        else if (character.StartsWith("NPC"))
            cell = grid.WorldToCell(CurrentMap.GetNPC(character).gameObject.transform.position);

        result.Add(new Vector2Int(cell.x, cell.y));
        if (cell.x > 0) result.Add(new Vector2Int(cell.x - 1, cell.y));
        if (cell.x < CurrentMap.MapWidth - 1) result.Add(new Vector2Int(cell.x + 1, cell.y));
        if (cell.y > 0) result.Add(new Vector2Int(cell.x, cell.y - 1));
        if (cell.y < CurrentMap.MapHeight - 1) result.Add(new Vector2Int(cell.x, cell.y + 1));

        return result;
    }

    public void Start()
    {
        ChangeMap("Start_Town", 10.5f, 26.5f);

        NPC npc = NPCController.CreateNPC("John", "NPC_Male1", 24.5f, 5f, CurrentMap);
        LoadedMaps["Start_Town"].AddNPC(npc);
    }

    public void Update()
    {
        CurrentMap.Update_Map();
    }
}

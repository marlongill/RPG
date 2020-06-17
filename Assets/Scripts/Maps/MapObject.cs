using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

using Newtonsoft.Json;

[Serializable]
public class MapObject : MonoBehaviour
{
    private class MetaDataItem
    {
        public string Type;
        public string Value;
    }

    private class MetaData
    {
        public List<MetaDataItem> Data;
    }

    private class MetaDataInfo
    {
        public List<MetaData> Items;
    }
    
    private class LayerInfo
    {
        public string Layer;
        public List<int> SpriteID;
    }

    private class MapCellInfo
    {
        public List<LayerInfo> Layers;
    }

    private class NPCInfo
    {
        public string Name;
        public string ModelName;
        public int X;
        public int Y;
        public float Speed;
        public string POI;
        public string Wander;
        public string VisFunc;
        public string VisParam;
        public string IntFunc;
        public string IntParam;
    }

    private class NPCData
    {
        public List<NPCInfo> NPC;
    }

    public string Name;
    public string SpriteSheetName;
    public MapCells mapcells = new MapCells(10, 10);
    public List<MapCells> Layers = new List<MapCells>();
    public List<string> LayerNames = new List<string>();
    public int MapWidth;
    public int MapHeight;
    public int TileWidth;
    public int TileHeight;
    public List<TileMetaData> TileAttributes = new List<TileMetaData>();
    public List<NPC> NPCs = new List<NPC>();
    public List<string> NPCNames = new List<string>();
    public List<POI> PointsOfInterest = new List<POI>();
    public List<string> PointOfInterestNames = new List<string>();
    public List<List<bool>> PathFindingGrid = new List<List<bool>>();


    public string MetaDataJson = "";
    public string LayersJson = "";
    public string NPCJson = "";

    private bool IsInitialised = false;

    public void AddNPC(NPC npc)
    {
        NPCNames.Add(npc.name);
        NPCs.Add(npc);

        // Block cells occupied by static NPCs to the Path Finding Grid
        if (!npc.CanMove)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Grid");
            Grid grid = go.GetComponent<Grid>();
            Vector3 position = npc.gameObject.GetComponent<SpriteRenderer>().transform.position;
            Vector3Int cell = grid.WorldToCell(position);

        }
    }

    public NPC GetNPC(string npcName)
    {
        return NPCs[NPCNames.IndexOf(npcName)];
    }

    public int GetCell(string layer, int x, int y)
    {
        if (LayerNames.Contains(layer))
        {
            return Layers[LayerNames.IndexOf(layer)].Cells[x, y];
        }
        else
            return 0;
    }

    public void CreatePathFindingArray(string layerName)
    {
        PathFindingGrid = new List<List<bool>>();
        for (int y = 0; y < MapHeight; y++)
        {
            PathFindingGrid.Add(new List<bool>());
            for (int x = 0; x < MapWidth; x++)
            {
                try
                {
                    PathFindingGrid[y].Add(TileAttributes[GetCell(layerName, x, y)].IsSolid);
                }
                catch (Exception ex)
                {
                    Debug.Log(String.Format("Pathfinding Array @ {0}, {1}\n{2}", x, y, ex.Message));
                }
            }
        }
    }

    public POI GetPointOfInterestByIndex(int index)
    {
        return PointsOfInterest[index];
    }

    public POI GetPointOfInterestByName(string name)
    {
        if (PointOfInterestNames.Contains(name))
            return PointsOfInterest[PointOfInterestNames.IndexOf("name")];
        else
            return new POI();
    }

    public List<List<bool>> GetPathFindingGrid(bool includeNPC)
    {
        return PathFindingGrid;
    }

    public void Initialise()
    {
        if (IsInitialised)
            return;

        // Get Tile Attributes from named map if we have no sheet
        string json = "";

        if (SpriteSheetName != Name)
        {
            MapController mapController = GameObject.FindGameObjectWithTag("Grid").GetComponent<MapController>();
            TileAttributes = mapController.LoadedMaps[SpriteSheetName].TileAttributes;
        }
        else
        {
            json = "{'Items': " + MetaDataJson + "}";
            MetaDataInfo metaData = JsonConvert.DeserializeObject<MetaDataInfo>(json);

            for (int i = 0; i < metaData.Items.Count; i++)
            {
                TileMetaData m = new TileMetaData();
                foreach (MetaDataItem meta in metaData.Items[i].Data)
                    switch (meta.Type)
                    {
                        case "Solid": m.IsSolid = meta.Value == "True"; break;
                        case "Friction": m.Friction = meta.Value; break;
                        case "Damage": m.Damage = Convert.ToInt32(meta.Value == "" ? "0" : meta.Value); break;
                        case "Action": m.Action = meta.Value; break;
                        case "Ladder": m.IsLadder = meta.Value == "True"; break;
                    }
                TileAttributes.Add(m);
            }
        }

        json = "{'Layers':" + LayersJson + "}";
        MapCellInfo mapCellInfo = JsonConvert.DeserializeObject<MapCellInfo>(json);

        Layers = new List<MapCells>();
        LayerNames = new List<string>();
        for (int i = 0; i < mapCellInfo.Layers.Count; i++)
        {
            MapCells layer = new MapCells(MapWidth, MapHeight);
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    int c = y * MapWidth + x;
                    layer.Cells[x, y] = mapCellInfo.Layers[i].SpriteID[c];
                }
            }
            LayerNames.Add(mapCellInfo.Layers[i].Layer);
            Layers.Add(layer);
        }

        CreatePathFindingArray("Collisions");

        // Create NPCs
        json = "{'NPC' :" + NPCJson + "}";
        NPCData npcData = JsonConvert.DeserializeObject<NPCData>(json);
        foreach(NPCInfo npcInfo in npcData.NPC)
        {
            NPC npc = NPCController.CreateNPC(npcInfo.Name, npcInfo.ModelName, npcInfo.X, MapHeight - npcInfo.Y - 1, this);
            npc.CanMoveToPOI = npcInfo.POI == "True";
            npc.CanWander = npcInfo.Wander == "True";
            npc.InteractFunction = npcInfo.IntFunc;
            npc.InteractParams = npcInfo.IntParam;
            npc.maxSpeed = npcInfo.Speed;
            npc.VisibilityFunction = npcInfo.VisFunc;
            npc.VisibilityParams = npcInfo.VisParam;

            AddNPC(npc);
        }


        IsInitialised = true;
    }

    public void Render(Grid grid)
    {
        for (int l = 0; l < Layers.Count; l++)
        {
            Tilemap map = grid.transform.Find("Tilemap_" + LayerNames[l]).gameObject.GetComponent<Tilemap>();
            map.origin = new Vector3Int(0, 0, 0);
            map.size = new Vector3Int(0, 0, 0);
            map.ResizeBounds();

            int x, y;
            for (x = 0; x < MapWidth; x++)
            {
                for (y = 0; y < MapHeight; y++)
                {
                    try
                    {
                        Tile tile = Resources.Load<Tile>("Maps/" + SpriteSheetName + "/" + SpriteSheetName + "_Tile_" + Layers[l].Cells[x, y].ToString());
                        map.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                    catch
                    {
                        Debug.Log(String.Format("Failed to place tile @ {0}, {1} on layer {2}", x, y, LayerNames[l]));
                    }
                }
            }
        }

        // Set Camera Bounds
        GameObject camgo = GameObject.FindGameObjectWithTag("MainCamera");
        PixelPerfectCamera camera = camgo.GetComponent<PixelPerfectCamera>();

        float minX = (float)camera.refResolutionX / TileWidth / 2;
        float minY = (float)camera.refResolutionY / TileHeight / 2;
        float maxX = (float)MapWidth - minX;
        float maxY = (float)MapHeight - minY;

        CameraController controller = camgo.GetComponent<CameraController>();
        controller.minPosition = new Vector2(minX, minY);
        controller.maxPosition = new Vector2(maxX, maxY);
    }

    public void Activate()
    {
        foreach (NPC npc in NPCs)
        {
            npc.gameObject.SetActive(true);
        }
    }

    public void Deactivate()
    {
        foreach (NPC npc in NPCs)
        {
            npc.gameObject.SetActive(false);
        }
    }

    public void Update_Map()
    {
        foreach (NPC npc in NPCs)
        {
            npc.Update();
        }
    }
}

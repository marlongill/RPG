using UnityEngine;

public class MapOld : MonoBehaviour
{
    //public List<Tile> Tiles = new List<Tile>();
    //public Dictionary<string, List<List<int>>> Cells = new Dictionary<string, List<List<int>>>();
    public int MapWidth;
    public int MapHeight;
    public int TileWidth;
    public int TileHeight;
    //public List<TileMetaData> TileAttributes = new List<TileMetaData>();
    //public List<NPC> NPCs = new List<NPC>();
    //public List<string> NPCNames = new List<string>();
    //public List<POI> PointsOfInterest = new List<POI>();
    //public List<string> PointOfInterestNames = new List<string>();
    //public List<List<bool>> PathFindingGrid = new List<List<bool>>();

    //public void AddNPC(NPC npc)
    //{
    //    //NPCNames.Add(npc.name);
    //    //NPCs.Add(npc);
    //}

    //public void CreatePathFindingArray(string layerName)
    //{
    //    //PathFindingGrid = new List<List<bool>>();
    //    //for (int y = 0; y < MapHeight; y++)
    //    //{
    //    //    PathFindingGrid.Add(new List<bool>());
    //    //    for (int x = 0; x < MapWidth; x++)
    //    //    {
    //    //        try
    //    //        {
    //    //            PathFindingGrid[y].Add(TileAttributes[Cells[layerName][y][x]].IsSolid);
    //    //        }
    //    //        catch (Exception ex)
    //    //        {
    //    //            Debug.Log(String.Format("Pathfinding Array @ {0}, {1}\n{2}", x, y, ex.Message));
    //    //        }
    //    //    }
    //    //}
    //}

    //public POI GetPointOfInterestByIndex(int index)
    //{
    //    return PointsOfInterest[index];
    //}

    //public POI GetPointOfInterestByName(string name)
    //{
    //    if (PointOfInterestNames.Contains(name))
    //        return PointsOfInterest[PointOfInterestNames.IndexOf("name")];
    //    else
    //        return new POI();
    //}

    public void Start()
    {
    }

    public void Update()
    {
        //foreach (NPC npc in NPCs)
        //{
        //    npc.Update();
        //}
    }
}


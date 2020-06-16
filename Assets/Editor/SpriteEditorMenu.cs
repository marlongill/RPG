using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using UnityEditor;
using UnityEditorInternal;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class SpriteEditorImports {

    public class MetaDataItem
    {
        public string Type;
        public string Value;
    }

    public class MetaData
    {
        public List<MetaDataItem> Data;
    }

    private class Sheet 
    {
        public int CellWidth;
        public int CellHeight;
        public List<Vector2Int> MaskPoints;
        public List<List<int>> Masks;
        public List<MetaData> MetaData;
        public List<Animation> Animations;
    }

    private class LayerInfo
    {
        public string Layer;
        public List<int> SpriteID;
    }

    private class PointOfInterest
    {
        public string Name;
        public int x;
        public int y;
    }

    private class MapInfo
    {
        public string SpriteSheetName;
        public int MapWidth;
        public int MapHeight;
        public int CellWidth;
        public int CellHeight;
        public List<LayerInfo> Layers;
        public List<PointOfInterest> POI;
    }

    private static bool ReadJsonFile(
        string path,
        string mapName,
        ref Texture2D texture,
        ref Sheet sheet,
        ref MapInfo mapInfo,
        ref bool hasSpriteSheet
    )
    {
        hasSpriteSheet = false;

        // Do we need to update the spritesheet, sprites and tiles?
        if (!File.Exists(path + "\\" + mapName + "_update.txt"))
            return false;

        if (File.Exists(path + "\\" + mapName + ".png"))
        {
            texture = new Texture2D(2, 2);
            byte[] pngData = File.ReadAllBytes(path + "\\" + mapName + ".png");
            MemoryStream bmpStream = new MemoryStream();
            bmpStream.Write(pngData, 0, pngData.Length);
            texture.LoadImage(bmpStream.ToArray());
            bmpStream.Close();

            // Get Sprite Sheet Info JSON File
            sheet = JsonConvert.DeserializeObject<Sheet>(File.ReadAllText(path + "\\" + mapName + ".sheet"));

            hasSpriteSheet = true;
        }

        // Get Map Info JSON File
        mapInfo = JsonConvert.DeserializeObject<MapInfo>(File.ReadAllText(path + "\\" + mapName + ".map"));

        return true;
    }

    private static bool ReadCharacterJsonFile(
        string path,
        string mapName,
        ref Texture2D texture,
        ref Sheet sheet
    )
    {
        // Do we need to update the spritesheet, sprites and tiles?
        if (!File.Exists(path + "\\" + mapName + "_update.txt"))
            return false;

        if (File.Exists(path + "\\" + mapName + ".png"))
        {
            texture = new Texture2D(2, 2);
            byte[] pngData = File.ReadAllBytes(path + "\\" + mapName + ".png");
            MemoryStream bmpStream = new MemoryStream();
            bmpStream.Write(pngData, 0, pngData.Length);
            texture.LoadImage(bmpStream.ToArray());
            bmpStream.Close();

            // Get Sprite Sheet Info JSON File
            sheet = JsonConvert.DeserializeObject<Sheet>(File.ReadAllText(path + "\\" + mapName + ".sheet"));
        }

        return true;
    }

    private class SpritePhysicsShapeImporter
    {
        private TextureImporter m_TI;
        private SerializedObject m_TISerialized;

        public SpritePhysicsShapeImporter(TextureImporter ti)
        {
            m_TI = ti;
            m_TISerialized = new SerializedObject(m_TI);
        }

        public List<Vector2[]> GetPhysicsShape(int index)
        {
            var physicsShapeProperty = GetPhysicsShapeProperty(index);
            var physicsShape = new List<Vector2[]>();
            for (int j = 0; j < physicsShapeProperty.arraySize; ++j)
            {
                SerializedProperty physicsShapePathSP = physicsShapeProperty.GetArrayElementAtIndex(j);
                var o = new Vector2[physicsShapePathSP.arraySize];
                for (int k = 0; k < physicsShapePathSP.arraySize; ++k)
                {
                    o[k] = physicsShapePathSP.GetArrayElementAtIndex(k).vector2Value;
                }
                physicsShape.Add(o);
            }
            return physicsShape;
        }

        public void SetPhysicsShape(int index, List<Vector2[]> data)
        {
            var physicsShapeProperty = GetPhysicsShapeProperty(index);
            physicsShapeProperty.ClearArray();
            for (int j = 0; j < data.Count; ++j)
            {
                physicsShapeProperty.InsertArrayElementAtIndex(j);
                var o = data[j];
                SerializedProperty outlinePathSP = physicsShapeProperty.GetArrayElementAtIndex(j);
                outlinePathSP.ClearArray();
                for (int k = 0; k < o.Length; ++k)
                {
                    outlinePathSP.InsertArrayElementAtIndex(k);
                    outlinePathSP.GetArrayElementAtIndex(k).vector2Value = o[k];
                }
            }
            m_TISerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Save()
        {
            AssetDatabase.ForceReserializeAssets(new string[] { m_TI.assetPath }, ForceReserializeAssetsOptions.ReserializeMetadata);
            m_TI.SaveAndReimport();
        }

        private SerializedProperty GetPhysicsShapeProperty(int index)
        {
            if (m_TI.spriteImportMode == SpriteImportMode.Multiple)
            {
                var spriteSheetSP = m_TISerialized.FindProperty("m_SpriteSheet.m_Sprites");
                if (index < spriteSheetSP.arraySize)
                {
                    var element = spriteSheetSP.GetArrayElementAtIndex(index);
                    return element.FindPropertyRelative("m_PhysicsShape");
                }
            }
            return m_TISerialized.FindProperty("m_SpriteSheet.m_PhysicsShape");
        }
    }

    [MenuItem("Assets/Import/Import Sprite Editor Maps")]
	private static void ImportEditorMaps()
	{
		string path = EditorUtility.OpenFolderPanel("Import Sprite Editor Maps", "", "");
		string[] mapFolders = Directory.GetDirectories(path);
        foreach (string mapFolder in mapFolders)
        {
            string unityFolder = mapFolder + "\\Unity";
            string mapName = mapFolder.Substring(mapFolder.IndexOf("\\") + 1).Replace(" ", "_");

            if (Directory.Exists(unityFolder))
            {
                MapInfo mapInfo = null;
                Sheet sheet = null;
                Texture2D texture = null;
                bool hasSpriteSheet = false;

                bool needUpdate = ReadJsonFile(unityFolder, mapName, ref texture, ref sheet, ref mapInfo, ref hasSpriteSheet);

                if (needUpdate)
                {
                    if (hasSpriteSheet)
                    {
                        // Add the image to the assets library
                        texture.name = mapName;
                        var bytes = texture.EncodeToPNG();
                        string assetPath = "Assets/Resources/Maps/" + mapName + "/";

                        if (!Directory.Exists(assetPath))
                            Directory.CreateDirectory(assetPath);

                        string fileName = assetPath + mapName + ".png";
                        File.WriteAllBytes(fileName, bytes);
                        AssetDatabase.Refresh();
                        AssetDatabase.ImportAsset(fileName);
                        TextureImporter importer = AssetImporter.GetAtPath(fileName) as TextureImporter;
                        if (importer.spriteImportMode == SpriteImportMode.Multiple)
                        {
                            importer.spriteImportMode = SpriteImportMode.Single;
                            AssetDatabase.WriteImportSettingsIfDirty(fileName);
                        }
                        TextureImporterSettings settings = new TextureImporterSettings();
                        importer.ReadTextureSettings(settings);
                        settings.spriteGenerateFallbackPhysicsShape = false;
                        importer.SetTextureSettings(settings);

                        importer.spriteImportMode = SpriteImportMode.Multiple;
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spritePixelsPerUnit = sheet.CellWidth;

                        // Build Sprite Sheet
                        Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture, new Vector2(0, 0), new Vector2(sheet.CellWidth, sheet.CellHeight), new Vector2(0, 0), true);

                        var metas = new List<SpriteMetaData>();
                        int rectNum = 0;

                        foreach (Rect rect in rects)
                        {
                            var meta = new SpriteMetaData();
                            meta.pivot = Vector2.zero;
                            meta.alignment = (int)SpriteAlignment.Center;
                            meta.rect = rect;
                            meta.name = mapName + "_" + rectNum++;
                            metas.Add(meta);
                        }

                        importer.spritesheet = metas.ToArray();

                        AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);

                        // Set Physics Shapes
                        UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Maps/" + mapName + "/" + mapName + ".png");

                        for (int i = 0; i < sprites.Length; i++)
                        {
                            Sprite sprite = sprites[i] as Sprite;

                            if (sprite != null)
                            {
                                try
                                {
                                    int idx = Convert.ToInt32(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1));
                                    List<Vector2> points = new List<Vector2>();
                                    if (sheet.Masks[idx].Count > 0)
                                    {
                                        for (int e = 0; e < sheet.Masks[idx].Count; e++)
                                        {
                                            Vector2Int pt = sheet.MaskPoints[sheet.Masks[idx][e]];
                                            float x = (float)pt.x;
                                            if (x == (sheet.CellWidth / 2) - 1 || x == sheet.CellWidth - 1) x++;
                                            x = 0 - ((sheet.CellWidth / 2) - x);

                                            float y = (float)pt.y;
                                            if (y == (sheet.CellHeight / 2) - 1 || y == sheet.CellHeight - 1) y++;
                                            y = y + (sheet.CellHeight / 2);

                                            points.Add(new Vector2(x, sheet.CellHeight - y));
                                        }

                                        var spritePhysicsShapeImporter = new SpritePhysicsShapeImporter(importer);
                                        spritePhysicsShapeImporter.SetPhysicsShape(idx, new List<Vector2[]>() { points.ToArray() });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex.Message);
                                }
                            }
                        }

                        AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);

                        // Build Tiles
                        AssetDatabase.StartAssetEditing();

                        // Initialise Array
                        sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Maps/" + mapName + "/" + mapName + ".png");
                        List<Tile> tiles = new List<Tile>();
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            tiles.Add(null);
                        }

                        for (int i = 0; i < sprites.Length; i++)
                        {
                            Sprite sprite = sprites[i] as Sprite;

                            if (sprite != null)
                            {
                                try
                                {
                                    int idx = Convert.ToInt32(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1));
                                    Tile tile = ScriptableObject.CreateInstance<Tile>();
                                    tile.colliderType = sheet.Masks[idx].Count == 0 ? Tile.ColliderType.None : Tile.ColliderType.Sprite;
                                    tile.hideFlags = HideFlags.None;
                                    tile.sprite = sprite;
                                    tile.name = sprite.name;

                                    path = "Assets/Resources/Maps/" + mapName + "/" + mapName + "_Tile_" + idx.ToString() + ".asset";
                                    AssetDatabase.CreateAsset(tile, path);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex.Message);
                                }
                            }
                        }

                        AssetDatabase.StopAssetEditing();
                    }

                    // Create Map GameObject
                    GameObject go = new GameObject(mapName);
                    MapObject map = go.AddComponent<MapObject>();

                    map.SpriteSheetName = mapInfo.SpriteSheetName;
                    map.MapWidth = mapInfo.MapWidth;
                    map.MapHeight = mapInfo.MapHeight;
                    map.TileWidth = mapInfo.CellWidth;
                    map.TileHeight = mapInfo.CellHeight;

                    // Add Points of Interest
                    foreach (PointOfInterest poi in mapInfo.POI)
                    {
                        map.PointOfInterestNames.Add(poi.Name);
                        map.PointsOfInterest.Add(new POI() { Name = poi.Name, Location = new Vector2Int(poi.x, poi.y) });
                    }

                    ////public Dictionary<string, NPCController.NPC> NPCs = new Dictionary<string, NPCController.NPC>();
                    ////public List<List<bool>> PathFindingGrid = new List<List<bool>>();

                    // Store Map Layers and Cells Information
                    string json = JsonConvert.SerializeObject(mapInfo.Layers);
                    map.LayersJson = json;

                    // Store Tile Attributes Information
                    json = JsonConvert.SerializeObject(sheet.MetaData);
                    map.MetaDataJson = json;

                    // Create Prefab
                    string localPath = "Assets/Resources/Maps/" + mapName + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(go, localPath);

                    // Delete Update file
                    //File.Delete(unityFolder + "\\" + mapName + "_update.txt.meta");
                    //File.Delete(unityFolder + "\\" + mapName + "_update.txt");
                }
            }
        }
	}

    [MenuItem("Assets/Import/Import Sprite Editor Characters")]
    private static void ImportEditorCharacters()
    {
        string path = EditorUtility.OpenFolderPanel("Import Sprite Editor Characters", "", "");
        string[] mapFolders = Directory.GetDirectories(path);
        foreach (string characterFolder in mapFolders)
        {
            string unityFolder = characterFolder + "\\Unity";
            string characterName = characterFolder.Substring(characterFolder.IndexOf("\\") + 1).Replace(" ", "_");

            if (Directory.Exists(unityFolder))
            {
                Sheet sheet = null;
                Texture2D texture = null;

                bool needUpdate = ReadCharacterJsonFile(unityFolder, characterName, ref texture, ref sheet);

                if (needUpdate)
                {
                    // Add the image to the assets library
                    texture.name = characterName;
                    var bytes = texture.EncodeToPNG();
                    string assetPath = "Assets/Resources/Characters/" + characterName + "/";

                    if (!Directory.Exists(assetPath))
                        Directory.CreateDirectory(assetPath);

                    string fileName = assetPath + characterName + ".png";
                    File.WriteAllBytes(fileName, bytes);
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(fileName);
                    TextureImporter importer = AssetImporter.GetAtPath(fileName) as TextureImporter;
                    if (importer.spriteImportMode == SpriteImportMode.Multiple)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        AssetDatabase.WriteImportSettingsIfDirty(fileName);
                    }
                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    settings.spriteGenerateFallbackPhysicsShape = false;
                    importer.SetTextureSettings(settings);

                    importer.spriteImportMode = SpriteImportMode.Multiple;
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = sheet.CellWidth;

                    // Build Sprite Sheet
                    Rect[] rects = InternalSpriteUtility.GenerateGridSpriteRectangles(texture, new Vector2(0, 0), new Vector2(sheet.CellWidth, sheet.CellHeight), new Vector2(0, 0), true);

                    var metas = new List<SpriteMetaData>();
                    int rectNum = 0;

                    foreach (Rect rect in rects)
                    {
                        var meta = new SpriteMetaData();
                        meta.pivot = Vector2.zero;
                        meta.alignment = (int)SpriteAlignment.Center;
                        meta.rect = rect;
                        meta.name = characterName + "_" + rectNum++;
                        metas.Add(meta);
                    }

                    importer.spritesheet = metas.ToArray();

                    AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);

                    // Set Physics Shapes
                    UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Characters/" + characterName + "/" + characterName + ".png");

                    for (int i = 0; i < sprites.Length; i++)
                    {
                        Sprite sprite = sprites[i] as Sprite;

                        if (sprite != null)
                        {
                            try
                            {
                                int idx = Convert.ToInt32(sprite.name.Substring(sprite.name.LastIndexOf('_') + 1));
                                List<Vector2> points = new List<Vector2>();
                                if (sheet.Masks[idx].Count > 0)
                                {
                                    for (int e = 0; e < sheet.Masks[idx].Count; e++)
                                    {
                                        Vector2Int pt = sheet.MaskPoints[sheet.Masks[idx][e]];
                                        float x = (float)pt.x;
                                        if (x == (sheet.CellWidth / 2) - 1 || x == sheet.CellWidth - 1) x++;
                                        x = 0 - ((sheet.CellWidth / 2) - x);

                                        float y = (float)pt.y;
                                        if (y == (sheet.CellHeight / 2) - 1 || y == sheet.CellHeight - 1) y++;
                                        y = y + (sheet.CellHeight / 2);

                                        points.Add(new Vector2(x, sheet.CellHeight - y));
                                    }

                                    var spritePhysicsShapeImporter = new SpritePhysicsShapeImporter(importer);
                                    spritePhysicsShapeImporter.SetPhysicsShape(idx, new List<Vector2[]>() { points.ToArray() });
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError(ex.Message);
                            }
                        }
                    }

                    AssetDatabase.ImportAsset(fileName, ImportAssetOptions.ForceUpdate);

                    // Create Animated Sprite
                    GameObject character = new GameObject("New Sprite");

                    SpriteRenderer renderer = character.AddComponent<SpriteRenderer>();
                    renderer.sprite = Resources.Load<Sprite>("Characters/" + characterName + "/" + characterName + "_1");
                    renderer.drawMode = SpriteDrawMode.Simple;
                    renderer.enabled = true;

                    SpriteAnimator animator = character.AddComponent<SpriteAnimator>();
                    animator.AnimationsJson = "{'Animations':" + JsonConvert.SerializeObject(sheet.Animations) + "}";
                    animator.SpriteCount = sprites.Length - 1;
                    animator.SpriteSheetName = characterName;

                    // Save as Prefab
                    // Create Prefab
                    string localPath = "Assets/Resources/Characters/" + characterName + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(character, localPath);

                    // Delete Update file
                    //File.Delete(unityFolder + "\\" + characterName + "_update.txt.meta");
                    //File.Delete(unityFolder + "\\" + characterName + "_update.txt");
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileInfo : MonoBehaviour
{
	public static TileInfo instance;

	public Dictionary<Vector3, TileMetaData> tiles;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy(gameObject);

		tiles = new Dictionary<Vector3, TileMetaData>();
	}

	public void AddMetaData(Vector3 v, TileMetaData m)
	{
		tiles.Add(v, m);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class TileMetaData
{
    public bool IsSolid { get; set; }
    public bool IsLadder { get; set; }
    public int Damage { get; set; }
    public string Friction { get; set; }
    public string Action { get; set; }
 
    public TileMetaData()
    {
        IsSolid = false;
        IsLadder = false;
        Damage = 0;
        Friction = "H";
        Action = "";
    }
}


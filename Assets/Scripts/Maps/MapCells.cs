using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapCells 
{
    public int[,] Cells;
    public string[,] Actions;

    public MapCells(int w, int h, bool clear = false)
    {
        Cells = new int[w, h];
        Actions = new string[w, h];
        if (clear)
        {
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    Cells[x, y] = 0;
                    Actions[x, y] = "";
                }
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapCells 
{
    public int[,] Cells;

    public MapCells(int w, int h, bool clear = false)
    {
        Cells = new int[w, h];
        if (clear)
        {
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    Cells[x, y] = 0;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public class FontFix : MonoBehaviour
{
    public Font[] fonts;

    private void Start()
    {
        for (int i = 0; i < fonts.Length; i++)
            fonts[i].material.mainTexture.filterMode = FilterMode.Point;
    }
}

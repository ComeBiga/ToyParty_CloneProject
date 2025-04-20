using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HexaVector2Int
{
    public int row;
    public int column;

    public HexaVector2Int(int row, int column)
    {
        this.row = row;
        this.column = column;
    }
}

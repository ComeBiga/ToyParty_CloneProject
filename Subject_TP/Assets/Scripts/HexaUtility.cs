using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexaUtility
{
    public enum EDirection { Up, RightUp, RightDown, Down, LeftDown, LeftUp }

    //public static readonly (int, int)[] oddDelta = new (int, int)[] { 
    //                                                                    (1, 0), 
    //                                                                    (1, 1), 
    //                                                                    (0, 1), 
    //                                                                    (-1, 0), 
    //                                                                    (0, -1), 
    //                                                                    (1, -1) 
    //                                                                };

    //public static readonly (int, int)[] evenDelta = new (int, int)[] { 
    //                                                                    (1, 0), 
    //                                                                    (0, 1), 
    //                                                                    (-1, 1), 
    //                                                                    (-1, 0), 
    //                                                                    (-1, -1), 
    //                                                                    (0, -1) 
    //                                                                };

    public static readonly HexaVector2Int[] oddDelta = new HexaVector2Int[] {
                                                                                new HexaVector2Int(1, 0),
                                                                                new HexaVector2Int(1, 1),
                                                                                new HexaVector2Int(0, 1),
                                                                                new HexaVector2Int(-1, 0),
                                                                                new HexaVector2Int(0, -1),
                                                                                new HexaVector2Int(1, -1)
                                                                            };

    public static readonly HexaVector2Int[] evenDelta = new HexaVector2Int[] { 
                                                                                new HexaVector2Int(1, 0),
                                                                                new HexaVector2Int(0, 1), 
                                                                                new HexaVector2Int(-1, 1), 
                                                                                new HexaVector2Int(-1, 0), 
                                                                                new HexaVector2Int(-1, -1), 
                                                                                new HexaVector2Int(0, -1) 
                                                                            };

    public static HexaVector2Int[] GetDelta(int columnIndex)
    {
        int columnNumber = columnIndex + 1;

        return columnNumber % 2 == 0 ? evenDelta : oddDelta;
    }

    public static HexaVector2Int GetDelta(int columnIndex, EDirection direction)
    {
        HexaVector2Int[] delta = GetDelta(columnIndex);

        return delta[(int)direction];
    }
}

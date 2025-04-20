using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBlock : Block
{
    public virtual List<Block> GetTargetBlocks()
    {
        var board = HexBoardManager.Instance;
        HexaVector2Int currentCoordinates = board.GetCoordinates(index);
        HexaVector2Int nextCoordinates;
        var targetBlocks = new List<Block>(10);

        var dir1 = new HexaUtility.EDirection[] { HexaUtility.EDirection.Up, HexaUtility.EDirection.LeftUp, HexaUtility.EDirection.LeftDown };
        var dir2 = new HexaUtility.EDirection[] { HexaUtility.EDirection.Down, HexaUtility.EDirection.RightDown, HexaUtility.EDirection.RightUp };
        int randomIndex = Random.Range(0, dir1.Length);

        // dir1 ¹æÇâ Å½»ö
        while (true)
        {
            HexaVector2Int delta = HexaUtility.GetDelta(currentCoordinates.column, dir1[randomIndex]);
            nextCoordinates = new HexaVector2Int(currentCoordinates.row + delta.row, currentCoordinates.column + delta.column);

            if (!board.IsInRange(nextCoordinates.row, nextCoordinates.column))
            {
                break;
            }

            if (!board.IsEnableCell(nextCoordinates.row, nextCoordinates.column))
            {
                break;
            }

            Block nextBlock = board.GetBlock(nextCoordinates.row, nextCoordinates.column);

            if (nextBlock == null)
            {
                break;
            }

            targetBlocks.Add(nextBlock);

            currentCoordinates = nextCoordinates;
        }

        // dir2 ¹æÇâ Å½»ö
        currentCoordinates = board.GetCoordinates(index);

        while (true)
        {
            HexaVector2Int delta = HexaUtility.GetDelta(currentCoordinates.column, dir2[randomIndex]);
            nextCoordinates = new HexaVector2Int(currentCoordinates.row + delta.row, currentCoordinates.column + delta.column);

            if (!board.IsInRange(nextCoordinates.row, nextCoordinates.column))
            {
                break;
            }

            if (!board.IsEnableCell(nextCoordinates.row, nextCoordinates.column))
            {
                break;
            }

            Block nextBlock = board.GetBlock(nextCoordinates.row, nextCoordinates.column);

            if (nextBlock == null)
            {
                break;
            }

            targetBlocks.Add(nextBlock);

            currentCoordinates = nextCoordinates;
        }

        return targetBlocks;
    }
}

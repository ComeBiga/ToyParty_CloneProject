using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightMatchCheck : MatchCheck
{
    [SerializeField]
    private int _minMatchCount = 3;
    [SerializeField]
    private int _minItemMatchCount = 4;
    [SerializeField]
    private Block _prefItemBlock;

    public override bool Check(Block srcblock, out List<Block> matchableBlocks, List<ItemInfo> itemInfos)
    {
        var board = HexBoardManager.Instance;
        matchableBlocks = new List<Block>(10);

        // ÁÂ»ó -> ¿ìÇÏ
        List<Block> LuRdBlocks = GetMatchableBlocks(srcblock, HexaUtility.EDirection.LeftUp, HexaUtility.EDirection.RightDown);
        // ÁÂÇÏ -> ¿ì»ó
        List<Block> LdRuBlocks = GetMatchableBlocks(srcblock, HexaUtility.EDirection.LeftDown, HexaUtility.EDirection.RightUp);
        // »ó -> ÇÏ
        List<Block> UDBlocks = GetMatchableBlocks(srcblock, HexaUtility.EDirection.Up, HexaUtility.EDirection.Down);

        if(LuRdBlocks.Count >= _minMatchCount)
        {
            matchableBlocks.AddRange(LuRdBlocks);
        }

        if(LdRuBlocks.Count >= _minMatchCount)
        {
            matchableBlocks.AddRange(LdRuBlocks);
        }

        if(UDBlocks.Count >= _minMatchCount)
        {
            matchableBlocks.AddRange(UDBlocks);
        }

        if(LuRdBlocks.Count >= _minItemMatchCount || LdRuBlocks.Count >= _minItemMatchCount || UDBlocks.Count >= _minItemMatchCount)
        {
            var itemInfo = new ItemInfo();
            itemInfo.srcBlock = srcblock;
            itemInfo.prefItemBlock = _prefItemBlock;
            itemInfo.matchableBlocks = matchableBlocks;
            itemInfos.Add(itemInfo);
        }

        return LuRdBlocks.Count >= _minMatchCount 
                || LdRuBlocks.Count >= _minMatchCount 
                || UDBlocks.Count >= _minMatchCount;
    }

    public static List<Block> GetMatchableBlocks(Block srcBlock, HexaUtility.EDirection dir1, HexaUtility.EDirection dir2)
    {
        var board = HexBoardManager.Instance;
        HexaVector2Int currentCoordinates = board.GetCoordinates(srcBlock.index);
        HexaVector2Int nextCoordinates;
        var matchableBlocks = new List<Block>(10);

        // Ã¹¹øÂ° ºí·° Å½»ö
        while (true)
        {
            HexaVector2Int delta = HexaUtility.GetDelta(currentCoordinates.column, dir1);
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

            if(nextBlock == null)
            {
                break;
            }

            if (!nextBlock.IsMatchable(srcBlock))
            {
                break;
            }

            currentCoordinates = nextCoordinates;
        }

        // ¸ÅÄª ºí·° Å½»ö
        while (true)
        {
            matchableBlocks.Add(board.GetBlock(currentCoordinates.row, currentCoordinates.column));

            HexaVector2Int delta = HexaUtility.GetDelta(currentCoordinates.column, dir2);
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

            if(nextBlock == null)
            {
                break;
            }

            if (!nextBlock.IsMatchable(srcBlock))
            {
                break;
            }

            currentCoordinates = nextCoordinates;
        }

        return matchableBlocks;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MatchCheck : MonoBehaviour
{
    public struct ItemInfo
    {
        public Block srcBlock;
        public Block prefItemBlock;
        public List<Block> matchableBlocks;
        //public HexaVector2Int coordinates;
        //public Block.EColor colorType;
    }

    public abstract bool Check(Block block, out List<Block> matchableBlocks, List<ItemInfo> itemInfos);

    public bool Check(Block srcBlock, Player.PopInfo popInfo)
    {
        bool bMatched = Check(srcBlock, out List<Block> matchableBlocks, popInfo.createdItemInfos);

        foreach(Block block in matchableBlocks)
        {
            popInfo.matchableBlocksSet.Add(block);
            popInfo.destoryBlocksSet.Add(block);
        }

        return bMatched;
    }
}

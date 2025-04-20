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
}

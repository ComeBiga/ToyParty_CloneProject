using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopInfo : MonoBehaviour
{
    public enum EState { Swap, Drop }

    [SerializeField]
    private MatchCheck[] _matchChecks;

    public HashSet<Block> matchableBlocksSet = new HashSet<Block>();
    public List<MatchCheck.ItemInfo> createdItemInfos = new List<MatchCheck.ItemInfo>();
    public HashSet<Block> destoryBlocksSet = new HashSet<Block>();
    public EState state;
    public Block srcBlock;
    public Block dstBlock;

    public void Reset()
    {
        matchableBlocksSet.Clear();
        createdItemInfos.Clear();
        destoryBlocksSet.Clear();
        srcBlock = null;
        dstBlock = null;
    }

    public void DestroyBlocks()
    {
        var board = HexBoardManager.Instance;
        board.DestroyBlocks(destoryBlocksSet);
    }

    public bool CheckMatch(Block srcBlock)
    {
        bool bMatched = false;

        if (!srcBlock.isMatchable)
        {
            return bMatched;
        }

        foreach (var matchCheck in _matchChecks)
        {
            if (matchCheck.Check(srcBlock, out List<Block> matchableBlocks, createdItemInfos))
            {
                foreach (Block block in matchableBlocks)
                {
                    matchableBlocksSet.Add(block);
                    destoryBlocksSet.Add(block);
                }

                bMatched = true;
            }
        }

        return bMatched;
    }

    public bool CheckMatchAll()
    {
        var board = HexBoardManager.Instance;
        List<Block> blocks = board.Blocks;
        bool bMatched = false;

        foreach (Block block in blocks)
        {
            if (CheckMatch(block))
            {
                bMatched = true;
            }
        }

        return bMatched;
    }

    public IEnumerator UseItemBlock()
    {
        bool bUsed = false;

        foreach (Block matchableBlock in matchableBlocksSet)
        {
            var itemBlock = matchableBlock as ItemBlock;

            if (itemBlock != null)
            {
                // 아이템 발동
                List<Block> targetBlocks = itemBlock.GetTargetBlocks();

                foreach (Block targetBlock in targetBlocks)
                {
                    destoryBlocksSet.Add(targetBlock);
                    targetBlock.AnimateLight();
                }

                bUsed = true;
                yield return new WaitForSeconds(.1f);
            }
        }

        // return bUsed;
    }

    public IEnumerator CreateItemBlock()
    {
        var board = HexBoardManager.Instance;
        var searchedBlocks = new HashSet<Block>();

        foreach (MatchCheck.ItemInfo itemInfo in createdItemInfos)
        {
            bool bSearchConflict = false;

            foreach (Block matchableBlock in itemInfo.matchableBlocks)
            {
                if (!searchedBlocks.Add(matchableBlock))
                {
                    bSearchConflict = true;
                    break;
                }
            }

            if (!bSearchConflict)
            {
                float timer = 0f;
                float duration = .2f;
                HexaVector2Int dstCoordinates = board.GetCoordinates(itemInfo.srcBlock.index);
                Cell dstCell = board.GetCell(dstCoordinates.row, dstCoordinates.column);

                while (timer < duration)
                {
                    foreach (Block matchableBlock in itemInfo.matchableBlocks)
                    {
                        HexaVector2Int coordinates = board.GetCoordinates(matchableBlock.index);
                        Cell cell = board.GetCell(coordinates.row, coordinates.column);
                        matchableBlock.transform.position = Vector3.Lerp(cell.transform.position, dstCell.transform.position, timer / duration);
                    }

                    timer += Time.deltaTime;
                    yield return null;
                }

                foreach (Block matchableBlock in itemInfo.matchableBlocks)
                {
                    destoryBlocksSet.Remove(matchableBlock);
                    board.DestroyBlock(matchableBlock);
                }

                Block itemBlock = board.CreateBlock(itemInfo.prefItemBlock, itemInfo.srcBlock.index);
                itemBlock.SetColor(itemInfo.srcBlock.colorType);
            }
        }
    }

    public void CheckBreakableBlock()
    {
        var board = HexBoardManager.Instance;
        var matchableBlocks = new List<Block>(matchableBlocksSet);
        var decreasedBlocks = new HashSet<Block>(10);

        foreach (Block block in matchableBlocks)
        {
            HexaVector2Int blockCoordinates = board.GetCoordinates(block.index);

            foreach (HexaUtility.EDirection dir in HexaUtility.directions)
            {
                HexaVector2Int delta = HexaUtility.GetDelta(blockCoordinates.column, dir);
                HexaVector2Int nextCoordinates = new HexaVector2Int(blockCoordinates.row + delta.row, blockCoordinates.column + delta.column);

                if (!board.IsInRange(nextCoordinates))
                {
                    continue;
                }

                if (!board.IsEnableCell(nextCoordinates))
                {
                    continue;
                }

                Block nextBlock = board.GetBlock(nextCoordinates.row, nextCoordinates.column);

                if (nextBlock != null)
                {
                    var breakableBlock = nextBlock as BreakableBlock;

                    if (breakableBlock != null)
                    {
                        if (!decreasedBlocks.Contains(breakableBlock))
                        {
                            breakableBlock.DecreaseHP(1);

                            if (breakableBlock.hp <= 0)
                            {
                                destoryBlocksSet.Add(breakableBlock);
                            }

                            decreasedBlocks.Add(breakableBlock);
                        }
                    }
                }
            }
        }
    }
}

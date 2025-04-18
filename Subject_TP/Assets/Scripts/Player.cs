using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public class PopInfo
    {
        public enum EState { Swap, Drop }

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
    }

    [SerializeField]
    private MatchCheck[] _matchChecks;
    [SerializeField]
    private float _swapDuration = .2f;
    [SerializeField]
    private float _dropDuration = .5f;

    private Block mSrcBlock = null;
    private Block mDstBlock = null;

    private bool mbPopping = false;

    private void Start()
    {
        StartCoroutine(eInputRoutine());
    }

    private IEnumerator eInputRoutine()
    {
        while(true)
        {
            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mSrcBlock = raycastBlock();
                }

                if(mSrcBlock == null)
                {
                    yield return null;
                    continue;
                }

                Block block = raycastBlock();

                if(block == null || block == mSrcBlock)
                {
                    yield return null;
                    continue;
                }

                mDstBlock = block;

                var board = HexBoardManager.Instance;

                board.SwapBlock(mSrcBlock, mDstBlock);
                yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);

                var popInfo = new PopInfo();
                popInfo.state = PopInfo.EState.Swap;
                // var matchableBlocksSet = new HashSet<Block>(board.Blocks.Count);
                // var itemInfos = new List<MatchCheck.ItemInfo>();
                //bool bSrcPopping = checkMatch(mSrcBlock, matchableBlocksSet, itemInfos);
                //bool bDstPopping = checkMatch(mDstBlock, matchableBlocksSet, itemInfos);
                bool bSrcPopping = checkMatch(mSrcBlock, popInfo);
                bool bDstPopping = checkMatch(mDstBlock, popInfo);

                useItemBlock(popInfo);

                // createItemBlock(itemInfos);
                createItemBlock(popInfo);
                // itemInfos.Clear();

                //checkBreakableBlock(matchableBlocksSet);
                checkBreakableBlock(popInfo);

                //board.DestroyBlocks(matchableBlocksSet);
                board.DestroyBlocks(popInfo.destoryBlocksSet);

                popInfo.Reset();

                mbPopping = bSrcPopping || bDstPopping;

                if(mbPopping)
                {
                    popInfo.state = PopInfo.EState.Drop;

                    int breaker = 0;
                    while (true)
                    {
                        // if(Input.GetKeyDown(KeyCode.Space))
                        {
                            bool bSpawned = spawnBlock();
                            bool bDropped = dropBlocks();

                            if (!bDropped && !bSpawned)
                            {
                                // matchableBlocksSet.Clear();
                                // bool bMatched = checkMatchAll(matchableBlocksSet, itemInfos);
                                bool bMatched = checkMatchAll(popInfo);

                                useItemBlock(popInfo);

                                //createItemBlock(itemInfos);
                                createItemBlock(popInfo);
                                //itemInfos.Clear();

                                //checkBreakableBlock(matchableBlocksSet);
                                checkBreakableBlock(popInfo);

                                //board.DestroyBlocks(matchableBlocksSet);
                                board.DestroyBlocks(popInfo.destoryBlocksSet);

                                popInfo.Reset();

                                if (!bMatched)
                                {
                                    break;
                                }
                            }

                            if (++breaker > 100)
                            {
                                Debug.LogError("Drop Loop Break");
                                break;
                            }
                        }

                        yield return new WaitForSeconds(_dropDuration + .01f);
                    }

                    mbPopping = false;
                }
                else
                {
                    board.SwapBlock(mSrcBlock, mDstBlock);
                    yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);
                }

                mSrcBlock = null;
                mDstBlock = null;
            }

            yield return null;
        }
    }

    private Block raycastBlock()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Block"))
        {
            return hit.collider.GetComponent<Block>();
        }
        else
        {
            return null;
        }
    }

    private IEnumerator eAnimateBlockSwap(Block srcBlock, Block dstBlock)
    {
        var board = HexBoardManager.Instance;
        Vector3 srcBlockPosition = srcBlock.transform.position;
        Vector3 dstBlockPosition = dstBlock.transform.position;

        Vector3 dirToDst = (dstBlockPosition - srcBlockPosition).normalized;
        Vector3 dirToScr = (srcBlockPosition - dstBlockPosition).normalized;

        float duration = _swapDuration;
        float timer = 0f;

        while(timer < duration)
        {
            Vector3 srcBlockCurrentPosition = Vector3.Lerp(srcBlockPosition, dstBlockPosition, timer / duration);
            srcBlock.transform.position = srcBlockCurrentPosition;

            Vector3 dstBlockCurrentPosition = Vector3.Lerp(dstBlockPosition, srcBlockPosition, timer / duration);
            dstBlock.transform.position = dstBlockCurrentPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        srcBlock.transform.position = dstBlockPosition;
        dstBlock.transform.position = srcBlockPosition;
    }

    // private bool checkMatch(Block srcBlock, HashSet<Block> matchableBlocksSet, List<MatchCheck.ItemInfo> itemInfos)
    private bool checkMatch(Block srcBlock, PopInfo popInfo)
    {
        bool bMatched = false;

        if (!srcBlock.isMatchable)
        {
            return bMatched;
        }

        foreach (var matchCheck in _matchChecks)
        {
            // if (matchCheck.Check(srcBlock, out List<Block> matchableBlocks, itemInfos))
            if (matchCheck.Check(srcBlock, popInfo))
            {
                //foreach(Block matchableBlock in matchableBlocks)
                //{
                //    matchableBlocksSet.Add(matchableBlock);
                //}

                bMatched = true;
            }
        }

        return bMatched;
    }

    // private bool checkMatchAll(HashSet<Block> matchableBlocksSet, List<MatchCheck.ItemInfo> itemInfos)
    private bool checkMatchAll(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        List<Block> blocks = board.Blocks;
        bool bMatched = false;

        foreach (Block block in blocks)
        {
            // if(checkMatch(block, matchableBlocksSet, itemInfos))
            if(checkMatch(block, popInfo))
            {
                bMatched = true;
            }
        }

        return bMatched;
    }

    //private void checkBreakableBlock(HashSet<Block> matchableBlocksSet)
    private void checkBreakableBlock(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        //var matchableBlocks = new List<Block>(matchableBlocksSet);
        var matchableBlocks = new List<Block>(popInfo.matchableBlocksSet);
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

                if(nextBlock != null)
                {
                    var breakableBlock = nextBlock as BreakableBlock;

                    if(breakableBlock != null)
                    {
                        if(!decreasedBlocks.Contains(breakableBlock))
                        { 
                            breakableBlock.DecreaseHP(1);

                            if(breakableBlock.hp <= 0)
                            {
                                //matchableBlocksSet.Add(breakableBlock);
                                popInfo.destoryBlocksSet.Add(breakableBlock);
                            }

                            decreasedBlocks.Add(breakableBlock);
                        }
                    }
                }
            }
        }
    }

    //private void createItemBlock(List<MatchCheck.ItemInfo> itemInfos)
    private void createItemBlock(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        var searchedBlocks = new HashSet<Block>();

        //foreach (MatchCheck.ItemInfo itemInfo in itemInfos)
        foreach (MatchCheck.ItemInfo itemInfo in popInfo.createdItemInfos)
        {
            bool bSearchConflict = false;

            foreach(Block matchableBlock in itemInfo.matchableBlocks)
            {
                if(!searchedBlocks.Add(matchableBlock))
                {
                    bSearchConflict = true;
                    break;
                }
            }

            if(!bSearchConflict)
            {
                Block itemBlock = board.CreateBlock(itemInfo.prefItemBlock, itemInfo.srcBlock.index);
                itemBlock.SetColor(itemInfo.srcBlock.colorType);
            }
        }
    }

    private bool useItemBlock(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        bool bUsed = false;

        foreach(Block matchableBlock in popInfo.matchableBlocksSet)
        {
            var itemBlock = matchableBlock as ItemBlock;

            if (itemBlock != null)
            {
                // 아이템 발동
                List<Block> targetBlocks = itemBlock.GetTargetBlocks();

                foreach (Block targetBlock in targetBlocks)
                {
                    popInfo.destoryBlocksSet.Add(targetBlock);
                }

                bUsed = true;
            }
        }

        return bUsed;
    }

    private bool dropBlocks()
    {
        var board = HexBoardManager.Instance;

        List<Block> blocks = board.Blocks;
        bool bDropped = false;
        var dirs = new HexaUtility.EDirection[] { 
                                                    HexaUtility.EDirection.Down, 
                                                    HexaUtility.EDirection.LeftDown,
                                                    HexaUtility.EDirection.RightDown
                                                };

        for (int i = 0; i < blocks.Count; ++i)
        {
            Block block = blocks[i];

            if (block == null)
            {
                continue;
            }

            HexaVector2Int blockCoordinates = board.GetCoordinates(block.index);

            foreach(HexaUtility.EDirection dir in dirs)
            {
                HexaVector2Int downDelta = HexaUtility.GetDelta(blockCoordinates.column, dir);
                HexaVector2Int downCoordinates = new HexaVector2Int(blockCoordinates.row + downDelta.row, blockCoordinates.column + downDelta.column);

                if(!board.IsInRange(downCoordinates))
                {
                    continue;
                }

                if(!board.IsEnableCell(downCoordinates))
                {
                    continue;
                }

                Block downBlock = board.GetBlock(downCoordinates.row, downCoordinates.column);

                if(downBlock == null)
                {
                    if(dir == HexaUtility.EDirection.LeftDown || dir == HexaUtility.EDirection.RightDown)
                    {
                        HexaVector2Int upCoordinates = new HexaVector2Int(downCoordinates.row, downCoordinates.column);
                        bool bFallingBlock = false;

                        while(true)
                        {
                            upCoordinates = new HexaVector2Int(upCoordinates.row + 1, upCoordinates.column);

                            if (!board.IsInRange(upCoordinates))
                            {
                                break;
                            }

                            if (!board.IsEnableCell(upCoordinates))
                            {
                                break;
                            }

                            Block upBlock = board.GetBlock(upCoordinates.row, upCoordinates.column);

                            if(upBlock != null)
                            {
                                bFallingBlock = true;
                                break;
                            }
                        }

                        if(bFallingBlock)
                        {
                            continue;
                        }
                    }

                    board.SetBlockIndex(downCoordinates, block);
                    StartCoroutine(eDropBlock(block, downCoordinates));

                    bDropped = true;

                    break;
                }
            }
        }

        return bDropped;
    }

    private IEnumerator eDropBlock(Block srcBlock, HexaVector2Int toCoordinates)
    {
        var board = HexBoardManager.Instance;
        Vector3 fromPosition = srcBlock.transform.position;
        Vector3 toPosition = board.GetWorldPosition(toCoordinates);
        Vector3 dir = (toPosition - fromPosition).normalized;

        float duration = _dropDuration;
        float timer = 0f;

        while(timer < duration)
        {
            Vector3 currentPosition = Vector3.Lerp(fromPosition, toPosition, timer / duration);
            srcBlock.transform.position = currentPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        board.SetBlockWorldPosition(toCoordinates.row, toCoordinates.column, srcBlock);
    }

    private bool spawnBlock()
    {
        var board = HexBoardManager.Instance;

        Cell cell = board.GetSpawnCell();
        HexaVector2Int spawnCoordinates = board.GetCellCoordinates(cell);
        HexaVector2Int delta = HexaUtility.GetDelta(spawnCoordinates.column, HexaUtility.EDirection.Down);
        HexaVector2Int downCoordinates = new HexaVector2Int(spawnCoordinates.row + delta.row, spawnCoordinates.column + delta.column);

        if(!board.IsInRange(downCoordinates))
        {
            return false;
        }

        if(!board.IsEnableCell(downCoordinates))
        {
            return false;
        }

        Block downBlock = board.GetBlock(downCoordinates.row, downCoordinates.column);

        if(downBlock == null)
        {
            board.SpawnBlock();
            return true;
        }

        return false;
    }
}

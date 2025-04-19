using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance => instance;
    private static StageManager instance = null;

    [SerializeField]
    private PopInfo _popInfo;
    [SerializeField]
    private Block _prefBlock;
    [SerializeField]
    private BreakableBlock _prefBreakableBlock;
    [SerializeField]
    private float _swapDuration;
    [SerializeField]
    private float _dropDuration;

    private HexBoardManager mBoard;

    public Block SpawnBlock()
    {
        Cell spawnCell = mBoard.GetSpawnCell();
        int spawnCellIndex = mBoard.GetCellIndex(spawnCell);

        Block spawnedBlock = mBoard.CreateBlock(_prefBlock, spawnCellIndex);
        spawnedBlock.SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));

        return spawnedBlock;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mBoard = HexBoardManager.Instance;

        // createBlock(_prefBreakableBlock, GetIndex(0, 2));
        // CreateBlock(_prefBreakableBlock, GetIndex(2, 3));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(1, 1));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(0, 2));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(0, 3));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(0, 4));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(1, 5));

        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(2, 3));

        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(4, 1));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(4, 2));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(4, 4));
        mBoard.CreateBlock(_prefBreakableBlock, mBoard.GetIndex(4, 5));
        
        for (int i = 0; i < mBoard.Cells.Length; ++i)
        {
            Cell cell = mBoard.Cells[i];

            if (!cell.enable)
            {
                continue;
            }

            if (mBoard.GetBlock(mBoard.GetCoordinates(i)) == null)
            {
                mBoard.CreateBlock(_prefBlock, i).SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));
            }
        }

        cleanUpBoard(_popInfo);

        StartCoroutine(eInputRoutine());
    }

    /// <summary>
    /// 스테이지 시작 전 매칭되는 블럭이 없도록 정리
    /// </summary>
    /// <param name="popInfo"></param>
    private void cleanUpBoard(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        int breaker = 0;
        while (true)
        {
            // if(Input.GetKeyDown(KeyCode.Space))
            {
                bool bSpawned = spawnBlock();
                bool bDropped = dropBlocks(0f);

                if (!bDropped && !bSpawned)
                {
                    bool bMatched = popInfo.CheckMatchAll();
                    popInfo.CheckBreakableBlock();

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
        }
    }

    private IEnumerator eInputRoutine()
    {
        Block mSrcBlock = null;
        Block mDstBlock = null;

        while (true)
        {
            if (Input.GetMouseButton(0))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mSrcBlock = raycastBlock();
                }

                if (mSrcBlock == null)
                {
                    yield return null;
                    continue;
                }

                Block block = raycastBlock();

                if (block == null || block == mSrcBlock)
                {
                    yield return null;
                    continue;
                }

                mDstBlock = block;

                var board = HexBoardManager.Instance;

                board.SwapBlock(mSrcBlock, mDstBlock);
                yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);

                _popInfo.Reset();
                _popInfo.state = PopInfo.EState.Swap;

                bool bSrcMatched = _popInfo.CheckMatch(mSrcBlock);
                bool bDstMatched = _popInfo.CheckMatch(mDstBlock);

                bool bSwapMatched = bSrcMatched || bDstMatched;

                if (bSwapMatched)
                {
                    _popInfo.UseItemBlock();
                    _popInfo.CreateItemBlock();
                    _popInfo.CheckBreakableBlock();

                    yield return eDestroyBlocks(_popInfo);

                    _popInfo.Reset();
                    _popInfo.state = PopInfo.EState.Drop;

                    int breaker = 0;
                    while (true)
                    {
                        // if(Input.GetKeyDown(KeyCode.Space))
                        {
                            bool bSpawned = spawnBlock();
                            bool bDropped = dropBlocks(_dropDuration);

                            yield return new WaitForSeconds(_dropDuration + .01f);

                            if (!bDropped && !bSpawned)
                            {
                                _popInfo.Reset();

                                bool bMatched = _popInfo.CheckMatchAll();

                                _popInfo.UseItemBlock();
                                _popInfo.CreateItemBlock();
                                _popInfo.CheckBreakableBlock();

                                yield return eDestroyBlocks(_popInfo);

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

                    }

                    bSwapMatched = false;
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

        //Vector3 dirToDst = (dstBlockPosition - srcBlockPosition).normalized;
        //Vector3 dirToScr = (srcBlockPosition - dstBlockPosition).normalized;

        float duration = _swapDuration;
        float timer = 0f;

        while (timer < duration)
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

    private IEnumerator eDestroyBlocks(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        bool bDestroy = false;

        foreach (Block destroyBlock in popInfo.destoryBlocksSet)
        {
            destroyBlock.AnimateDestroy();
            bDestroy = true;
        }

        if (bDestroy)
        {
            yield return new WaitForSeconds(.4f);
        }

        board.DestroyBlocks(popInfo.destoryBlocksSet);
    }

    private bool spawnBlock()
    {
        var board = HexBoardManager.Instance;

        Cell spawnCell = board.GetSpawnCell();
        HexaVector2Int spawnCoordinates = board.GetCellCoordinates(spawnCell);
        HexaVector2Int delta = HexaUtility.GetDelta(spawnCoordinates.column, HexaUtility.EDirection.Down);
        HexaVector2Int downCoordinates = new HexaVector2Int(spawnCoordinates.row + delta.row, spawnCoordinates.column + delta.column);

        if (!board.IsInRange(downCoordinates))
        {
            return false;
        }

        if (!board.IsEnableCell(downCoordinates))
        {
            return false;
        }

        Block downBlock = board.GetBlock(downCoordinates.row, downCoordinates.column);

        if (downBlock == null)
        {
            int spawnCellIndex = board.GetCellIndex(spawnCell);
            Block spawnedBlock = board.CreateBlock(_prefBlock, spawnCellIndex);
            spawnedBlock.SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));

            return true;
        }

        return false;
    }


    private bool dropBlocks(float duration)
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

            foreach (HexaUtility.EDirection dir in dirs)
            {
                HexaVector2Int downDelta = HexaUtility.GetDelta(blockCoordinates.column, dir);
                HexaVector2Int downCoordinates = new HexaVector2Int(blockCoordinates.row + downDelta.row, blockCoordinates.column + downDelta.column);

                if (!board.IsInRange(downCoordinates))
                {
                    continue;
                }

                if (!board.IsEnableCell(downCoordinates))
                {
                    continue;
                }

                Block downBlock = board.GetBlock(downCoordinates.row, downCoordinates.column);

                if (downBlock == null)
                {
                    if (dir == HexaUtility.EDirection.LeftDown || dir == HexaUtility.EDirection.RightDown)
                    {
                        HexaVector2Int upCoordinates = new HexaVector2Int(downCoordinates.row, downCoordinates.column);
                        bool bFallingBlock = false;

                        while (true)
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

                            if (upBlock != null)
                            {
                                bFallingBlock = true;
                                break;
                            }
                        }

                        if (bFallingBlock)
                        {
                            continue;
                        }
                    }

                    board.SetBlockIndex(downCoordinates, block);

                    StartCoroutine(eAnimateDropBlock(block, downCoordinates, duration));

                    bDropped = true;

                    break;
                }
            }
        }

        return bDropped;
    }

    private IEnumerator eAnimateDropBlock(Block srcBlock, HexaVector2Int toCoordinates, float duration)
    {
        var board = HexBoardManager.Instance;
        Vector3 fromPosition = srcBlock.transform.position;
        Vector3 toPosition = board.GetWorldPosition(toCoordinates);

        // float duration = _dropDuration;
        float timer = 0f;

        while (timer < duration)
        {
            Vector3 currentPosition = Vector3.Lerp(fromPosition, toPosition, timer / duration);
            srcBlock.transform.position = currentPosition;

            timer += Time.deltaTime;
            yield return null;
        }

        board.SetBlockWorldPosition(toCoordinates.row, toCoordinates.column, srcBlock);
    }
}

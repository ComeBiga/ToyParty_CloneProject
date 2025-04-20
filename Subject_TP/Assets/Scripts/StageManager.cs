using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance => instance;
    private static StageManager instance = null;

    [SerializeField]
    private List<StageData> _stageDatas;
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
    [SerializeField]
    private int _moveCount = 20;
    [SerializeField]
    private int _goalCount = 10;

    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI _txtMoveCount;
    [SerializeField]
    private TextMeshProUGUI _txtGoalCount;
    [SerializeField]
    private Button _btnPause;
    [SerializeField]
    private GameObject _goPanelClear;

    private HexBoardManager mBoard;
    private StageData mCurrentStageData;
    private Coroutine mStageRoutine = null;
    private int mRemainMoveCount;
    private int mRemainGoalCount;

    /// <summary>
    /// 스테이지 로드
    /// </summary>
    /// <param name="stageID"></param>
    public void LoadStage(int stageID)
    {
        StageData stageData = _stageDatas.Find(x => x.stageID == stageID);

        LoadStage(stageData);
    }

    /// <summary>
    /// 스테이지 로드
    /// </summary>
    /// <param name="stageData"></param>
    public void LoadStage(StageData stageData)
    {
        mCurrentStageData = stageData;

        if (mCurrentStageData.startRandomBlocks)
        {
            placeRandomBlocks();
            cleanUpBoard(_popInfo);
        }
        else
        {
            placeBlocks(mCurrentStageData);
        }

        // 골 블럭 수
        if (!mCurrentStageData.useGoal)
        {
            mRemainGoalCount = int.MaxValue;
            _txtGoalCount.text = $"?";
        }

        // 이동 횟수
        if (mCurrentStageData.useMoveCount)
        {
            mRemainMoveCount = _moveCount;
            _txtMoveCount.text = $"{mRemainMoveCount}";
        }
        else
        {
            mRemainMoveCount = int.MaxValue;
            _txtMoveCount.text = $"?";
        }

        // 스테이지 시작
        mStageRoutine = StartCoroutine(eStageRoutine());
    }

    /// <summary>
    /// 스테이지 리로드
    /// </summary>
    public void ReloadStage()
    {
        StopCoroutine(mStageRoutine);
        mStageRoutine = null;

        for (int i = mBoard.Blocks.Count - 1; i >= 0; --i)
        {
            mBoard.DestroyBlock(mBoard.Blocks[i]);
        }

        _btnPause.interactable = true;

        LoadStage(mCurrentStageData);
    }

    /// <summary>
    /// 스테이지 중단
    /// </summary>
    public void StopStage()
    {
        StopCoroutine(mStageRoutine);
        mStageRoutine = null;

        for (int i = mBoard.Blocks.Count - 1; i >= 0; --i)
        {
            mBoard.DestroyBlock(mBoard.Blocks[i]);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mBoard = HexBoardManager.Instance;
    }

    /// <summary>
    /// 블럭 배치
    /// </summary>
    /// <param name="stageData"></param>
    private void placeBlocks(StageData stageData)
    {
        StageData.BlockInfo[] blockInfos = stageData.blockInfos;
        mRemainGoalCount = 0;

        for (int i = 0; i < mBoard.Cells.Length; ++i)
        {
            Cell cell = mBoard.Cells[i];

            if (!cell.enable)
            {
                continue;
            }

            StageData.BlockInfo blockInfo = blockInfos[i];

            if (blockInfo.isGoal)
            {
                mBoard.CreateBlock(_prefBreakableBlock, i);
                ++mRemainGoalCount;
                _txtGoalCount.text = $"{mRemainGoalCount}";
            }
            else
            {
                mBoard.CreateBlock(_prefBlock, i).SetColor(blockInfo.color);
            }
        }
    }

    /// <summary>
    /// 블럭 랜덤 배치
    /// </summary>
    private void placeRandomBlocks()
    {
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
    }

    /// <summary>
    /// 스테이지 시작 전 매칭되는 블럭이 없도록 정리
    /// </summary>
    /// <param name="popInfo"></param>
    private void cleanUpBoard(PopInfo popInfo)
    {
        var board = HexBoardManager.Instance;
        int breaker = 0;
        popInfo.Reset();

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

    /// <summary>
    /// 스테이지 메인 루틴(루프)
    /// </summary>
    /// <returns></returns>
    private IEnumerator eStageRoutine()
    {
        bool isStageEnd = false;
        Block mSrcBlock = null;
        Block mDstBlock = null;

        while (!isStageEnd)
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

                _btnPause.interactable = false;
                mDstBlock = block;

                var board = HexBoardManager.Instance;

                // 블럭 스왑
                board.SwapBlock(mSrcBlock, mDstBlock);
                yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);

                _popInfo.Reset();
                _popInfo.state = PopInfo.EState.Swap;

                bool bSrcMatched = _popInfo.CheckMatch(mSrcBlock);
                bool bDstMatched = _popInfo.CheckMatch(mDstBlock);

                bool bSwapMatched = bSrcMatched || bDstMatched;

                // 블럭 매칭 시
                if (bSwapMatched)
                {
                    if (mCurrentStageData.useMoveCount)
                    {
                        _txtMoveCount.text = $"{--mRemainMoveCount}";
                    }

                    yield return _popInfo.UseItemBlock();
                    yield return _popInfo.CreateItemBlock();
                    _popInfo.CheckBreakableBlock();

                    yield return eDestroyBlocks(_popInfo);

                    _popInfo.Reset();
                    _popInfo.state = PopInfo.EState.Drop;

                    // int breaker = 0;
                    // 블럭 스폰 및 드롭
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

                                yield return _popInfo.UseItemBlock();
                                yield return _popInfo.CreateItemBlock();
                                _popInfo.CheckBreakableBlock();

                                yield return eDestroyBlocks(_popInfo);

                                if (!bMatched)
                                {
                                    _btnPause.interactable = true;

                                    break;
                                }
                            }

                            //if (++breaker > 100)
                            //{
                            //    Debug.LogError("Drop Loop Break");
                            //    break;
                            //}
                        }

                    }

                    bSwapMatched = false;
                }
                // 블럭 비매칭 시 재스왑
                else
                {
                    board.SwapBlock(mSrcBlock, mDstBlock);
                    yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);

                    _btnPause.interactable = true;
                }

                mSrcBlock = null;
                mDstBlock = null;
            }

            if(mRemainGoalCount <= 0 || mRemainMoveCount <= 0)
            {
                isStageEnd = true;
            }

            yield return null;
        }

        if(mRemainGoalCount <= 0)
        {
            _goPanelClear.gameObject.SetActive(true);
        }
        else if(mRemainMoveCount <= 0)
        {
            _goPanelClear.gameObject.SetActive(true);
        }

        _btnPause.interactable = false;
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

            if(destroyBlock is BreakableBlock)
            {
                _txtGoalCount.text = $"{--mRemainGoalCount}";
            }
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

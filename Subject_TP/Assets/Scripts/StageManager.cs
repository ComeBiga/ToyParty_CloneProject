using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance => instance;
    private static StageManager instance = null;

    public enum EStageMode { Level21, Training }

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
    private TextMeshProUGUI _txtLevelClear;
    [SerializeField]
    private TextMeshProUGUI _txtLevelFail;
    [SerializeField]
    private Button _btnPause;
    [SerializeField]
    private Button _btnResume;
    [SerializeField]
    private Button _btnPauseRetry;
    [SerializeField]
    private Button _btnClearRetry;
    [SerializeField]
    private GameObject _goPanelPause;
    [SerializeField]
    private GameObject _goPanelClear;

    private HexBoardManager mBoard;
    private EStageMode mStageMode;
    private Coroutine mInputRoutine = null;
    private int mRemainMoveCount;
    private int mRemainGoalCount;
    private bool mbUseMoveCount = true;

    public Block SpawnBlock()
    {
        Cell spawnCell = mBoard.GetSpawnCell();
        int spawnCellIndex = mBoard.GetCellIndex(spawnCell);

        Block spawnedBlock = mBoard.CreateBlock(_prefBlock, spawnCellIndex);
        spawnedBlock.SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));

        return spawnedBlock;
    }

    //public void LoadStage(EStageMode stageMode)
    //{
    //    mStageMode = stageMode;

    //    switch(mStageMode)
    //    {
    //        case EStageMode.Level21:
    //            loadStage();
    //            break;
    //        case EStageMode.Training:
    //            loadTrainingStage();
    //            break;
    //    }
    //}

    public void LoadStage()
    {
        mStageMode = EStageMode.Level21;

        mRemainMoveCount = _moveCount;
        _txtMoveCount.text = $"{mRemainMoveCount}";
        mbUseMoveCount = true;

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

        createGoalBlocks();

        mInputRoutine = StartCoroutine(eInputRoutine());
    }

    public void LoadTrainingStage()
    {
        mStageMode = EStageMode.Training;

        mRemainMoveCount = int.MaxValue;
        _txtMoveCount.text = $"?";
        mbUseMoveCount = false;
        mRemainGoalCount = int.MaxValue;
        _txtGoalCount.text = $"?";

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

        // createGoalBlocks();

        mInputRoutine = StartCoroutine(eInputRoutine());
    }

    public void StopStage()
    {
        StopCoroutine(mInputRoutine);
        mInputRoutine = null;

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

        _btnPause.onClick.AddListener(() => _goPanelPause.gameObject.SetActive(true));
        _btnResume.onClick.AddListener(() => _goPanelPause.gameObject.SetActive(false));
        _btnPauseRetry.onClick.AddListener(() =>
        {
            _goPanelPause.gameObject.SetActive(false);
            reloadStage();
        });
        _btnClearRetry.onClick.AddListener(() =>
        {
            _goPanelClear.gameObject.SetActive(false);
            reloadStage();
        });
        _btnPause.interactable = true;

        //loadStage();
        //loadTrainingStage();
    }

    
    private void createGoalBlocks()
    {
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(1, 1));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(0, 2));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(0, 3));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(0, 4));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(1, 5));

        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(2, 3));

        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(4, 1));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(4, 2));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(4, 4));
        mBoard.ReplaceBlock(_prefBreakableBlock, mBoard.GetIndex(4, 5));

        mRemainGoalCount = 10;
        _txtGoalCount.text = $"{mRemainGoalCount}";
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

    private void reloadStage()
    {
        StopCoroutine(mInputRoutine);
        mInputRoutine = null;

        for (int i = mBoard.Blocks.Count - 1; i >= 0; --i)
        {
            mBoard.DestroyBlock(mBoard.Blocks[i]);
        }

        _btnPause.interactable = true;

        switch(mStageMode)
        {
            case EStageMode.Level21:
                LoadStage();
                break;
            case EStageMode.Training:
                LoadTrainingStage();
                break;
        }
    }

    private IEnumerator eInputRoutine()
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

                board.SwapBlock(mSrcBlock, mDstBlock);
                yield return eAnimateBlockSwap(mSrcBlock, mDstBlock);

                _popInfo.Reset();
                _popInfo.state = PopInfo.EState.Swap;

                bool bSrcMatched = _popInfo.CheckMatch(mSrcBlock);
                bool bDstMatched = _popInfo.CheckMatch(mDstBlock);

                bool bSwapMatched = bSrcMatched || bDstMatched;

                if (bSwapMatched)
                {
                    if (mbUseMoveCount)
                    {
                        _txtMoveCount.text = $"{--mRemainMoveCount}";
                    }

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
                                    _btnPause.interactable = true;

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
            //_txtLevelClear.gameObject.SetActive(true);
            _goPanelClear.gameObject.SetActive(true);
        }
        else if(mRemainMoveCount <= 0)
        {
            //_txtLevelFail.gameObject.SetActive(true);
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

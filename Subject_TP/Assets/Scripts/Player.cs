using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
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

                bool bSrcPopping = checkMatch(mSrcBlock);
                bool bDstPopping = checkMatch(mDstBlock);

                mbPopping = bSrcPopping || bDstPopping;

                if(mbPopping)
                {
                    int breaker = 0;
                    while (true)
                    {
                        // if(Input.GetKeyDown(KeyCode.Space))
                        {
                            if (!dropBlocks())
                            {
                                break;
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

    private bool checkMatch(Block srcBlock)
    {
        bool bMatched = false;

        foreach (var matchCheck in _matchChecks)
        {
            if (matchCheck.Check(srcBlock, out List<Block> matchableBlocks))
            {
                for (int i = matchableBlocks.Count - 1; i >= 0; i--)
                {
                    HexBoardManager.Instance.DestroyBlock(matchableBlocks[i]);
                }

                bMatched = true;
            }
        }

        return bMatched;
    }

    private bool dropBlocks()
    {
        var board = HexBoardManager.Instance;

        Block[] blocks = board.Blocks;
        var dropBlocks = new List<Block>(board.Blocks.Length);
        bool bDropped = false;
        var dirs = new HexaUtility.EDirection[] { 
                                                    HexaUtility.EDirection.Down, 
                                                    HexaUtility.EDirection.LeftDown,
                                                    HexaUtility.EDirection.RightDown
                                                };

        for (int i = 0; i < blocks.Length; ++i)
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
                    // dropBlocks.Add(block);
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
}

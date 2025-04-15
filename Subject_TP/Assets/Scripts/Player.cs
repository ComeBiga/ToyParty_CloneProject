using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private MatchCheck[] _matchChecks;

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

                bool bSrcPopping = CheckMatch(mSrcBlock);
                bool bDstPopping = CheckMatch(mDstBlock);

                mbPopping = bSrcPopping || bDstPopping;

                if(mbPopping)
                {
                    int breaker = 0;
                    while (DropBlocks())
                    {
                        if (++breaker > 100)
                        {
                            Debug.LogError("Drop Loop Break");
                            break;
                        }
                    }

                    mbPopping = false;
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

    private bool CheckMatch(Block srcBlock)
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

    private bool DropBlocks()
    {
        var board = HexBoardManager.Instance;

        Block[] blocks = board.Blocks;
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

            foreach(HexaUtility.EDirection dir in dirs)
            {
                HexaVector2Int blockCoordinates = board.GetCoordinates(block.index);
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
                        HexaVector2Int upCoordinates = new HexaVector2Int(downCoordinates.row + 1, downCoordinates.column);

                        //if (!board.IsInRange(upCoordinates))
                        //{
                        //    continue;
                        //}

                        //if (!board.IsEnableCell(upCoordinates))
                        //{
                        //    continue;
                        //}

                        Block upBlock = board.GetBlock(downCoordinates.row, downCoordinates.column);

                        if(upBlock != null)
                        {
                            continue;
                        }
                    }

                    board.SetBlockPosition(downCoordinates, block);
                    bDropped = true;
                }
            }
        }

        return bDropped;
    }
}

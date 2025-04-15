using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private MatchCheck[] _matchChecks;

    private Block mSrcBlock = null;
    private Block mDstBlock = null;

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

                CheckMatch(mSrcBlock);
                CheckMatch(mDstBlock);

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

    private void CheckMatch(Block srcBlock)
    {
        foreach (var matchCheck in _matchChecks)
        {
            if (matchCheck.Check(srcBlock, out List<Block> matchableBlocks))
            {
                for (int i = matchableBlocks.Count - 1; i >= 0; i--)
                {
                    HexBoardManager.Instance.DestroyBlock(matchableBlocks[i]);
                }
            }
        }
    }
}

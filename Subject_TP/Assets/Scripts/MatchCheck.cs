using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MatchCheck : MonoBehaviour
{
    public abstract bool Check(Block block, out List<Block> matchableBlocks);
}

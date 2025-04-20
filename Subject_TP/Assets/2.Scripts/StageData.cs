using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new StageData", menuName = "StageData")]
public class StageData : ScriptableObject
{
    [Serializable]
    public struct BlockInfo
    {
        public Block.EColor color;
        public bool isGoal;
    }

    public int stageID;
    public bool startRandomBlocks = false;
    public bool useGoal = true;
    public bool useMoveCount = true;
    public BlockInfo[] blockInfos;
}

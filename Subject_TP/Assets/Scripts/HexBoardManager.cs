using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBoardManager : MonoBehaviour
{
    public static HexBoardManager Instance => instance;
    private static HexBoardManager instance = null;

    public Cell[] Cells => _board;
    public Block[] Blocks => mBlocks;

    [SerializeField]
    private int _rowSize = 6;
    [SerializeField]
    private int _columnSize = 7;

    [SerializeField]
    private Cell[] _board;
    [SerializeField]
    private Block _prefBlock;

    private Block[] mBlocks;

    public Block GetBlock(int row, int column)
    {
        int targetIndex = GetIndex(row, column);

        for(int i = 0; i < mBlocks.Length; ++i)
        {
            if (mBlocks[i] == null)
            {
                continue;
            }

            if (mBlocks[i].index == targetIndex)
            {
                return mBlocks[i];
            }
        }

        return null;
    }

    public void SetBlockIndex(int row, int column, Block block)
    {
        // Cell cell = GetCell(row, column);

        // block.transform.position = cell.transform.position;
        block.index = GetIndex(row, column);
    }
    
    public void SetBlockIndex(HexaVector2Int coordinates, Block block)
    {
        SetBlockIndex(coordinates.row, coordinates.column, block);
    }

    public void SetBlockWorldPosition(int row, int column, Block block)
    {
        Cell cell = GetCell(row, column);

        block.transform.position = cell.transform.position;
    }

    public void SwapBlock(Block srcBlock, Block dstBlock)
    {
        HexaVector2Int srcBlockCoordinates = GetCoordinates(srcBlock.index);
        HexaVector2Int dstBlockCoordinates = GetCoordinates(dstBlock.index);

        SetBlockIndex(dstBlockCoordinates.row, dstBlockCoordinates.column, srcBlock);
        SetBlockIndex(srcBlockCoordinates.row, srcBlockCoordinates.column, dstBlock);

        //SetBlockWorldPosition(dstBlockCoordinates.row, dstBlockCoordinates.column, srcBlock);
        //SetBlockWorldPosition(srcBlockCoordinates.row, srcBlockCoordinates.column, dstBlock);
    }

    public void DestroyBlock(Block block)
    {
        for(int i = 0; i < mBlocks.Length; ++i)
        {
            if (mBlocks[i] == block)
            {
                mBlocks[i] = null;
                break;
            }
        }

        Destroy(block.gameObject);
    }

    public Cell GetCell(int row, int column)
    {
        return _board[GetIndex(row, column)];
    }

    public HexaVector2Int GetCellCoordinates(Cell cell)
    {
        for(int i = 0; i < _board.Length; ++i)
        {
            if(cell == _board[i])
            {
                int row = i / _columnSize;
                int column = i % _columnSize;

                return new HexaVector2Int(row, column);
            }
        }

        return new HexaVector2Int(-1, -1);
    }

    public int GetCellIndex(Cell cell)
    {
        for (int i = 0; i < _board.Length; ++i)
        {
            if (cell == _board[i])
            {
                return i;
            }
        }

        return -1;
    }

    public HexaVector2Int GetCoordinates(int index)
    {
        return new HexaVector2Int(index / _columnSize, index % _columnSize);
    }

    public int GetIndex(int row, int column)
    {
        return row * _columnSize + column;
    }

    public int GetIndex(HexaVector2Int coordinates)
    {
        return GetIndex(coordinates.row, coordinates.column);
    }

    public bool IsInRange(int row, int column)
    {
        return row >= 0 && column >= 0 && row < _rowSize && column < _columnSize;
    }

    public bool IsInRange(HexaVector2Int coordinates)
    {
        return IsInRange(coordinates.row, coordinates.column);
    }

    public bool IsEnableCell(int row, int column)
    {
        return _board[GetIndex(row, column)].enable;
    }

    public bool IsEnableCell(HexaVector2Int coordinates)
    {
        return IsEnableCell(coordinates.row, coordinates.column);
    }

    public Vector3 GetWorldPosition(int row, int column)
    {
        Cell cell = GetCell(row, column);

        return cell.transform.position;
    }

    public Vector3 GetWorldPosition(HexaVector2Int coordinates)
    {
        return GetWorldPosition(coordinates.row, coordinates.column);
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mBlocks = new Block[_board.Length];

        for (int i = 0; i < _board.Length; ++i)
        {
            if (!_board[i].enable)
            {
                continue;
            }

            Block newBlock = Instantiate(_prefBlock);
            newBlock.index = i;
            newBlock.SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));
            mBlocks[i] = newBlock;

            HexaVector2Int coordinates = GetCoordinates(i);
            SetBlockIndex(coordinates.row, coordinates.column, newBlock);
            SetBlockWorldPosition(coordinates.row, coordinates.column, newBlock);
        }
    }
}

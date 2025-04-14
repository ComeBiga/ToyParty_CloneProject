using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexBoardManager : MonoBehaviour
{
    [Serializable]
    public class HexRow
    {
        public List<Cell> cells = new List<Cell>();
    }

    [SerializeField]
    private int _rowSize = 6;
    [SerializeField]
    private int _columnSize = 7;

    [SerializeField]
    private Cell[] _board;
    [SerializeField]
    private Block _prefBlock;

    public void SetBlockPosition(int row, int column, Block block)
    {
        Cell cell = GetCell(row, column);

        block.transform.position = cell.transform.position;
    }

    public Cell GetCell(int row, int column)
    {
        return _board[GetIndex(row, column)];
    }

    public Vector2Int GetCellCoordinates(Cell cell)
    {
        for(int i = 0; i < _board.Length; ++i)
        {
            if(cell == _board[i])
            {
                int row = i / _columnSize;
                int column = i % _columnSize;

                return new Vector2Int(row, column);
            }
        }

        return new Vector2Int(-1, -1);
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

    public Vector2Int GetCoordinates(int index)
    {
        return new Vector2Int(index / _columnSize, index % _columnSize);
    }

    public int GetIndex(int row, int column)
    {
        return row * _columnSize + column;
    }

    private void Start()
    {
        // mBoard = new Cell[_rowSize, _columnSize];

        for(int i = 0; i < _board.Length; ++i)
        {
            if (!_board[i].enable)
            {
                continue;
            }

            Block newBlock = Instantiate(_prefBlock);
            newBlock.SetColor((Block.EColor)UnityEngine.Random.Range(0, 6));

            Vector2Int coordinates = GetCoordinates(i);

            SetBlockPosition(coordinates.x, coordinates.y, newBlock);
        }
    }
}

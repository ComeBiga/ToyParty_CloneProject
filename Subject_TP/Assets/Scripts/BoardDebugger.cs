using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BoardDebugger : MonoBehaviour
{
    [SerializeField]
    private HexBoardManager _board;

    [SerializeField]
    private bool _active = true;
    [SerializeField]
    private bool _showCellIndex = true;

    private void OnDrawGizmos()
    {
        if (_active)
        {
            foreach (Cell cell in _board.Cells)
            {
                if(_showCellIndex)
                {
                    int cellIndex = _board.GetCellIndex(cell);
                    Handles.Label(cell.transform.position, $"{cellIndex}");
                }
                else
                {
                    HexaVector2Int cellCoordinates = _board.GetCellCoordinates(cell);
                    Handles.Label(cell.transform.position, $"({cellCoordinates.row}, {cellCoordinates.column})");
                }
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class GridController : SceneSingleton<GridController>
{

    [SerializeField] private GameObject _regionPrefab;
    [SerializeField] private RectTransform _regionContainer;

    private CellTile[,] _cells = new CellTile[9, 9];

    private SudokuBoard _mirrorBoard;

    public SudokuBoard MirrorBoard => _mirrorBoard;

    private void Awake()
    {
        foreach (Transform child in _regionContainer.transform)
            Destroy(child.gameObject);

        for (int yCorner = 0; yCorner < 9; yCorner += 3)
        {
            for (int xCorner = 0; xCorner < 9; xCorner += 3)
            {
                GameObject regionObject = Instantiate(_regionPrefab, _regionContainer);
                Region region = regionObject.GetComponent<Region>();

                List<CellTile> generatedCells = region.GenerateCells(xCorner, yCorner);

                foreach (CellTile generatedCell in generatedCells)
                    _cells[generatedCell.X, generatedCell.Y] = generatedCell;
            }
        }
    }

    public void SetMirrorBoard(SudokuBoard board)
    {
        _mirrorBoard = board;

        CopyBoardPreviewState(_mirrorBoard);

        _mirrorBoard.OnProposedChangeAdded += ProposedChangeAdded;
        _mirrorBoard.OnProposedChangesReset += ProposedChangesReset;

        _mirrorBoard.OnProposedChangesCommitted += ProposedChangesCommitted;
    }

    private void ProposedChangeAdded(SudokuBoard board, SudokuBoard.ProposedPlacements placements)
    {
        CellTile cellTile = GetCell(placements.Position);
        cellTile.SetValue(placements.Value);
        cellTile.SetColourState(CellTile.ColourState.PROPOSED_PLACEMENT);
    }

    private void ProposedChangesReset(SudokuBoard board)
    {
        CopyBoardPreviewState(_mirrorBoard);
    }

    public void CopyBoardPreviewState(SudokuBoard board)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                _cells[x, y].SetValue(board.PreviewState.Values[x, y]);
                _cells[x, y].SetColourState(board.PreviewState.Colours[x, y]);
            }
        }
    }

    private void ProposedChangesCommitted(SudokuBoard board, List<SudokuBoard.PlacementResult> changeResults)
    {
        foreach (SudokuBoard.PlacementResult changeResult in changeResults)
        {
            CellTile cellTile = GetCell(changeResult.Position);

            switch (changeResult.ResultCode)
            {
                case SudokuBoard.PlacementResult.PlacementResultCode.SUCCESS:
                    cellTile.SetColourState(CellTile.ColourState.PLAYER_BLUE);
                    break;
                case SudokuBoard.PlacementResult.PlacementResultCode.WRONG:
                    cellTile.SetColourState(CellTile.ColourState.INCORRECT);
                    break;
            }
        }
    }

    public CellTile GetNearestCellTile(Vector2 position, out float distance)
    {
        CellTile nearestTile = _cells[0, 0];
        distance = float.MaxValue;

        foreach (CellTile cellTile in _cells)
        {
            float tileDistance = Vector2.Distance(position, cellTile.transform.position);
            if (tileDistance < distance)
            {
                nearestTile = cellTile;
                distance = tileDistance;
            }
        }

        return nearestTile;
    }

    public CellTile GetCell(Vector2Int position)
    {
        return _cells[position.x, position.y];
    }

}
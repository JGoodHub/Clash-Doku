using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using UnityEngine;

public class GridController : SceneSingleton<GridController>
{

    [SerializeField] private GameObject _regionPrefab;
    [SerializeField] private RectTransform _regionContainer;

    private BoardCell[,] _cells = new BoardCell[9, 9];

    private Dictionary<NumberTile, Vector2Int> _placementPositionsMap;

    private SudokuBoard _boardData;

    public SudokuBoard BoardData => _boardData;

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

                List<BoardCell> generatedCells = region.GenerateCells(xCorner, yCorner);

                foreach (BoardCell generatedCell in generatedCells)
                    _cells[generatedCell.X, generatedCell.Y] = generatedCell;
            }
        }

        _placementPositionsMap = new Dictionary<NumberTile, Vector2Int>();
    }

    public void SetBoardData(SudokuBoard board)
    {
        _boardData = board;

        CopyBoardState(_boardData);

        //MatchController.Instance.OnProposedPlacementAdded += ProposedChangeAdded;
        MatchController.Instance.OnProposedPlacementsCommitted += ProposedChangesCommitted;
    }

    public void CopyBoardState(SudokuBoard board)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                _cells[x, y].SetValue(board.BaseState.Values[x, y]);
                _cells[x, y].SetColourState(board.BaseState.Colours[x, y]);
            }
        }
    }
    // private void ProposedChangeAdded(SudokuBoard board, ProposedPlacements placements)
    // {
    //     CellTile cellTile = GetCell(placements.Position);
    //     cellTile.SetValue(placements.Value);
    //     cellTile.SetColourState(ColourState.PROPOSED_PLACEMENT);
    // }

    private void ProposedChangesCommitted(SudokuBoard board, List<PlacementResult> changeResults)
    {
        foreach (PlacementResult changeResult in changeResults)
        {
            BoardCell cellTile = GetCell(changeResult.Position);

            switch (changeResult.ResultCode)
            {
                case PlacementResult.PlacementResultCode.SUCCESS:
                    cellTile.SetColourState(ColourState.PLAYER_BLUE);
                    break;
                case PlacementResult.PlacementResultCode.WRONG:
                    cellTile.SetColourState(ColourState.INCORRECT);
                    break;
            }
        }
    }

    public BoardCell GetNearestCellTile(Vector2 position, out float minDistance)
    {
        BoardCell nearestCell = _cells[0, 0];
        minDistance = float.MaxValue;

        foreach (BoardCell cellTile in _cells)
        {
            float cellDistance = Vector2.Distance(position, cellTile.transform.position);

            if (cellDistance > minDistance)
                continue;

            nearestCell = cellTile;
            minDistance = cellDistance;
        }

        return nearestCell;
    }

    public BoardCell GetCell(Vector2Int position)
    {
        return _cells[position.x, position.y];
    }

    public bool IsTileOnBoard(NumberTile tile)
    {
        return _placementPositionsMap.ContainsKey(tile);
    }

    public bool IsTileWithinRangeOfValidCell(NumberTile tile, out BoardCell cell)
    {
        cell = GetNearestCellTile(tile.transform.position, out float cellTileDistance);
        _boardData.BaseState.GetValueAndColour(cell.Position, out int value, out ColourState state);

        // To far away from a cell
        if (cellTileDistance > 0.25f)
            return false;

        // You can't override already correct slots
        if (value == -1 || state == ColourState.INCORRECT)
            return true;

        return false;
    }

    public void HandleDraggingTileOffBoard(NumberTile tile)
    {
        MatchController.Instance.RemoveProposedPlacement(_placementPositionsMap[tile]);

        _placementPositionsMap.Remove(tile);
    }

    public void HandleTilePlacedOnBoard(NumberTile tile, BoardCell cell)
    {
        ProposedPlacements placement = new ProposedPlacements(cell.Position, tile.Value);
        MatchController.Instance.AddProposedPlacement(placement);

        tile.transform.position = (Vector2)cell.transform.position;
        tile.SetScaleFactor(SortingLayer.BOARD);
        tile.SetColourState(ColourState.PROPOSED_PLACEMENT);

        SortingLayerHandler.Instance.SetSortingLayer(transform, SortingLayer.BOARD);

        _placementPositionsMap.Add(tile, cell.Position);
    }

}

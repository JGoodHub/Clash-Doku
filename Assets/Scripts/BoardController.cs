using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using NaughtyAttributes;
using UnityEngine;
using Random = System.Random;

public class BoardController : SceneSingleton<BoardController>
{

    [InfoBox("Cells on the board are indexed from top left to bottom right, e.g. (0, 0) to (8, 8)")]
    [SerializeField] private List<BoardRegion> _regions = new List<BoardRegion>();
    [Space]
    [SerializeField] private GameObject _bonusIconPrefab;
    [SerializeField] private RectTransform _bonusIconsContainer;
    [Space]
    [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 _x2BonusProbability = new Vector2(0.8f, 0.9f);
    [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 _x4BonusProbability = new Vector2(0.9f, 0.967f);
    [SerializeField, MinMaxSlider(0f, 1f)] private Vector2 _x6BonusProbability = new Vector2(0.967f, 1f);

    private SudokuBoard _boardData;
    private MatchReport _matchReport;

    private BoardCell[,] _cells = new BoardCell[9, 9];
    private int[,] _bonusMultipliers = new int[9, 9];

    private List<PositionTilePair> _playerTilesAndPositions = new List<PositionTilePair>();
    private Dictionary<Vector2Int, BonusIcon> _bonusIcons = new Dictionary<Vector2Int, BonusIcon>();

    public void Initialise(SudokuBoard board)
    {
        _matchReport = GameController.Instance.ActiveMatchReport;
        _boardData = board;

        SetupBoardCells();

        SetupBonusIcons();
    }

    private void SetupBoardCells()
    {
        int absIndex = 0;

        for (int yCorner = 0; yCorner < 9; yCorner += 3)
        {
            for (int xCorner = 0; xCorner < 9; xCorner += 3)
            {
                List<BoardCell> generatedCells = _regions[absIndex].GenerateCells(xCorner, yCorner);

                foreach (BoardCell generatedCell in generatedCells)
                {
                    _cells[generatedCell.X, generatedCell.Y] = generatedCell;
                    generatedCell.SetValue(_boardData.BaseState.Values[generatedCell.X, generatedCell.Y]);
                }

                absIndex++;
            }
        }
    }

    private void SetupBonusIcons()
    {
        Random random = new Random(_matchReport.RoomID);

        for (int y = 0; y < _bonusMultipliers.GetLength(1); y++)
        {
            for (int x = 0; x < _bonusMultipliers.GetLength(0); x++)
            {
                if (_boardData.BaseState.Values[x, y] != -1)
                    continue;

                Vector2Int position = new Vector2Int(x, y);

                float chance = (float)random.NextDouble();
                int multiplier = 1;

                if (_x2BonusProbability.x <= chance && chance <= _x2BonusProbability.y)
                {
                    multiplier = 2;
                }
                else if (_x4BonusProbability.x <= chance && chance <= _x4BonusProbability.y)
                {
                    multiplier = 4;
                }
                else if (_x6BonusProbability.x <= chance && chance <= _x6BonusProbability.y)
                {
                    multiplier = 6;
                }

                _bonusMultipliers[x, y] = multiplier;

                // Create the bonus icon

                if (multiplier > 1)
                {
                    BonusIcon bonusIcon = Instantiate(_bonusIconPrefab, _bonusIconsContainer).GetComponent<BonusIcon>();
                    bonusIcon.transform.position = _cells[x, y].transform.position;

                    bonusIcon.Initialise(_bonusMultipliers[x, y]);

                    _bonusIcons.Add(position, bonusIcon);
                }
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
        return _playerTilesAndPositions.Exists(pair => pair.Tile == tile);
    }

    public bool IsTileWithinRangeOfValidCell(NumberTile tile, out BoardCell cell)
    {
        cell = GetNearestCellTile(tile.transform.position, out float cellTileDistance);
        _boardData.BaseState.GetValueAndColour(cell.Position, out int value, out ColourState state);

        // To far away from a cell
        if (cellTileDistance > 0.25f)
            return false;

        // You can't override already correct slots
        if (value != -1 && state != ColourState.INCORRECT)
            return false;

        // Can't place over an existing guess
        Vector2Int cellPosition = cell.Position;
        if (_playerTilesAndPositions.Exists(item => item.Position == cellPosition))
            return false;

        return true;
    }

    public void HandleDraggingTileOffBoard(NumberTile tile)
    {
        PositionTilePair tileAndPosition = _playerTilesAndPositions.Find(pair => pair.Tile == tile);

        MatchController.Instance.RemoveProposedPlacement(tileAndPosition.Position);

        SetBonusCellOccupied(tileAndPosition.Position, false);

        _playerTilesAndPositions.Remove(tileAndPosition);
    }

    public void HandleTilePlacedOnBoard(NumberTile tile, BoardCell cell)
    {
        ProposedGuess guess = new ProposedGuess(cell.Position, tile.Value);
        MatchController.Instance.AddProposedPlacement(guess);

        tile.transform.position = (Vector2)cell.transform.position;
        tile.SetScaleFactor(SortingLayer.BOARD);
        tile.SetColourState(ColourState.PROPOSED_PLACEMENT);

        SortingLayerHandler.Instance.SetSortingLayer(tile.transform, SortingLayer.BOARD);

        SetBonusCellOccupied(cell.Position, true);

        _playerTilesAndPositions.Add(new PositionTilePair(cell.Position, tile));
    }

    public NumberTile GetTileForPosition(Vector2Int position)
    {
        return _playerTilesAndPositions.Find(pair => pair.Position == position)?.Tile;
    }

    public void RemoveTileFromBoard(NumberTile tile)
    {
        PositionTilePair tileAndPosition = _playerTilesAndPositions.Find(pair => pair.Tile == tile);
        _playerTilesAndPositions.Remove(tileAndPosition);
    }

    public void LockAllTiles()
    {
        foreach (PositionTilePair pair in _playerTilesAndPositions)
        {
            pair.Tile.SetLockedState(true);
        }
    }

    public void SetBonusCellOccupied(Vector2Int position, bool isOccupied)
    {
        if (_bonusIcons.TryGetValue(position, out BonusIcon bonusIcon))
        {
            bonusIcon.SetOccupied(isOccupied);
        }
    }

    public void RemoveBonusAtPosition(Vector2Int position)
    {
        if (_bonusIcons.TryGetValue(position, out BonusIcon bonusIcon))
        {
            Destroy(bonusIcon.gameObject);
        }
    }

    public int GetScoreForPosition(Vector2Int position)
    {
        return 1 * _bonusMultipliers[position.x, position.y];
    }
}

[Serializable]
public class PositionTilePair
{
    public Vector2Int Position;
    public NumberTile Tile;

    public PositionTilePair(Vector2Int position, NumberTile tile)
    {
        Position = position;
        Tile = tile;
    }
}

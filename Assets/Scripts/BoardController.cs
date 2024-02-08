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

    private SudokuBoard _board;
    private MatchReport _matchReport;
    private MatchConfig _matchConfig;

    private BoardCell[,] _cells = new BoardCell[9, 9];
    private int[,] _bonusMultipliers = new int[9, 9];

    private List<PositionTilePair> _playerTilesAndPositions = new List<PositionTilePair>();
    private Dictionary<Vector2Int, BonusIcon> _bonusIcons = new Dictionary<Vector2Int, BonusIcon>();

    public void Initialise(SudokuBoard board)
    {
        _matchReport = GameController.Singleton.ActiveMatchReport;
        _matchConfig = GameController.Singleton.ActiveMatchConfig;

        _board = board;

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
                    generatedCell.SetValue(_board.BoardState[generatedCell.X, generatedCell.Y]);
                }

                absIndex++;
            }
        }
    }

    private void SetupBonusIcons()
    {
        Random random = new Random(_matchReport.RoomID);

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                _bonusMultipliers[x, y] = 1;
            }
        }

        int timesTwoBonusCount = _matchConfig.TimesTwoBonusCount;
        int timesFourBonusCount = _matchConfig.TimesFourBonusCount;
        int timesSixBonusCount = _matchConfig.TimesSixBonusCount;
        int totalBonuses = timesTwoBonusCount + timesFourBonusCount + timesSixBonusCount;

        List<Vector2Int> bonusCells = _board.GetRandomEmptyCells(totalBonuses, random);

        if (bonusCells.Count < totalBonuses)
        {
            Debug.LogError($"[{GetType()}]: The number of total bonus cells is greater than the number of blank cells on the board, " +
                           $"either reduce the number of bonuses or increase the number of blank cells");
            return;
        }

        int bonusCellIndex = 0;

        for (int i = 0; i < timesTwoBonusCount; i++)
        {
            Vector2Int bonusCell = bonusCells[bonusCellIndex];

            _bonusMultipliers[bonusCell.x, bonusCell.y] = 2;

            BonusIcon bonusIcon = Instantiate(_bonusIconPrefab, _bonusIconsContainer).GetComponent<BonusIcon>();
            bonusIcon.transform.position = _cells[bonusCell.x, bonusCell.y].transform.position;

            bonusIcon.Initialise(_bonusMultipliers[bonusCell.x, bonusCell.y]);

            _bonusIcons.Add(bonusCell, bonusIcon);
            bonusCellIndex++;
        }

        for (int i = 0; i < timesFourBonusCount; i++)
        {
            Vector2Int bonusCell = bonusCells[bonusCellIndex];

            _bonusMultipliers[bonusCell.x, bonusCell.y] = 4;

            BonusIcon bonusIcon = Instantiate(_bonusIconPrefab, _bonusIconsContainer).GetComponent<BonusIcon>();
            bonusIcon.transform.position = _cells[bonusCell.x, bonusCell.y].transform.position;

            bonusIcon.Initialise(_bonusMultipliers[bonusCell.x, bonusCell.y]);

            _bonusIcons.Add(bonusCell, bonusIcon);
            bonusCellIndex++;
        }

        for (int i = 0; i < timesSixBonusCount; i++)
        {
            Vector2Int bonusCell = bonusCells[bonusCellIndex];

            _bonusMultipliers[bonusCell.x, bonusCell.y] = 6;

            BonusIcon bonusIcon = Instantiate(_bonusIconPrefab, _bonusIconsContainer).GetComponent<BonusIcon>();
            bonusIcon.transform.position = _cells[bonusCell.x, bonusCell.y].transform.position;

            bonusIcon.Initialise(_bonusMultipliers[bonusCell.x, bonusCell.y]);

            _bonusIcons.Add(bonusCell, bonusIcon);
            bonusCellIndex++;
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
        _board.GetStateAtPosition(cell.Position, out int value);

        // To far away from a cell
        if (cellTileDistance > 0.25f)
            return false;

        // You can't override already occupied cells
        if (value != -1)
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

        MatchController.Singleton.RemoveProposedPlacement(tileAndPosition.Position);

        SetBonusCellOccupied(tileAndPosition.Position, false);

        _playerTilesAndPositions.Remove(tileAndPosition);
    }

    public void HandleTilePlacedOnBoard(NumberTile tile, BoardCell cell)
    {
        ProposedGuess guess = new ProposedGuess(cell.Position, tile.Value);
        MatchController.Singleton.AddProposedPlacement(guess);

        tile.transform.position = (Vector2)cell.transform.position;
        tile.SetScaleFactor(SortingLayer.BOARD);
        tile.SetColourState(ColourState.PROPOSED_PLACEMENT);

        SortingLayerHandler.Singleton.SetSortingLayer(tile.transform, SortingLayer.BOARD);

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

    public int GetTotalScoreForGuesses(List<ProposedGuess> guesses)
    {
        return guesses.Select(guess => GetScoreForPosition(guess.Position)).Sum();
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
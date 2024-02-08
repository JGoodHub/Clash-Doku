using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Async.Connector.Methods;
using Async.Connector.Models;
using DG.Tweening;
using GoodHub.Core.Runtime;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

[DefaultExecutionOrder(-10)]
public class MatchController : SceneSingleton<MatchController>
{

    // INSPECTOR

    [SerializeField] private GameObject _opponentTilePrefab;
    [SerializeField] private RectTransform _opponentTileSource;

    // FIELDS

    private MatchReport _matchReport;
    private MatchConfig _matchConfig;

    private SudokuBoard _board;

    private NumberBag _numberBag;

    private List<ProposedGuess> _playerGuesses;

    // PROPS

    public SudokuBoard Board => _board;

    public int RoundSeed => _matchReport.RoomID * _matchReport.RoundNumber;

    // EVENTS

    public event Action RoundEnded;

    public event Action RoundStarted;

    private void Start()
    {
        Canvas.ForceUpdateCanvases();

        _matchReport = GameController.Singleton.ActiveMatchReport;
        _matchConfig = GameController.Singleton.ActiveMatchConfig;

        HeaderController.Singleton.Initialise(CorePlayerData.Singleton.DisplayName, "Bot");

        _playerGuesses = new List<ProposedGuess>();

        // TODO Load the board state from the match report

        _board = SudokuHelper.GenerateSudokuBoard(_matchConfig.BlankCellsCount, new Random(_matchReport.RoomID));

        _matchReport.CompletedColumns = _board.GetCompletedColumns();
        _matchReport.CompletedColumns = _board.GetCompletedRows();

        BoardController.Singleton.Initialise(_board);

        RackController.Singleton.PopulateRack();

        // TODO Check for opponent moves
    }

    public NumberBag GetBoardNumberBag()
    {
        return _numberBag ??= new NumberBag(_board, RoundSeed);
    }

    public void AddProposedPlacement(ProposedGuess proposedGuess)
    {
        if (_playerGuesses.Exists(guess => guess.Position == proposedGuess.Position))
            return;

        _playerGuesses.Add(proposedGuess);
    }

    public void RemoveProposedPlacement(Vector2Int placementPosition)
    {
        ProposedGuess guessToRemove = _playerGuesses.Find(guess => guess.Position == placementPosition);

        if (guessToRemove == null)
            return;

        _playerGuesses.Remove(guessToRemove);
    }

    public void ResetAllProposedPlacements()
    {
        //_playerGuesses.Clear();
    }

    public void EndTurn()
    {
        if (_matchReport.OpponentType == OpponentType.Bot)
        {
            HandleBotGameEndTurn();
        }
        else
        {
            HandlePvpGameEndTurn();
        }
    }

    private void StartTurn()
    {
        RackController.Singleton.PopulateRack();
    }

    private void GameOver()
    {
        GameOverPopup gameOverPopup = PopupCanvas.Singleton.ShowGameOverPopup();
        gameOverPopup.Initialise(_matchReport.PlayerScore >= _matchReport.OpponentScore, _matchReport.PlayerScore, _matchReport.OpponentScore);
    }

    private void HandleBotGameEndTurn()
    {
        List<ProposedGuess> botPlacements = BotOpponentController.GetBotGuesses(RoundSeed, _playerGuesses, _board, _matchConfig.BotLevel, 5);
        EvaluateRoundResults(_playerGuesses, botPlacements);
    }

    private List<int> GetPlayerCompletedColumns(SudokuBoard preRoundBoard, List<ProposedGuess> playerCorrectGuesses)
    {
        List<int> previouslyCompletedColumns = preRoundBoard.GetCompletedColumns();
        List<int> newlyCompletedColumns = _board.GetCompletedColumns().Except(previouslyCompletedColumns).ToList();

        List<int> playerCompletedColumns = new List<int>();

        foreach (int newlyCompletedColumn in newlyCompletedColumns)
        {
            bool playerCompletedColumn = true;

            for (int y = 0; y < 9; y++)
            {
                // Was already filled the round before
                if (preRoundBoard.BoardState[newlyCompletedColumn, y] != -1)
                    continue;

                // We filled this square this round
                if (playerCorrectGuesses.Exists(guess => guess.Position == new Vector2Int(newlyCompletedColumn, y)))
                    continue;

                // The opponent filled this square this round
                playerCompletedColumn = false;
            }

            if (playerCompletedColumn)
            {
                playerCompletedColumns.Add(newlyCompletedColumn);
            }
        }

        return playerCompletedColumns;
    }

    private List<int> GetPlayerCompletedRows(SudokuBoard preRoundBoard, List<ProposedGuess> playerCorrectGuesses)
    {
        List<int> previouslyCompletedRows = preRoundBoard.GetCompletedRows();
        List<int> newlyCompletedRows = _board.GetCompletedRows().Except(previouslyCompletedRows).ToList();

        List<int> playerCompletedRows = new List<int>();

        foreach (int newlyCompletedRow in newlyCompletedRows)
        {
            bool playerCompletedRow = true;

            for (int x = 0; x < 9; x++)
            {
                // Was already filled the round before
                if (preRoundBoard.BoardState[x, newlyCompletedRow] != -1)
                    continue;

                // We filled this square this round
                if (playerCorrectGuesses.Exists(guess => guess.Position == new Vector2Int(x, newlyCompletedRow)))
                    continue;

                // The opponent filled this square this round
                playerCompletedRow = false;
            }

            if (playerCompletedRow)
            {
                playerCompletedRows.Add(newlyCompletedRow);
            }
        }

        return playerCompletedRows;
    }

    private List<int> GetOpponentCompletedColumns(SudokuBoard preRoundBoard, List<ProposedGuess> opponentCorrectGuesses)
    {
        List<int> previouslyCompletedColumns = preRoundBoard.GetCompletedColumns();
        List<int> newlyCompletedColumns = _board.GetCompletedColumns().Except(previouslyCompletedColumns).ToList();

        List<int> opponentCompletedColumns = new List<int>();

        foreach (int newlyCompletedColumn in newlyCompletedColumns)
        {
            bool opponentCompletedColumn = true;

            for (int y = 0; y < 9; y++)
            {
                // Was already filled the round before
                if (preRoundBoard.BoardState[newlyCompletedColumn, y] != -1)
                    continue;

                // We filled this square this round
                if (opponentCorrectGuesses.Exists(guess => guess.Position == new Vector2Int(newlyCompletedColumn, y)))
                    continue;

                // The opponent filled this square this round
                opponentCompletedColumn = false;
            }

            if (opponentCompletedColumn)
            {
                opponentCompletedColumns.Add(newlyCompletedColumn);
            }
        }

        return opponentCompletedColumns;
    }

    private List<int> GetOpponentCompletedRows(SudokuBoard preRoundBoard, List<ProposedGuess> opponentCorrectGuesses)
    {
        List<int> previouslyCompletedRows = preRoundBoard.GetCompletedRows();
        List<int> newlyCompletedRows = _board.GetCompletedRows().Except(previouslyCompletedRows).ToList();

        List<int> opponentCompletedRows = new List<int>();

        foreach (int newlyCompletedRow in newlyCompletedRows)
        {
            bool opponentCompletedRow = true;

            for (int x = 0; x < 9; x++)
            {
                // Was already filled the round before
                if (preRoundBoard.BoardState[x, newlyCompletedRow] != -1)
                    continue;

                // We filled this square this round
                if (opponentCorrectGuesses.Exists(guess => guess.Position == new Vector2Int(x, newlyCompletedRow)))
                    continue;

                // The opponent filled this square this round
                opponentCompletedRow = false;
            }

            if (opponentCompletedRow)
            {
                opponentCompletedRows.Add(newlyCompletedRow);
            }
        }

        return opponentCompletedRows;
    }

    private void HandlePvpGameEndTurn()
    {
        // Construct the round package

        RoundDataPackage myRoundData = new RoundDataPackage(_matchReport.RoundNumber, _playerGuesses);

        string roundDataJson = JsonConvert.SerializeObject(myRoundData, Formatting.None);
        Command roundCommand = new Command("ROUND_MOVES", roundDataJson);

        RoomMethods.SendCommandToRoom(GameController.Singleton.ActiveMatchReport.RoomID, roundCommand)
            .Then(room =>
            {
                // Look for the opponents moves for this round if they've submitted before us
                RoundDataPackage opponentRoundData = room.CommandInvocations
                    .Find(command => command.Extract<RoundDataPackage>().RoundIndex == _matchReport.RoundNumber && command.SenderUserID != roundCommand.SenderUserID)
                    .Extract<RoundDataPackage>();

                // Keep checking at intervals
                if (opponentRoundData == null)
                {
                    //TODO Setup a refresh call every 10 seconds
                    return;
                }

                // We have the data, show the results
                EvaluateRoundResults(myRoundData.ProposedGuesses, opponentRoundData.ProposedGuesses);
            });
    }

    private void EvaluateRoundResults(List<ProposedGuess> playerGuesses, List<ProposedGuess> opponentGuesses)
    {
        StartCoroutine(EvaluateRoundResultsCoroutine(playerGuesses, opponentGuesses));
    }

    private IEnumerator EvaluateRoundResultsCoroutine(List<ProposedGuess> playerGuesses, List<ProposedGuess> opponentGuesses)
    {
        // Filter the guesses into right and wrong

        FilterGuesses(playerGuesses, opponentGuesses,
            out List<ProposedGuess> playerCorrectGuesses, out List<ProposedGuess> playerWrongGuesses,
            out List<ProposedGuess> opponentCorrectGuesses, out List<ProposedGuess> opponentWrongGuesses);

        SudokuBoard preRoundBoard = _board.DeepClone();

        // Update the match report

        _board.UpdateBoardWithGuesses(playerCorrectGuesses, opponentCorrectGuesses);

        _matchReport.PlayerCorrectGuesses.AddRange(playerCorrectGuesses);
        _matchReport.OpponentCorrectGuesses.AddRange(opponentCorrectGuesses);

        _matchReport.PlayerScore += BoardController.Singleton.GetTotalScoreForGuesses(playerCorrectGuesses) +
                                    (GetPlayerCompletedColumns(preRoundBoard, playerCorrectGuesses).Count * 9) +
                                    (GetPlayerCompletedRows(preRoundBoard, playerCorrectGuesses).Count * 9);

        _matchReport.OpponentScore += BoardController.Singleton.GetTotalScoreForGuesses(opponentCorrectGuesses) +
                                      (GetOpponentCompletedColumns(preRoundBoard, opponentCorrectGuesses).Count * 9) +
                                      (GetOpponentCompletedRows(preRoundBoard, opponentCorrectGuesses).Count * 9);

        _matchReport.CompletedColumns = _board.GetCompletedColumns();
        _matchReport.CompletedRows = _board.GetCompletedRows();
        _matchReport.Board = _board.DeepClone();

        // Exclude any duplicate guesses so the same value isn't removed twice

        List<ProposedGuess> uniqueGuesses = new List<ProposedGuess>();

        foreach (ProposedGuess playerCorrectGuess in playerCorrectGuesses)
        {
            uniqueGuesses.Add(playerCorrectGuess);
        }

        foreach (ProposedGuess opponentCorrectGuess in opponentCorrectGuesses)
        {
            if (uniqueGuesses.Exists(guess => guess.Position == opponentCorrectGuess.Position))
                continue;

            uniqueGuesses.Add(opponentCorrectGuess);
        }

        _numberBag.ConsumeNumbers(uniqueGuesses.Select(guess => guess.Value).ToList());

        BoardController.Singleton.LockAllTiles();

        RackController.Singleton.ClearRack();

        FooterController.Singleton.DisableButtons();

        yield return StartCoroutine(OpponentThinking());

        yield return StartCoroutine(HandleFlyingGuesses(playerCorrectGuesses, playerWrongGuesses, opponentCorrectGuesses));

        yield return StartCoroutine(HandlePointEffects(preRoundBoard, playerCorrectGuesses, opponentCorrectGuesses));

        // Clean up and getting ready for the next round

        _playerGuesses.Clear();

        FooterController.Singleton.EnableButtons();

        if (_board.IsComplete())
        {
            GameOver();
        }
        else
        {
            StartTurn();
        }
    }

    private void FilterGuesses(List<ProposedGuess> playerGuesses, List<ProposedGuess> opponentGuesses,
        out List<ProposedGuess> playerCorrectGuesses, out List<ProposedGuess> playerWrongGuesses,
        out List<ProposedGuess> opponentCorrectGuesses, out List<ProposedGuess> opponentWrongGuesses)
    {
        playerCorrectGuesses = playerGuesses
            .Where(guess => guess.Value == _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        playerWrongGuesses = playerGuesses
            .Where(guess => guess.Value != _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        opponentCorrectGuesses = opponentGuesses
            .Where(guess => guess.Value == _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        opponentWrongGuesses = opponentGuesses
            .Where(guess => guess.Value != _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();
    }

    private IEnumerator OpponentThinking()
    {
        // Show opponent thinking and pause

        yield return new WaitForSeconds(0.5f);

        HeaderController.Singleton.OpponentProfile.SetThinkingStatus(true);

        yield return new WaitForSeconds(2f);

        HeaderController.Singleton.OpponentProfile.SetThinkingStatus(false);

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator HandleFlyingGuesses(List<ProposedGuess> playerCorrectGuesses, List<ProposedGuess> playerWrongGuesses,
        List<ProposedGuess> opponentCorrectGuesses)
    {
        // Remove all of my placements that failed

        foreach (ProposedGuess playerWrongGuess in playerWrongGuesses)
        {
            NumberTile tile = BoardController.Singleton.GetTileForPosition(playerWrongGuess.Position);

            tile.SetScaleFactor(SortingLayer.MOVING);
            SortingLayerHandler.Singleton.SetSortingLayer(tile.transform, SortingLayer.MOVING);

            BoardController.Singleton.SetBonusCellOccupied(playerWrongGuess.Position, false);

            tile.transform
                .DOMove(tile.transform.position + Vector3.right * 6f, 1.5f)
                .SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    BoardController.Singleton.RemoveTileFromBoard(tile);
                    Destroy(tile.gameObject, 1f);
                });

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);

        // Turn the remaining ones blue for my correct colour

        foreach (ProposedGuess playerCorrectGuess in playerCorrectGuesses)
        {
            NumberTile tile = BoardController.Singleton.GetTileForPosition(playerCorrectGuess.Position);
            tile.SetColourState(ColourState.PLAYER_BLUE);

            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForSeconds(0.5f);

        // Have all the correct opponents placements fly in

        foreach (ProposedGuess opponentCorrectGuess in opponentCorrectGuesses)
        {
            RectTransform movingParent = SortingLayerHandler.Singleton.GetTransformForSortingLayer(SortingLayer.MOVING);
            NumberTile opponentTile = Instantiate(_opponentTilePrefab, movingParent).GetComponent<NumberTile>();

            opponentTile.transform.position = _opponentTileSource.transform.position;

            opponentTile.SetLockedState(true);
            opponentTile.SetShadowState(true);
            opponentTile.SetState(opponentCorrectGuess.Value);
            opponentTile.SetScaleFactor(SortingLayer.MOVING);
            opponentTile.SetColourState(ColourState.PLAYER_RED);

            opponentTile.transform
                .DOMove(BoardController.Singleton.GetCell(opponentCorrectGuess.Position).transform.position, 1f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    SortingLayerHandler.Singleton.SetSortingLayer(opponentTile.transform, SortingLayer.BOARD);

                    opponentTile.SetScaleFactor(SortingLayer.BOARD);
                    opponentTile.SetShadowState(false);

                    BoardController.Singleton.SetBonusCellOccupied(opponentCorrectGuess.Position, true);

                    if (_matchReport.PlayerCorrectGuesses.Exists(playerCorrectGuess => opponentCorrectGuess.Position == playerCorrectGuess.Position))
                    {
                        opponentTile.SetMaskState(true);
                    }
                });

            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator HandlePointEffects(SudokuBoard preRoundBoard,
        List<ProposedGuess> playerCorrectGuesses, List<ProposedGuess> opponentCorrectGuesses)
    {
        // Handle individual cell scoring for the player

        Vector3 playerRoundTotalPosition = HeaderController.Singleton.PlayerProfile.RoundScorePosition;
        PointsDingEffect playerRoundTotal = EffectsController.Singleton.CreatePointsDing(0, false, playerRoundTotalPosition);

        yield return new WaitForSeconds(0.5f);

        foreach (ProposedGuess playerCorrectGuess in playerCorrectGuesses)
        {
            BoardCell cell = BoardController.Singleton.GetCell(playerCorrectGuess.Position);

            int guessScore = BoardController.Singleton.GetScoreForPosition(playerCorrectGuess.Position);

            PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(guessScore, true, cell.transform.position);

            pointsEffect.transform
                .DOMove(playerRoundTotalPosition, 0.75f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    playerRoundTotal.IncrementValue(guessScore);
                    Destroy(pointsEffect.gameObject);
                });

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(1f);

        // Handle individual cell scoring for the opponent

        Vector3 opponentRoundTotalPosition = HeaderController.Singleton.OpponentProfile.RoundScorePosition;
        PointsDingEffect opponentRoundTotal = EffectsController.Singleton.CreatePointsDing(0, false, opponentRoundTotalPosition);

        yield return new WaitForSeconds(0.5f);

        foreach (ProposedGuess guess in opponentCorrectGuesses)
        {
            BoardCell cell = BoardController.Singleton.GetCell(guess.Position);

            int guessScore = BoardController.Singleton.GetScoreForPosition(guess.Position);

            PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(guessScore, true, cell.transform.position);

            pointsEffect.transform
                .DOMove(opponentRoundTotalPosition, 0.75f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    opponentRoundTotal.IncrementValue(guessScore);
                    Destroy(pointsEffect.gameObject);
                });

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(1f);

        // Handle bonuses for the player completing a column

        List<int> playerCompletedColumns = GetPlayerCompletedColumns(preRoundBoard, playerCorrectGuesses);

        if (playerCompletedColumns.Count > 0)
        {
            foreach (int playerCompletedColumn in playerCompletedColumns)
            {
                for (int y = 0; y < 9; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(playerCompletedColumn, y);
                    BoardCell cell = BoardController.Singleton.GetCell(cellPosition);
                    PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(1, true, cell.transform.position);

                    pointsEffect.transform
                        .DOMove(playerRoundTotalPosition, 0.65f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            playerRoundTotal.IncrementValue(1);
                            Destroy(pointsEffect.gameObject);
                        });

                    yield return new WaitForSeconds(0.15f);
                }
                
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Handle bonuses for the player completing a row

        List<int> playerCompletedRows = GetPlayerCompletedRows(preRoundBoard, playerCorrectGuesses);

        if (playerCompletedRows.Count > 0)
        {
            foreach (int playerCompletedRow in playerCompletedRows)
            {
                for (int x = 0; x < 9; x++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, playerCompletedRow);
                    BoardCell cell = BoardController.Singleton.GetCell(cellPosition);
                    PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(1, true, cell.transform.position);

                    pointsEffect.transform
                        .DOMove(playerRoundTotalPosition, 0.65f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            playerRoundTotal.IncrementValue(1);
                            Destroy(pointsEffect.gameObject);
                        });

                    yield return new WaitForSeconds(0.15f);
                }
                
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Handle bonuses for the opponent completing a column

        List<int> opponentCompletedColumns = GetPlayerCompletedColumns(preRoundBoard, opponentCorrectGuesses);

        if (opponentCompletedColumns.Count > 0)
        {
            foreach (int opponentCompletedColumn in opponentCompletedColumns)
            {
                for (int y = 0; y < 9; y++)
                {
                    Vector2Int cellPosition = new Vector2Int(opponentCompletedColumn, y);
                    BoardCell cell = BoardController.Singleton.GetCell(cellPosition);
                    PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(1, true, cell.transform.position);

                    pointsEffect.transform
                        .DOMove(opponentRoundTotalPosition, 0.65f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            opponentRoundTotal.IncrementValue(1);
                            Destroy(pointsEffect.gameObject);
                        });

                    yield return new WaitForSeconds(0.15f);
                }
                
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Handle bonuses for the opponent completing a row

        List<int> opponentCompletedRows = GetPlayerCompletedRows(preRoundBoard, opponentCorrectGuesses);

        if (opponentCompletedRows.Count > 0)
        {
            foreach (int opponentCompletedRow in opponentCompletedRows)
            {
                for (int x = 0; x < 9; x++)
                {
                    Vector2Int cellPosition = new Vector2Int(x, opponentCompletedRow);
                    BoardCell cell = BoardController.Singleton.GetCell(cellPosition);
                    PointsDingEffect pointsEffect = EffectsController.Singleton.CreatePointsDing(1, true, cell.transform.position);

                    pointsEffect.transform
                        .DOMove(opponentRoundTotalPosition, 0.65f)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            opponentRoundTotal.IncrementValue(1);
                            Destroy(pointsEffect.gameObject);
                        });

                    yield return new WaitForSeconds(0.15f);
                }
                
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Add both score to the match totals

        yield return new WaitForSeconds(1f);

        Vector3 playerMatchTotalPosition = HeaderController.Singleton.PlayerProfile.MatchScorePosition;

        playerRoundTotal.transform
            .DOMove(playerMatchTotalPosition, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                HeaderController.Singleton.PlayerProfile.SetScore(_matchReport.PlayerScore);
                Destroy(playerRoundTotal.gameObject);
            });

        playerRoundTotal.transform
            .DOScale(Vector3.one * 0.1f, 0.5f)
            .SetEase(Ease.InQuad);

        Vector3 opponentMatchTotalPosition = HeaderController.Singleton.OpponentProfile.MatchScorePosition;

        opponentRoundTotal.transform
            .DOMove(opponentMatchTotalPosition, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                HeaderController.Singleton.OpponentProfile.SetScore(_matchReport.OpponentScore);
                Destroy(opponentRoundTotal.gameObject);
            });

        opponentRoundTotal.transform
            .DOScale(Vector3.one * 0.1f, 0.5f)
            .SetEase(Ease.InQuad);
    }

}

[Serializable]
public class RoundDataPackage
{

    public readonly int RoundIndex;
    public readonly List<ProposedGuess> ProposedGuesses;

    public RoundDataPackage(int roundIndex, List<ProposedGuess> proposedGuesses)
    {
        RoundIndex = roundIndex;
        ProposedGuesses = proposedGuesses;
    }

}

public class ProposedGuess
{

    public readonly Vector2Int Position;
    public readonly int Value;

    public ProposedGuess(Vector2Int position, int value)
    {
        Position = position;
        Value = value;
    }

}

public class PlacementResult
{

    public enum PlacementResultCode
    {

        SUCCESS,
        WRONG

    }

    public readonly Vector2Int Position;
    public readonly PlacementResultCode ResultCode;

    public PlacementResult(Vector2Int position, PlacementResultCode resultCode)
    {
        Position = position;
        ResultCode = resultCode;
    }

}
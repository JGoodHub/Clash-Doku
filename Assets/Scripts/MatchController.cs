using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Async.Connector;
using Async.Connector.Methods;
using Async.Connector.Models;
using DG.Tweening;
using GoodHub.Core.Runtime;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DefaultExecutionOrder(-10)]
public class MatchController : SceneSingleton<MatchController>
{

    // INSPECTOR

    [SerializeField] private GameObject _opponentTilePrefab;
    [SerializeField] private RectTransform _opponentTileSource;

    // FIELDS

    private MatchReport _matchReport;

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

        HeaderController.Singleton.Initialise(CorePlayerData.Singleton.DisplayName, "Bot");

        _playerGuesses = new List<ProposedGuess>();

        // TODO Load the board state from the match report

        _board = SudokuHelper.GenerateSudokuBoard(_matchReport.RoomID, _matchReport.StartingCoverage);

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
        List<ProposedGuess> botPlacements = BotOpponentController.GetBotGuesses(RoundSeed, _playerGuesses, _board, _matchReport.BotLevel, 5);
        EvaluateRoundResults(_playerGuesses, botPlacements);
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
        List<ProposedGuess> playerCorrectGuesses = playerGuesses
            .Where(guess => guess.Value == _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        List<ProposedGuess> playerWrongGuesses = playerGuesses
            .Where(guess => guess.Value != _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        List<ProposedGuess> opponentCorrectGuesses = opponentGuesses
            .Where(guess => guess.Value == _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        List<ProposedGuess> opponentWrongGuesses = opponentGuesses
            .Where(guess => guess.Value != _board.GetSolutionForCell(guess.Position))
            .OrderBy(guess => guess.Position.y)
            .ThenBy(guess => guess.Position.x)
            .ToList();

        _matchReport.PlayerCorrectGuesses.AddRange(playerCorrectGuesses);
        _matchReport.OpponentCorrectGuesses.AddRange(playerCorrectGuesses);

        _matchReport.PlayerScore += playerCorrectGuesses.Select(guess => BoardController.Singleton.GetScoreForPosition(guess.Position)).Sum();
        _matchReport.OpponentScore += opponentCorrectGuesses.Select(guess => BoardController.Singleton.GetScoreForPosition(guess.Position)).Sum();

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

        RackController.Singleton.ClearRack();

        StartCoroutine(CompareGuessesSequenceCoroutine(playerCorrectGuesses, playerWrongGuesses, opponentCorrectGuesses, opponentWrongGuesses));
    }

    private IEnumerator CompareGuessesSequenceCoroutine(
        List<ProposedGuess> playerCorrectGuesses, List<ProposedGuess> playerWrongGuesses,
        List<ProposedGuess> opponentCorrectGuesses, List<ProposedGuess> opponentWrongGuesses)
    {
        FooterController.Singleton.DisableButtons();

        // Lock all tiles

        BoardController.Singleton.LockAllTiles();

        // Show opponent thinking and pause

        yield return new WaitForSeconds(0.5f);

        HeaderController.Singleton.OpponentProfile.SetThinkingStatus(true);

        yield return new WaitForSeconds(2f);

        HeaderController.Singleton.OpponentProfile.SetThinkingStatus(false);

        yield return new WaitForSeconds(0.5f);

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

        // Handle scoring for the player

        Vector3 playerRoundTotalPosition = HeaderController.Singleton.PlayerProfile.RoundScorePosition;
        PointsDingEffect playerRoundTotal = EffectsController.Singleton.CreatePointsDing(0, false, playerRoundTotalPosition);

        yield return new WaitForSeconds(0.5f);

        foreach (ProposedGuess guess in playerCorrectGuesses)
        {
            BoardCell cell = BoardController.Singleton.GetCell(guess.Position);

            int guessScore = BoardController.Singleton.GetScoreForPosition(guess.Position);

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

        yield return new WaitForSeconds(0.75f);

        // Handle scoring for the opponent

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

        yield return new WaitForSeconds(0.75f);

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

        // Clean up and getting ready for the next round

        UpdateBoardWithGuesses(playerCorrectGuesses, opponentCorrectGuesses);

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

    private void UpdateBoardWithGuesses(List<ProposedGuess> playerCorrectGuesses, List<ProposedGuess> opponentCorrectGuesses)
    {
        foreach (ProposedGuess playerCorrectGuess in playerCorrectGuesses)
        {
            _board.SolidifyGuessInBaseState(playerCorrectGuess);
        }

        foreach (ProposedGuess opponentCorrectGuess in opponentCorrectGuesses)
        {
            _board.SolidifyGuessInBaseState(opponentCorrectGuess);
        }
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
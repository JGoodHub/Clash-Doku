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
using UnityEngine.UI;
using Random = System.Random;

[DefaultExecutionOrder(-10)]
public class MatchController : SceneSingleton<MatchController>
{

    // INSPECTOR

    [SerializeField] private int _gameSeed;
    [SerializeField, Range(0.05f, 0.95f)] private float _startingPercentage = 0.25f;
    [Space]
    [SerializeField] private Text _myDisplayNameText;
    [SerializeField] private Text _opponentDisplayNameText;
    [SerializeField] private Text _myScoreText;
    [SerializeField] private Text _opponentScoreText;
    [Space]
    [SerializeField] private GameObject _flyingTilePrefab;
    [SerializeField] private RectTransform _flyingTileParent;

    // FIELDS

    private MatchReport _matchReport;

    private SudokuBoard _board;
    private int _roundIndex = 1;

    private List<ProposedPlacements> _proposedPlacements;

    public List<ProposedPlacements> ProposedPlacements => _proposedPlacements;

    // PROPS

    public SudokuBoard Board => _board;

    public int RoundSeed => _gameSeed * _roundIndex;

    // EVENTS

    public event Action<SudokuBoard, ProposedPlacements> OnProposedPlacementAdded;

    public event Action<SudokuBoard> OnProposedPlacementsReset;

    public event Action<SudokuBoard, List<PlacementResult>> OnProposedPlacementsCommitted;


    private void Start()
    {
        _matchReport = GameController.Instance.ActiveMatchReport;
        _gameSeed = _matchReport.RoomID;

        _myDisplayNameText.text = CorePlayerData.Instance.DisplayName;

        _proposedPlacements = new List<ProposedPlacements>();

        // TODO Load the board state from the match report

        _board = SudokuHelper.GenerateSudokuBoard(_gameSeed, _startingPercentage);

        GridController.Instance.SetBoardData(_board);

        RackController.Instance.PopulateRack();

        // TODO Check for opponent moves
    }

    public NumberBag GetBoardNumberBag()
    {
        return new NumberBag(_board, _matchReport.RoomID);
    }

    public void AddProposedPlacement(ProposedPlacements proposedPlacements)
    {
        if (_proposedPlacements.Exists(change => change.Position == proposedPlacements.Position))
            return;

        _proposedPlacements.Add(proposedPlacements);

        OnProposedPlacementAdded?.Invoke(_board, proposedPlacements);
    }

    public void RemoveProposedPlacement(Vector2Int placementPosition)
    {
        ProposedPlacements placementToRemove = _proposedPlacements.Find(placement => placement.Position == placementPosition);

        if (placementToRemove == null)
            return;

        _proposedPlacements.Remove(placementToRemove);
    }

    public void ResetAllProposedPlacements()
    {
        _proposedPlacements.Clear();
        OnProposedPlacementsReset?.Invoke(_board);
    }

    public void CommitProposedChanges()
    {
        List<PlacementResult> changeResults = new List<PlacementResult>();

        foreach (ProposedPlacements proposedChange in _proposedPlacements)
        {
            if (_board.Solution[proposedChange.Position.x, proposedChange.Position.y] == proposedChange.Value)
            {
                changeResults.Add(new PlacementResult(proposedChange.Position, PlacementResult.PlacementResultCode.SUCCESS));
                _board.BaseState.SetValueAndColour(proposedChange.Position, proposedChange.Value, ColourState.PLAYER_BLUE);
            }
            else
            {
                changeResults.Add(new PlacementResult(proposedChange.Position, PlacementResult.PlacementResultCode.WRONG));
                _board.BaseState.SetValueAndColour(proposedChange.Position, proposedChange.Value, ColourState.INCORRECT);
            }
        }

        _proposedPlacements.Clear();
        OnProposedPlacementsCommitted?.Invoke(_board, changeResults);
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

    private void HandleBotGameEndTurn()
    {
        List<ProposedPlacements> botPlacements = BotOpponentController.GetBotPlacements(_proposedPlacements, _board, 50, 6);
        EvaluateRoundSequence(_proposedPlacements, botPlacements);
    }

    private void HandlePvpGameEndTurn()
    {
        // Construct the round package

        RoundDataPackage myRoundData = new RoundDataPackage(_roundIndex, _proposedPlacements);

        string roundDataJson = JsonConvert.SerializeObject(myRoundData, Formatting.None);
        Command roundCommand = new Command("ROUND_MOVES", roundDataJson);

        RoomMethods.SendCommandToRoom(GameController.Instance.ActiveMatchReport.RoomID, roundCommand)
            .Then(room =>
            {
                // Look for the opponents moves for this round if they've submitted before us
                RoundDataPackage opponentRoundData = room.CommandInvocations
                    .Find(command => command.Extract<RoundDataPackage>().RoundIndex == _roundIndex && command.SenderUserID != roundCommand.SenderUserID)
                    .Extract<RoundDataPackage>();

                // Keep checking at intervals
                if (opponentRoundData == null)
                {
                    //TODO Setup a refresh call every 10 seconds
                    return;
                }

                // We have the data, show the results
                EvaluateRoundSequence(myRoundData.ProposedChanges, opponentRoundData.ProposedChanges);
            });
    }

    private void EvaluateRoundSequence(List<ProposedPlacements> myPlacements, List<ProposedPlacements> opponentPlacements)
    {
        StartCoroutine(EvaluateRoundSequenceCoroutine(myPlacements, opponentPlacements));
    }

    private IEnumerator EvaluateRoundSequenceCoroutine(List<ProposedPlacements> myPlacements, List<ProposedPlacements> opponentPlacements)
    {
        BoardCell flyingTile = Instantiate(_flyingTilePrefab, _flyingTileParent).GetComponent<BoardCell>();
        CanvasGroup flyingTileAlpha = flyingTile.GetComponent<CanvasGroup>();
        flyingTile.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        foreach (ProposedPlacements opponentPlacement in opponentPlacements)
        {
            flyingTile.gameObject.SetActive(true);
            flyingTile.transform.position = _opponentDisplayNameText.transform.position;
            flyingTile.SetValue(opponentPlacement.Value);
            flyingTile.SetColourState(ColourState.PROPOSED_PLACEMENT);

            Vector3 targetCellPosition = GridController.Instance.GetCell(opponentPlacement.Position).transform.position;

            flyingTile.transform.DOMove(targetCellPosition, 0.75f).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(0.5f);

            // Did we also have a placement at this location
            ProposedPlacements myMatchingPlacement = myPlacements.Find(myPlacement => myPlacement.Position.Equals(opponentPlacement.Position));

            // We didn't place in this square
            if (myMatchingPlacement == null)
            {
                // Is the opponents placement correct
                if (_board.Solution[opponentPlacement.Position.x, opponentPlacement.Position.y] == opponentPlacement.Value)
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, ColourState.PLAYER_RED);
                }
                else
                {
                    flyingTileAlpha.DOFade(0f, 0.33f);
                }

                yield return new WaitForSeconds(0.34f);
            }
            else
            {
                // Have both players guess correctly
                if (myMatchingPlacement.Value == _board.GetSolutionForCell(myMatchingPlacement.Position) && opponentPlacement.Value == _board.GetSolutionForCell(opponentPlacement.Position))
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, ColourState.INITIAL_STATE);
                }
                else if (myMatchingPlacement.Value == _board.GetSolutionForCell(myMatchingPlacement.Position)) // Only ours was right
                {
                    _board.BaseState.SetValueAndColour(myMatchingPlacement.Position, myMatchingPlacement.Value, ColourState.PLAYER_BLUE);
                }
                else // Only the opponents was right
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, ColourState.PLAYER_RED);
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

}

[Serializable]
public class RoundDataPackage
{

    public readonly int RoundIndex;
    public readonly List<ProposedPlacements> ProposedChanges;

    public RoundDataPackage(int roundIndex, List<ProposedPlacements> proposedChanges)
    {
        RoundIndex = roundIndex;
        ProposedChanges = proposedChanges;
    }

}

public class NumberBag
{
    private List<int> _bagNumbers;

    public NumberBag(SudokuBoard board, int seed)
    {
        _bagNumbers = new List<int>();

        Random random = new Random(seed);
        Dictionary<int, int> missingNumberCounts = board.GetMissingNumberCounts();

        foreach (int key in missingNumberCounts.Keys)
        {
            for (int i = 0; i < missingNumberCounts[key]; i++)
            {
                _bagNumbers.Add(key);
            }
        }

        for (int i = 0; i < _bagNumbers.Count * 10; i++)
        {
            int indexA = random.Next(0, _bagNumbers.Count);
            int indexB = random.Next(0, _bagNumbers.Count);

            (_bagNumbers[indexA], _bagNumbers[indexB]) = (_bagNumbers[indexB], _bagNumbers[indexA]);
        }
    }


    public List<int> PeekNextNumbers(int count)
    {
        count = Mathf.Min(count, _bagNumbers.Count);

        List<int> output = new List<int>();

        for (int i = 0; i < count; i++)
        {
            output.Add(_bagNumbers[i]);
        }

        return output;
    }

    public void ConsumeNumbers(List<int> numbers)
    {
        foreach (int number in numbers)
        {
            _bagNumbers.Remove(number);
        }
    }

}

public class ProposedPlacements
{

    public readonly Vector2Int Position;
    public readonly int Value;

    public ProposedPlacements(Vector2Int position, int value)
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

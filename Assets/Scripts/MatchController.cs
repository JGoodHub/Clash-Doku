using System;
using System.Collections;
using System.Collections.Generic;
using Async.Connector;
using DG.Tweening;
using GoodHub.Core.Runtime;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : SceneSingleton<MatchController>
{

    [SerializeField] private int _gameSeed;
    [SerializeField, Range(0.1f, 0.9f)] private float _startingPercentage = 0.25f;
    [Space]
    [SerializeField] private Text _myDisplayNameText;
    [SerializeField] private Text _opponentDisplayNameText;
    [SerializeField] private Text _myScoreText;
    [SerializeField] private Text _opponentScoreText;
    [Space]
    [SerializeField] private GameObject _flyingTilePrefab;
    [SerializeField] private RectTransform _flyingTileParent;

    private SudokuBoard _board;
    private int _roundIndex = 1;

    public SudokuBoard Board => _board;

    public int RoundSeed => _gameSeed * _roundIndex;

    private void Start()
    {
        _gameSeed = GameController.Instance.ActiveMatchReport.RoomID;

        _myDisplayNameText.text = CorePlayerData.Instance.DisplayName;

        // TODO Load the board state from the match report

        _board = SudokuHelper.GenerateSudokuBoard(_gameSeed, _startingPercentage);
        GridController.Instance.SetMirrorBoard(_board);

        // TODO Check for opponent moves
    }

    public void EndTurn()
    {
        // Construct the round package

        RoundDataPackage myRoundData = new RoundDataPackage(_roundIndex, _board.ProposedChanges);

        string roundDataJson = JsonConvert.SerializeObject(myRoundData, Formatting.None);
        Command roundCommand = new Command("ROUND_MOVES", roundDataJson);

        RoomMethods.SendCommandToRoom(GameController.Instance.ActiveMatchReport.RoomID, roundCommand)
            .Then(room =>
            {
                RoundDataPackage opponentRoundData = room.CommandInvocations
                    .Find(command => command.Extract<RoundDataPackage>().RoundIndex == _roundIndex && command.SenderUserID != roundCommand.SenderUserID)
                    .Extract<RoundDataPackage>();

                if (opponentRoundData == null)
                {
                    //TODO Setup a refresh call every 10 seconds
                    return;
                }

                EvaluateRoundSequence(myRoundData.ProposedChanges, opponentRoundData.ProposedChanges);
            });
    }

    private void EvaluateRoundSequence(List<SudokuBoard.ProposedPlacements> myPlacements, List<SudokuBoard.ProposedPlacements> opponentPlacements)
    {
        StartCoroutine(EvaluateRoundSequenceCoroutine(myPlacements, opponentPlacements));
    }

    private IEnumerator EvaluateRoundSequenceCoroutine(List<SudokuBoard.ProposedPlacements> myPlacements, List<SudokuBoard.ProposedPlacements> opponentPlacements)
    {
        CellTile flyingTile = Instantiate(_flyingTilePrefab, _flyingTileParent).GetComponent<CellTile>();
        CanvasGroup flyingTileAlpha = flyingTile.GetComponent<CanvasGroup>();
        flyingTile.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        foreach (SudokuBoard.ProposedPlacements opponentPlacement in opponentPlacements)
        {
            flyingTile.gameObject.SetActive(true);
            flyingTile.transform.position = _opponentDisplayNameText.transform.position;
            flyingTile.SetValue(opponentPlacement.Value);
            flyingTile.SetColourState(CellTile.ColourState.PROPOSED_PLACEMENT);

            Vector3 targetCellPosition = GridController.Instance.GetCell(opponentPlacement.Position).transform.position;

            flyingTile.transform.DOMove(targetCellPosition, 0.75f).SetEase(Ease.InOutQuad);

            yield return new WaitForSeconds(0.5f);

            // Did we also have a placement at this location
            SudokuBoard.ProposedPlacements myMatchingPlacement = myPlacements.Find(myPlacement => myPlacement.Position.Equals(opponentPlacement.Position));

            // We didn't place in this square
            if (myMatchingPlacement == null)
            {
                // Is the opponents placement correct
                if (_board.Solution[opponentPlacement.Position.x, opponentPlacement.Position.y] == opponentPlacement.Value)
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, CellTile.ColourState.PLAYER_RED);
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
                if (myMatchingPlacement.Value == _board.SolutionAtPosition(myMatchingPlacement.Position) && opponentPlacement.Value == _board.SolutionAtPosition(opponentPlacement.Position))
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, CellTile.ColourState.INITIAL_STATE);
                }
                else if (myMatchingPlacement.Value == _board.SolutionAtPosition(myMatchingPlacement.Position)) // Only ours was right
                {
                    _board.BaseState.SetValueAndColour(myMatchingPlacement.Position, myMatchingPlacement.Value, CellTile.ColourState.PLAYER_BLUE);
                }
                else // Only the opponents was right
                {
                    _board.BaseState.SetValueAndColour(opponentPlacement.Position, opponentPlacement.Value, CellTile.ColourState.PLAYER_RED);
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
    public readonly List<SudokuBoard.ProposedPlacements> ProposedChanges;

    public RoundDataPackage(int roundIndex, List<SudokuBoard.ProposedPlacements> proposedChanges)
    {
        RoundIndex = roundIndex;
        ProposedChanges = proposedChanges;
    }

}
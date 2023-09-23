using System;
using System.Collections.Generic;
using UnityEngine;

public class SudokuBoard
{

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

    public class BoardState
    {

        public readonly int[,] Values;
        public readonly CellTile.ColourState[,] Colours;

        public BoardState(int[,] initialValues)
        {
            Values = new int[9, 9];
            Colours = new CellTile.ColourState[9, 9];

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Values[x, y] = initialValues[x, y];
                }
            }
        }

        public void CopyFrom(BoardState other)
        {
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Values[x, y] = other.Values[x, y];
                    Colours[x, y] = other.Colours[x, y];
                }
            }
        }

        public void SetValueAndColour(Vector2Int position, int value, CellTile.ColourState colourState)
        {
            Values[position.x, position.y] = value;
            Colours[position.x, position.y] = colourState;
        }

        public void GetValueAndColour(Vector2Int position, out int value, out CellTile.ColourState colourState)
        {
            value = Values[position.x, position.y];
            colourState = Colours[position.x, position.y];
        }

    }

    private List<ProposedPlacements> _proposedChanges;

    public readonly BoardState BaseState;
    public readonly BoardState PreviewState;
    public readonly int[,] Solution;

    public List<ProposedPlacements> ProposedChanges => _proposedChanges;

    public event Action<SudokuBoard, ProposedPlacements> OnProposedChangeAdded;

    public event Action<SudokuBoard> OnProposedChangesReset;
    public event Action<SudokuBoard, List<PlacementResult>> OnProposedChangesCommitted;

    public SudokuBoard(int[,] solution, int[,] currentValues)
    {
        BaseState = new BoardState(currentValues);
        PreviewState = new BoardState(currentValues);
        Solution = solution;

        _proposedChanges = new List<ProposedPlacements>();
    }

    public void AddProposedValueChange(ProposedPlacements proposedPlacements)
    {
        if (_proposedChanges.Exists(change => change.Position == proposedPlacements.Position))
            return;

        _proposedChanges.Add(proposedPlacements);
        PreviewState.SetValueAndColour(proposedPlacements.Position, proposedPlacements.Value, CellTile.ColourState.PROPOSED_PLACEMENT);

        OnProposedChangeAdded?.Invoke(this, proposedPlacements);
    }

    public void ResetAllProposedValueChanges()
    {
        _proposedChanges.Clear();
        PreviewState.CopyFrom(BaseState);
        OnProposedChangesReset?.Invoke(this);
    }

    public void CommitProposedChanges()
    {
        PreviewState.CopyFrom(BaseState);

        List<PlacementResult> changeResults = new List<PlacementResult>();

        foreach (ProposedPlacements proposedChange in _proposedChanges)
        {
            if (Solution[proposedChange.Position.x, proposedChange.Position.y] == proposedChange.Value)
            {
                changeResults.Add(new PlacementResult(proposedChange.Position, PlacementResult.PlacementResultCode.SUCCESS));
                BaseState.SetValueAndColour(proposedChange.Position, proposedChange.Value, CellTile.ColourState.PLAYER_BLUE);
            }
            else
            {
                changeResults.Add(new PlacementResult(proposedChange.Position, PlacementResult.PlacementResultCode.WRONG));
                BaseState.SetValueAndColour(proposedChange.Position, proposedChange.Value, CellTile.ColourState.INCORRECT);
            }
        }

        _proposedChanges.Clear();
        OnProposedChangesCommitted?.Invoke(this, changeResults);
    }

    public Dictionary<int, int> GetMissingNumberCounts()
    {
        Dictionary<int, int> numberCounts = new Dictionary<int, int>()
        {
            {1, 0},
            {2, 0},
            {3, 0},
            {4, 0},
            {5, 0},
            {6, 0},
            {7, 0},
            {8, 0},
            {9, 0},
        };

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (BaseState.Values[x, y] == -1 || BaseState.Colours[x, y] == CellTile.ColourState.INCORRECT)
                {
                    numberCounts[Solution[x, y]]++;
                }
            }
        }

        return numberCounts;
    }

    public int SolutionAtPosition(Vector2Int position)
    {
        return Solution[position.x, position.y];
    }

}
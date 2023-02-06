using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SudokuHelper
{

    private class EntropyCell
    {

        public readonly int X;
        public readonly int Y;

        public readonly List<int> PossibleValues;

        public bool Collapsed;

        public Vector2Int AsVector => new Vector2Int(X, Y);

        public EntropyCell(int x, int y)
        {
            X = x;
            Y = y;

            PossibleValues = new List<int> {1, 2, 3, 4, 5, 6, 7, 8, 9};
        }

    }

    // The board is generated using wave function collapse
    // Start with every cell in a superposition of every state
    // As cells are assigned values (randomly at first) the list of possible states for each cell reduces until it can only be in one state
    public static SudokuBoard GenerateSudokuBoard(int seed = -1, float percentageFilled = 0.25f)
    {
        percentageFilled = Mathf.Clamp01(percentageFilled);

        if (seed != -1)
            Random.InitState(seed);

        // It'll generate a valid one eventually
        while (true)
        {
            try
            {
                List<EntropyCell> superCells = new List<EntropyCell>();
                int[,] solution = new int[9, 9];

                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        superCells.Add(new EntropyCell(x, y));
                        solution[x, y] = -1;
                    }
                }

                for (int i = 0; i < 9 * 9; i++)
                {
                    // Get the cell with the lowest entropy
                    int minEntropy = superCells.Where(cell => cell.Collapsed == false).Min(cell => cell.PossibleValues.Count);
                    List<EntropyCell> lowestEntropyCells = superCells.Where(cell => cell.Collapsed == false && cell.PossibleValues.Count == minEntropy).ToList();
                    EntropyCell targetCell = lowestEntropyCells[Random.Range(0, lowestEntropyCells.Count)];

                    List<EntropyCell> rowCells = superCells.Where(cell => cell.Y == targetCell.Y).ToList();
                    List<EntropyCell> columnCells = superCells.Where(cell => cell.X == targetCell.X).ToList();

                    List<Vector2Int> regionIndices = GetRegionIndices(targetCell.X, targetCell.Y);
                    List<EntropyCell> subGridCells = superCells.Where(cell => regionIndices.Contains(cell.AsVector)).ToList();

                    int collapsedValue = targetCell.PossibleValues[Random.Range(0, targetCell.PossibleValues.Count)];

                    foreach (EntropyCell rowCell in rowCells)
                        rowCell.PossibleValues.Remove(collapsedValue);

                    foreach (EntropyCell columnCell in columnCells)
                        columnCell.PossibleValues.Remove(collapsedValue);

                    foreach (EntropyCell regionCell in subGridCells)
                        regionCell.PossibleValues.Remove(collapsedValue);

                    targetCell.PossibleValues.RemoveAll(value => value != collapsedValue);
                    targetCell.Collapsed = true;

                    solution[targetCell.X, targetCell.Y] = collapsedValue;
                }

                int[,] state = new int[9, 9];

                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        state[x, y] = Random.Range(0f, 1f) > percentageFilled ? -1 : solution[x, y];
                    }
                }

                return new SudokuBoard(solution, state);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }

    private static List<Vector2Int> GetRegionIndices(int x, int y)
    {
        int xCorner = Mathf.FloorToInt(x / 3f) * 3;
        int yCorner = Mathf.FloorToInt(y / 3f) * 3;

        List<Vector2Int> subGridIndices = new List<Vector2Int>();

        for (int yOff = 0; yOff < 3; yOff++)
            for (int xOff = 0; xOff < 3; xOff++)
                subGridIndices.Add(new Vector2Int(xCorner + xOff, yCorner + yOff));

        return subGridIndices;
    }

}

public class SudokuBoard
{

    public class ProposedChange
    {

        public readonly Vector2Int Position;
        public readonly int Value;

        public ProposedChange(Vector2Int position, int value)
        {
            Position = position;
            Value = value;
        }

    }

    public class ChangeResult
    {

        public enum ChangeResultCode
        {

            SUCCESS,
            WRONG

        }

        public readonly Vector2Int Position;
        public readonly ChangeResultCode ResultCode;

        public ChangeResult(Vector2Int position, ChangeResultCode resultCode)
        {
            Position = position;
            ResultCode = resultCode;
        }

    }

    public class BoardState
    {

        public readonly int[,] Values;
        public readonly CellTile.ColourState[,] Colours;

        public BoardState(int[,] currentValues)
        {
            Values = new int[9, 9];
            Colours = new CellTile.ColourState[9, 9];

            for (int y = 0; y < 9; y++)
                for (int x = 0; x < 9; x++)
                    Values[x, y] = currentValues[x, y];
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

    }

    private List<ProposedChange> _proposedChanges;

    public readonly BoardState BaseState;
    public readonly BoardState PreviewState;
    public readonly int[,] Solution;

    public event Action<SudokuBoard, ProposedChange> OnProposedChangeAdded;

    public event Action<SudokuBoard> OnProposedChangesReset;
    public event Action<SudokuBoard, List<ChangeResult>> OnProposedChangesCommitted;

    public SudokuBoard(int[,] solution, int[,] currentValues)
    {
        BaseState = new BoardState(currentValues);
        PreviewState = new BoardState(currentValues);
        Solution = solution;

        _proposedChanges = new List<ProposedChange>();
    }

    public void AddProposedValueChange(ProposedChange proposedChange)
    {
        if (_proposedChanges.Exists(change => change.Position == proposedChange.Position) == false)
        {
            _proposedChanges.Add(proposedChange);
            PreviewState.SetValueAndColour(proposedChange.Position, proposedChange.Value, CellTile.ColourState.PROPOSED_CHANGE);

            OnProposedChangeAdded?.Invoke(this, proposedChange);
        }
    }

    public void ResetAllProposedValueChanges()
    {
        _proposedChanges.Clear();
        PreviewState.CopyFrom(BaseState);
        OnProposedChangesReset?.Invoke(this);
    }

    public void CommitProposedChanges()
    {
        List<ChangeResult> changeResults = new List<ChangeResult>();

        foreach (ProposedChange proposedChange in _proposedChanges)
        {
            if (Solution[proposedChange.Position.x, proposedChange.Position.y] == proposedChange.Value)
            {
                changeResults.Add(new ChangeResult(proposedChange.Position, ChangeResult.ChangeResultCode.SUCCESS));
            }
            else
            {
                changeResults.Add(new ChangeResult(proposedChange.Position, ChangeResult.ChangeResultCode.SUCCESS));
            }
        }

        _proposedChanges.Clear();
        OnProposedChangesCommitted?.Invoke(this, changeResults);
    }

}
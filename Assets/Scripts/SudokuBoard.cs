using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SudokuBoard
{
    public readonly BoardState BaseState;
    public readonly int[,] Solution;

    public SudokuBoard(int[,] solution, int[,] currentValues)
    {
        BaseState = new BoardState(currentValues);
        Solution = solution;
    }

    public Dictionary<int, int> GetMissingNumberCounts()
    {
        Dictionary<int, int> numberCounts = new Dictionary<int, int>()
        {
            { 1, 0 },
            { 2, 0 },
            { 3, 0 },
            { 4, 0 },
            { 5, 0 },
            { 6, 0 },
            { 7, 0 },
            { 8, 0 },
            { 9, 0 },
        };

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (BaseState.Values[x, y] == -1 || BaseState.Colours[x, y] == ColourState.INCORRECT)
                {
                    numberCounts[Solution[x, y]]++;
                }
            }
        }

        return numberCounts;
    }

    public List<Vector2Int> GetAllEmptyCells()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (BaseState.Values[x, y] == -1)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        return emptyCells;
    }

    public List<Vector2Int> GetRandomEmptyCells(int count)
    {
        List<Vector2Int> emptyCells = GetAllEmptyCells();
        List<Vector2Int> outputCells = new List<Vector2Int>();

        int outputCount = Mathf.Min(count, emptyCells.Count);

        for (int i = 0; i < outputCount; i++)
        {
            Vector2Int chosenCell = emptyCells[Random.Range(0, emptyCells.Count)];

            outputCells.Add(chosenCell);
            emptyCells.Remove(chosenCell);
        }

        return outputCells;
    }

    public int GetSolutionForCell(Vector2Int position)
    {
        return Solution[position.x, position.y];
    }

    public void SolidifyGuessInBaseState(ProposedGuess guess)
    {
        if (Solution[guess.Position.x, guess.Position.y] != guess.Value)
        {
            throw new Exception($"Only a correct guess can be written into the board BaseState, correct value is {Solution[guess.Position.x, guess.Position.y]}, guess was{guess.Value}");
        }

        BaseState.Values[guess.Position.x, guess.Position.y] = guess.Value;
    }


    public bool IsComplete()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (BaseState.Values[x, y] != Solution[x, y])
                    return false;
            }
        }

        return true;
    }

    public class BoardState
    {
        public readonly int[,] Values;
        public readonly ColourState[,] Colours;

        public BoardState(int[,] initialValues)
        {
            Values = new int[9, 9];
            Colours = new ColourState[9, 9];

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

        public void SetValueAndColour(Vector2Int position, int value, ColourState colourState)
        {
            Values[position.x, position.y] = value;
            Colours[position.x, position.y] = colourState;
        }

        public void GetValueAndColour(Vector2Int position, out int value, out ColourState colourState)
        {
            value = Values[position.x, position.y];
            colourState = Colours[position.x, position.y];
        }
    }
}
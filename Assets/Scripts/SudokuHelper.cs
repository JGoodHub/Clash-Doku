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

            PossibleValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
    }

    // The board is generated using wave function collapse
    // Start with every cell in a superposition of every state
    // As cells are assigned values (randomly at first) the list of possible states for all other cells reduces until they can only be in one state
    public static SudokuBoard GenerateSudokuBoard(int seed = -1, float percentageFilled = 0.25f)
    {
        percentageFilled = Mathf.Clamp01(percentageFilled);

        if (seed != -1)
        {
            Random.InitState(seed);
        }

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
                    {
                        rowCell.PossibleValues.Remove(collapsedValue);
                    }

                    foreach (EntropyCell columnCell in columnCells)
                    {
                        columnCell.PossibleValues.Remove(collapsedValue);
                    }

                    foreach (EntropyCell regionCell in subGridCells)
                    {
                        regionCell.PossibleValues.Remove(collapsedValue);
                    }

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
        {
            for (int xOff = 0; xOff < 3; xOff++)
            {
                subGridIndices.Add(new Vector2Int(xCorner + xOff, yCorner + yOff));
            }
        }

        return subGridIndices;
    }
}
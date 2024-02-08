using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime.Utils;
using UnityEngine;
using Random = System.Random;

public static class SudokuHelper
{
    private class SuperpositionCell
    {
        public readonly int X;
        public readonly int Y;

        public readonly List<int> PossibleValues;

        public bool Collapsed;

        public Vector2Int AsVector => new Vector2Int(X, Y);

        public SuperpositionCell(int x, int y)
        {
            X = x;
            Y = y;

            PossibleValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
    }

    // The board is generated using wave function collapse
    // Start with every cell in a superposition of every state
    // As cells are assigned values (randomly at first) the list of possible states for all other cells reduces until they can only be in one state
    public static SudokuBoard GenerateSudokuBoard(int blankCellsCount, Random random = null)
    {
        random ??= new Random();

        // It'll generate a valid one eventually
        while (true)
        {
            try
            {
                List<SuperpositionCell> superpositionCells = new List<SuperpositionCell>();
                int[,] solution = new int[9, 9];

                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        superpositionCells.Add(new SuperpositionCell(x, y));
                        solution[x, y] = -1;
                    }
                }

                for (int i = 0; i < 9 * 9; i++)
                {
                    // Get the cell with the lowest entropy
                    int minEntropy = superpositionCells.Where(cell => cell.Collapsed == false).Min(cell => cell.PossibleValues.Count);
                    
                    List<SuperpositionCell> lowestEntropyCells = superpositionCells.Where(cell => cell.Collapsed == false && cell.PossibleValues.Count == minEntropy).ToList();
                    SuperpositionCell targetCell = lowestEntropyCells[random.Next(0, lowestEntropyCells.Count)];

                    List<SuperpositionCell> rowCells = superpositionCells.Where(cell => cell.Y == targetCell.Y).ToList();
                    List<SuperpositionCell> columnCells = superpositionCells.Where(cell => cell.X == targetCell.X).ToList();

                    List<Vector2Int> regionIndices = GetRegionIndices(targetCell.X, targetCell.Y);
                    List<SuperpositionCell> subGridCells = superpositionCells.Where(cell => regionIndices.Contains(cell.AsVector)).ToList();

                    int collapsedValue = targetCell.PossibleValues[random.Next(0, targetCell.PossibleValues.Count)];

                    foreach (SuperpositionCell rowCell in rowCells)
                    {
                        rowCell.PossibleValues.Remove(collapsedValue);
                    }

                    foreach (SuperpositionCell columnCell in columnCells)
                    {
                        columnCell.PossibleValues.Remove(collapsedValue);
                    }

                    foreach (SuperpositionCell regionCell in subGridCells)
                    {
                        regionCell.PossibleValues.Remove(collapsedValue);
                    }

                    targetCell.PossibleValues.RemoveAll(value => value != collapsedValue);
                    targetCell.Collapsed = true;

                    solution[targetCell.X, targetCell.Y] = collapsedValue;
                }

                // Add in blank tiles to make the puzzle
                
                int[,] state = new int[9, 9];

                List<Vector2Int> cells = new List<Vector2Int>();
                
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        cells.Add(new Vector2Int(x, y));
                        state[x, y] =  solution[x, y];
                    }
                }

                for (int i = 0; i < blankCellsCount; i++)
                {
                    int blankCellIndex = random.Next(0, cells.Count);
                    Vector2Int blankCell = cells[blankCellIndex];

                    state[blankCell.x, blankCell.y] = -1;
                    
                    cells.RemoveAt(blankCellIndex);
                }
                
                return new SudokuBoard(state, solution);
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
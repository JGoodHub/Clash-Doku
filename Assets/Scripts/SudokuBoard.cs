using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class SudokuBoard
{

    public int[,] BoardState;
    public int[,] Solution;

    public SudokuBoard(int[,] currentValues, int[,] solution)
    {
        BoardState = currentValues;
        Solution = solution;
    }

    public Dictionary<int, int> GetMissingNumberCounts()
    {
        Dictionary<int, int> numberCounts = new Dictionary<int, int>
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
                if (BoardState[x, y] == -1)
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
                if (BoardState[x, y] == -1)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }

        return emptyCells;
    }

    public List<Vector2Int> GetRandomEmptyCells(int count, Random random = null)
    {
        random ??= new Random();

        List<Vector2Int> emptyCells = GetAllEmptyCells();
        List<Vector2Int> outputCells = new List<Vector2Int>();

        int outputCount = Mathf.Min(count, emptyCells.Count);

        for (int i = 0; i < outputCount; i++)
        {
            Vector2Int chosenCell = emptyCells[random.Next(0, emptyCells.Count)];

            outputCells.Add(chosenCell);
            emptyCells.Remove(chosenCell);
        }

        return outputCells;
    }

    public int GetSolutionForCell(Vector2Int position)
    {
        return Solution[position.x, position.y];
    }

    public void UpdateBoardWithGuesses(List<ProposedGuess> playerCorrectGuesses, List<ProposedGuess> opponentCorrectGuesses)
    {
        foreach (ProposedGuess playerCorrectGuess in playerCorrectGuesses)
        {
            SolidifyGuessInBaseState(playerCorrectGuess);
        }

        foreach (ProposedGuess opponentCorrectGuess in opponentCorrectGuesses)
        {
            SolidifyGuessInBaseState(opponentCorrectGuess);
        }
    }

    public void SolidifyGuessInBaseState(ProposedGuess guess)
    {
        if (Solution[guess.Position.x, guess.Position.y] != guess.Value)
        {
            throw new Exception($"Only a correct guess can be written into the board BaseState, correct value is {Solution[guess.Position.x, guess.Position.y]}, guess was{guess.Value}");
        }

        BoardState[guess.Position.x, guess.Position.y] = guess.Value;
    }

    public List<int> GetCompletedColumns()
    {
        List<int> completedColumns = new List<int>();

        for (int x = 0; x < 9; x++)
        {
            bool columnComplete = true;

            for (int y = 0; y < 9; y++)
            {
                if (BoardState[x, y] != -1)
                    continue;

                columnComplete = false;
                break;
            }

            if (columnComplete)
            {
                completedColumns.Add(x);
            }
        }

        return completedColumns;
    }

    public List<int> GetCompletedRows()
    {
        List<int> completedRows = new List<int>();

        for (int y = 0; y < 9; y++)
        {
            bool rowsComplete = true;

            for (int x = 0; x < 9; x++)
            {
                if (BoardState[x, y] != -1)
                    continue;

                rowsComplete = false;
                break;
            }

            if (rowsComplete)
            {
                completedRows.Add(y);
            }
        }

        return completedRows;
    }

    /// <summary>
    /// Get all the currently completed regions on this board.
    /// Returned regions are in pure index form where the top left region is 0 and the bottom right is 8.
    /// </summary>
    public List<int> GetCompletedRegions()
    {
        List<int> completedRegions = new List<int>();

        List<List<Vector2Int>> allRegionCells = GetAllRegionCells();

        for (int regionIndex = 0; regionIndex < allRegionCells.Count; regionIndex++)
        {
            bool regionComplete = true;

            foreach (Vector2Int cell in allRegionCells[regionIndex])
            {
                if (BoardState[cell.x, cell.y] != -1)
                    continue;

                regionComplete = false;
                break;
            }

            if (regionComplete)
            {
                completedRegions.Add(regionIndex);
            }
        }

        return completedRegions;
    }

    private List<List<Vector2Int>> GetAllRegionCells()
    {
        List<List<Vector2Int>> sortedRegionCells = new List<List<Vector2Int>>();

        for (int cornerY = 0; cornerY < 9; cornerY += 3)
        {
            for (int cornerX = 0; cornerX < 9; cornerX += 3)
            {
                List<Vector2Int> regionCells = new List<Vector2Int>();

                for (int regionY = 0; regionY < 3; regionY++)
                {
                    for (int regionX = 0; regionX < 3; regionX++)
                    {
                        regionCells.Add(new Vector2Int(cornerX + regionX, cornerY + regionY));
                    }
                }

                sortedRegionCells.Add(regionCells);
            }
        }

        return sortedRegionCells;
    }

    public bool IsComplete()
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (BoardState[x, y] != Solution[x, y])
                    return false;
            }
        }

        return true;
    }

    public SudokuBoard DeepClone()
    {
        int[,] boardState = (int[,])BoardState.Clone();
        int[,] solution = (int[,])Solution.Clone();

        return new SudokuBoard(boardState, solution);
    }

    public void SetValueAndColour(Vector2Int position, int value)
    {
        BoardState[position.x, position.y] = value;
    }

    public int GetStateAtPosition(Vector2Int position)
    {
        return BoardState[position.x, position.y];
    }

    public static List<Vector2Int> GetCellsForRegion(int regionIndex)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        // Calculate the top-left cell position of the region
        int cornerY = regionIndex / 3 * 3;
        int cornerX = (regionIndex % 3) * 3;

        // Iterate through the cells in the region
        for (int y = cornerY; y < cornerY + 3; y++)
        {
            for (int x = cornerX; x < cornerX + 3; x++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }

        return cells;
    }

}
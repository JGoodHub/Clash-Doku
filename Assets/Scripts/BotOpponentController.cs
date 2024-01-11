using System.Collections.Generic;
using UnityEngine;

public static class BotOpponentController
{

    /// <summary>
    /// Get the moves for the bot opponent in a player versus bot match.
    /// botLevel should range from 0 to 100 where 0 is super easy and 100 is super hard.
    /// </summary>
    public static List<SudokuBoard.ProposedPlacements> GetBotPlacements(List<SudokuBoard.ProposedPlacements> playerPlacements, SudokuBoard sudokuBoard, int botLevel, int placementsToMake)
    {
        List<SudokuBoard.ProposedPlacements> proposedPlacements = new List<SudokuBoard.ProposedPlacements>();

        // Just pick random cells for now and half correct values
        List<Vector2Int> randomEmptyCells = sudokuBoard.GetRandomEmptyCells(placementsToMake);

        foreach (Vector2Int emptyCell in randomEmptyCells)
        {
            if (Random.Range(0, 100) <= botLevel)
            {
                proposedPlacements.Add(new SudokuBoard.ProposedPlacements(emptyCell, sudokuBoard.GetSolutionForCell(emptyCell)));
            }
            else
            {
                proposedPlacements.Add(new SudokuBoard.ProposedPlacements(emptyCell, Random.Range(1, 10)));
            }
        }

        return proposedPlacements;
    }

}

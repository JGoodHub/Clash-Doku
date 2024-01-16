using System.Collections.Generic;
using UnityEngine;

public static class BotOpponentController
{
    /// <summary>
    /// Get the moves for the bot opponent in a player versus bot match.
    /// botLevel should range from 0 to 100 where 0 is super easy and 100 is super hard.
    /// </summary>
    public static List<ProposedGuess> GetBotGuesses(int seed, List<ProposedGuess> playerPlacements, SudokuBoard sudokuBoard, int botLevel, int placementsToMake)
    {
        System.Random random = new System.Random(seed);

        List<ProposedGuess> botGuesses = new List<ProposedGuess>();

        // Just pick random cells for now and half correct values
        List<Vector2Int> randomEmptyCells = sudokuBoard.GetRandomEmptyCells(placementsToMake);

        foreach (Vector2Int emptyCell in randomEmptyCells)
        {
            if (random.Next(0, 101) <= botLevel)
            {
                botGuesses.Add(new ProposedGuess(emptyCell, sudokuBoard.GetSolutionForCell(emptyCell)));
            }
            else
            {
                botGuesses.Add(new ProposedGuess(emptyCell, Random.Range(1, 10)));
            }
        }

        return botGuesses;
    }
}
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class BotOpponentController : SceneSingleton<BotOpponentController>
{
    
    
    
    
    
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
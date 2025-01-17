using System;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotOpponentController : SceneSingleton<BotOpponentController>
{
    public enum BotTier
    {
        Easy,
        Medium,
        Hard
    }
    
    
    [Serializable]
    public class BotSettings
    {
        public BotTier BotTier;
    }

    [SerializeField] private AnimationCurve _bonusTargetCurve;
    [SerializeField] private AnimationCurve _scoreTargetCurve;
    
    public static List<ProposedGuess> GetBotGuesses(int seed, List<ProposedGuess> playerGuesses,
        SudokuBoard sudokuBoard, MatchConfig matchConfig, MatchReport matchReport)
    {
        System.Random random = new System.Random(seed);

        List<ProposedGuess> botGuesses = new List<ProposedGuess>();

        int currentBotScore = matchReport.OpponentScore;
        int pointsGapRemaining = _scoreTar matchConfig.BotLevel - currentBotScore;

        List<Vector2Int> allEmptyCells = sudokuBoard.GetAllEmptyCells();


        // Just pick random cells for now and half correct values
        List<Vector2Int> randomEmptyCells = sudokuBoard.GetRandomEmptyCells(matchConfig.DrawTilesCount);

        foreach (Vector2Int emptyCell in randomEmptyCells)
        {
            if (random.Next(0, 101) <= 0)
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

    private int GetPotentialScoreAfterPlacement(SudokuBoard sudokuBoard, MatchReport matchReport, ProposedGuess botGuess, List<ProposedGuess> playerGuesses)
    {
        int currentBotScore = matchReport.OpponentScore;

        // That slots already filled
        if (sudokuBoard.GetStateAtPosition(botGuess.Position) == -1)
            return currentBotScore;

        currentBotScore += BoardController.Singleton.GetScoreForPosition(botGuess.Position);

        // Account for any filled, rows, columns or regions
        
        

        return currentBotScore;
    }
}
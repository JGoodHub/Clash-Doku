using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField]
    private AnimationCurve _botScoreCurve;

    public List<ProposedGuess> GetBotGuesses(int seed,
        SudokuBoard sudokuBoard, MatchConfig matchConfig, MatchReport matchReport, List<ProposedGuess> playerGuesses)
    {
        List<ProposedGuess> botGuesses = new List<ProposedGuess>();

        // For now the bot will only make at most one more correct guess than the player
        int playerCorrectGuesses = playerGuesses.Count(guess => guess.IsCorrect(sudokuBoard));
        int maxBotCorrectGuesses = Mathf.Clamp(playerCorrectGuesses + 1, 0, matchConfig.DrawTilesCount);

        int currentScore = matchReport.OpponentScore;
        int targetScore = matchConfig.TargetBotScore;

        // Queried after the player has made their guesses but before the guesses have been commited to the match report
        // This is the estimated progress at the end of the round after both the player and bot have made their guesses
        // E.g. round 0 this is 0, round 1 might be 0.04%, etc
        float postRoundGameProgress = Mathf.Clamp01(
            (matchReport.GetTotalCorrectGuesses() + playerCorrectGuesses + maxBotCorrectGuesses) /
            (float)matchConfig.BlankCellsCount);

        // How many points off are we from where we should be at the end of this round
        // Number is negative if over-scoring and positive if under-scoring
        int postRoundScoreErrorRate = Mathf.RoundToInt((_botScoreCurve.Evaluate(postRoundGameProgress) * targetScore) - currentScore);
        
        Debug.Log($"postRoundScoreErrorRate: {postRoundScoreErrorRate}");

        (List<Vector2Int> combination, int totalScore) calculatePossibleGuessesToMakeScore =
            CalculatePossibleGuessesToMakeScore(sudokuBoard, maxBotCorrectGuesses, postRoundScoreErrorRate);

        foreach (Vector2Int guessCell in calculatePossibleGuessesToMakeScore.combination)
        {
            botGuesses.Add(new ProposedGuess(guessCell, sudokuBoard.GetSolutionForCell(guessCell)));
        }

        return botGuesses;
    }

    private (List<Vector2Int> combination, int totalScore) CalculatePossibleGuessesToMakeScore(
        SudokuBoard sudokuBoard, int maxCorrectGuesses,
        int score)
    {
        BoardController boardController = BoardController.Singleton;

        // Since we can't get a negative score we just try to do as shit a turn as we can if we're over scoring
        int clampedScore = Mathf.Clamp(score, 0, score);

        List<Vector2Int> allEmptyCells = sudokuBoard.GetAllEmptyCells();

        Dictionary<Vector2Int, int> perCellScore =
            allEmptyCells.ToDictionary(pos => pos, pos => boardController.GetScoreForPosition(pos));

        // Generate all possible combinations of guesses and the score for playing that set of moves
        List<(List<Vector2Int> combination, int totalScore)> combinationsWithScores =
            new List<(List<Vector2Int>, int)>();

        for (int combinationSize = 1; combinationSize <= maxCorrectGuesses; combinationSize++)
        {
            foreach (List<Vector2Int> combination in GetCombinations(perCellScore.Keys.ToList(), combinationSize))
            {
                int totalCombinationScore = combination.Sum(pos => perCellScore[pos]);
                int completionBonusScore = GetPotentialBonusScoreForGuesses(sudokuBoard, combination, null);

                combinationsWithScores.Add((combination, totalCombinationScore + completionBonusScore));
            }
        }

        // Filter for exact matches
        List<(List<Vector2Int> combination, int totalScore)> exactMatches = combinationsWithScores
            .Where(c => c.totalScore == clampedScore)
            .ToList();

        (List<Vector2Int> combination, int totalScore) selectedCombination;

        if (exactMatches.Any())
        {
            selectedCombination = exactMatches[Random.Range(0, exactMatches.Count)];
        }
        else
        {
            // Find combinations with the closest score and group by the delta to the target score
            IGrouping<int, (List<Vector2Int> combination, int totalScore)> groupedByDistance = combinationsWithScores
                .GroupBy(c => Math.Abs(c.totalScore - clampedScore))
                .OrderBy(g => g.Key) // smallest distance first
                .First();

            List<(List<Vector2Int> combination, int totalScore)> closestCandidates = groupedByDistance.ToList();
            selectedCombination = closestCandidates[Random.Range(0, closestCandidates.Count)];
        }

        // You can now do whatever you need with selectedCombination
        Debug.Log(
            $"Selected combination with total score {string.Join(", ", selectedCombination.combination)} {selectedCombination.totalScore}");

        return selectedCombination;
    }

    private int GetPotentialBonusScoreForGuesses(SudokuBoard sudokuBoard,
        List<Vector2Int> correctBotGuesses, List<ProposedGuess> playerCorrectGuesses)
    {
        // TODO should take into account spaces the player filled as well

        List<int> previouslyCompletedRows = sudokuBoard.GetCompletedRows();
        List<int> newlyCompletedRows =
            sudokuBoard.GetCompletedRows(correctBotGuesses).Except(previouslyCompletedRows).ToList();

        List<int> previouslyCompletedColumns = sudokuBoard.GetCompletedColumns();
        List<int> newlyCompletedColumns = sudokuBoard.GetCompletedColumns(correctBotGuesses)
            .Except(previouslyCompletedColumns).ToList();

        List<int> previouslyCompletedRegions = sudokuBoard.GetCompletedRegions();
        List<int> newlyCompletedRegions = sudokuBoard.GetCompletedRegions(correctBotGuesses)
            .Except(previouslyCompletedRegions).ToList();

        return newlyCompletedRows.Count * 9 +
               newlyCompletedColumns.Count * 9 +
               newlyCompletedRegions.Count * 9;
    }


    /// <summary>
    /// Helper method to generate combinations without regard for order
    /// </summary>
    private IEnumerable<List<T>> GetCombinations<T>(List<T> list, int length)
    {
        int n = list.Count;
        if (length == 0 || length > n)
            yield break;

        // Initialize the first combination indices [0,1,2,...,length-1]
        int[] indices = new int[length];
        for (int i = 0; i < length; i++)
            indices[i] = i;

        while (true)
        {
            // Yield the current combination
            List<T> combination = new List<T>(length);
            for (int i = 0; i < length; i++)
                combination.Add(list[indices[i]]);
            yield return combination;

            // Find the rightmost index that can be incremented
            int pos = length - 1;
            while (pos >= 0 && indices[pos] == n - length + pos)
                pos--;

            // If none can be incremented, we are done
            if (pos < 0)
                break;

            // Increment this index
            indices[pos]++;

            // Reset all following indices
            for (int i = pos + 1; i < length; i++)
                indices[i] = indices[i - 1] + 1;
        }
    }
}
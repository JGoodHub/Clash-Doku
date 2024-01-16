using System;
using System.Collections;
using System.Collections.Generic;
using Async.Connector;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameController : GlobalSingleton<GameController>
{
    private MatchReport _activeMatchReport;

    public MatchReport ActiveMatchReport => _activeMatchReport;

    protected override void Awake()
    {
        base.Awake();
        AsyncConnector.InitialiseAndLogin(true);
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        yield return new WaitUntil(() => AsyncConnector.LoggedIn);

        // Load the menu scene

        SceneManager.LoadScene(1);
    }

    public void LoadIntoMatchScene(MatchReport matchReport)
    {
        _activeMatchReport = matchReport;

        SceneManager.LoadScene(2);
    }

    public void LoadHomeScene()
    {
        _activeMatchReport = null;
        SceneManager.LoadScene(1);
    }
}

[Serializable]
public class MatchReport
{
    public int RoomID;

    public OpponentType OpponentType;
    public int BotLevel;

    public int PlayerScore;
    public int OpponentScore;
    public int RoundNumber;

    public SudokuBoard.BoardState BoardState;

    public List<ProposedGuess> PlayerCorrectGuesses = new List<ProposedGuess>();
    public List<ProposedGuess> OpponentCorrectGuesses = new List<ProposedGuess>();
}

public enum OpponentType
{
    Bot,
    Human
}
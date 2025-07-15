using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "MatchConfig", menuName = "Clash-Doku/Create Match Config")]
public class MatchConfig : ScriptableObject
{
    [Header("Bonus Settings")]
    public BoardSize boardSize = BoardSize.NineByNine;
    public int BlankCellsCount = 32;

    [Header("Bonus Settings")]
    public int TimesTwoBonusCount = 9;
    public int TimesFourBonusCount = 6;
    public int TimesSixBonusCount = 3;

    [Header("Opponent Settings")]
    public OpponentType OpponentType = OpponentType.Bot;
    public int TargetBotScore = 100;

    [Header("Draw Settings")]
    public int DrawTilesCount = 5;

    public int GetBoardSizeCellCount()
    {
        switch (boardSize)
        {
            case BoardSize.FourByFour:
                return 16;
            case BoardSize.SixBySix:
                return 36;
            case BoardSize.EightByEight:
                return 64;
            case BoardSize.NineByNine:
                return 81;
            default:
                return 0;
        }
    }

    public int GetStartingOccupiedCellsCount()
    {
        return GetBoardSizeCellCount() - BlankCellsCount;
    }
}

public enum BoardSize
{
    FourByFour = 16,
    SixBySix = 36,
    EightByEight = 64,
    NineByNine = 81
}

public enum OpponentType
{
    None,
    Bot,
    Human
}
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
    public int BotLevel = 50;

    [Header("Draw Settings")]
    public int DrawTilesCount = 5;
}

public enum BoardSize
{
    FourByFour,
    SixBySix,
    EightByEight,
    NineByNine
}

public enum OpponentType
{
    None,
    Bot,
    Human
}
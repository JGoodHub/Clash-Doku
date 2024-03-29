using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "MatchConfig", menuName = "Clash-Doku/Create Match Config")]
public class MatchConfig : ScriptableObject
{

    public BoardSize boardSize = BoardSize.NineByNine;

    public OpponentType OpponentType = OpponentType.Bot;
    public int BotLevel = 50;

    public int BlankCellsCount = 32;

    public int TimesTwoBonusCount = 9;
    public int TimesFourBonusCount = 6;
    public int TimesSixBonusCount = 3;

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
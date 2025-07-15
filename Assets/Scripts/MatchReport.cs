using System;
using System.Collections.Generic;

[Serializable]
public class MatchReport
{

    public int RoomID;
    public int MasterSeed;

    public OpponentType OpponentType;

    public int PlayerScore;
    public int OpponentScore;
    public int RoundNumber;

    public List<ProposedGuess> PlayerCorrectGuesses = new List<ProposedGuess>();
    public List<ProposedGuess> OpponentCorrectGuesses = new List<ProposedGuess>();

    public int GetTotalCorrectGuesses()
    {
        return PlayerCorrectGuesses.Count + OpponentCorrectGuesses.Count;
    }

}
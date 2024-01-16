using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class HeaderController : SceneSingleton<HeaderController>
{
    [SerializeField] private PlayerProfile _playerProfile;
    [SerializeField] private PlayerProfile _opponentProfile;

    public PlayerProfile PlayerProfile => _playerProfile;

    public PlayerProfile OpponentProfile => _opponentProfile;

    public void Initialise(string playerUsername, string opponentUsername)
    {
        _playerProfile.Initialise(playerUsername, 0);
        _opponentProfile.Initialise(opponentUsername, 0);
    }
}
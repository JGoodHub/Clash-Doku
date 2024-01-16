using System;
using Async.Connector;
using Async.Connector.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _opponentNameText;
    [Space]
    [SerializeField] private TextMeshProUGUI _myScoreText;
    [SerializeField] private TextMeshProUGUI _opponentScoreText;
    [SerializeField] private TextMeshProUGUI _labelText;
    [Space]
    [SerializeField] private Button _playBtn;

    private bool _initialised;
    private Room _room;

    private void Start()
    {
        _playBtn.onClick.AddListener(EnterRoom);
    }

    public void Initialise(Room room)
    {
        if (room.HasSecondUser) // Show the match status
        {
            _opponentNameText.text = room.IsOurs() ? room.SecondaryUserData.DisplayName : room.PrimaryUserData.DisplayName;
            _labelText.text = "ONGOING";
        }
        else // Its a challenge
        {
            if (room.IsOurs()) // We're the challenger
            {
                _opponentNameText.text = "UNKNOWN";
                _labelText.text = "WAITING";
            }
            else // We're the challenged
            {
                _labelText.text = "CHALLENGED";
            }
        }

        _room = room;
        _initialised = true;
    }

    private void EnterRoom()
    {
        if (_initialised == false)
            return;

        MatchReport matchReport = LocalDataManager.Data.MatchReports.Find(report => report.RoomID == _room.RoomID);
        GameController.Singleton.LoadIntoMatchScene(matchReport);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Async.Connector;
using Async.Connector.Methods;
using Async.Connector.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MenuController : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _usernameText;
    [Space]
    [SerializeField] private Button _startNewOnlineGameBtn;
    [SerializeField] private Button _startNewOfflineGameBtn;
    [Space]
    [SerializeField] private Button _startNewOfflineEasyGameBtn;
    [SerializeField] private Button _startNewOfflineMediumGameBtn;
    [SerializeField] private Button _startNewOfflineHardGameBtn;
    [SerializeField] private Button _startNewOfflineSuperHardGameBtn;
    [Space]
    [SerializeField] private RectTransform _matchCardsContainer;
    [SerializeField] private GameObject _matchCardPrefab;

    private void Awake()
    {
        _startNewOnlineGameBtn.onClick.AddListener(StartNewOnlineAsyncGame);
        _startNewOfflineGameBtn.onClick.AddListener(StartNewOfflineAsyncGame);

        _startNewOfflineEasyGameBtn.onClick.AddListener(StartNewOfflineAsyncGameEasy);
        _startNewOfflineMediumGameBtn.onClick.AddListener(StartNewOfflineAsyncGameMedium);
        _startNewOfflineHardGameBtn.onClick.AddListener(StartNewOfflineAsyncGameHard);
        _startNewOfflineSuperHardGameBtn.onClick.AddListener(StartNewOfflineAsyncGameSuperHard);
    }

    private void Start()
    {
        _usernameText.text = CorePlayerData.Instance.DisplayName;

        InvokeRepeating(nameof(RefreshMatchCards), 0f, 10f);
    }

    private void StartNewOnlineAsyncGame()
    {
        RoomMethods.CreateOrJoinRoom(CorePlayerData.Instance.UserID)
            .Then(room =>
            {
                MatchReport newMatchReport = new MatchReport
                {
                    RoomID = room.RoomID,
                    OpponentType = OpponentType.Human
                };

                LocalDataManager.Data.MatchReports.Add(newMatchReport);
                LocalDataManager.Save();

                GameController.Instance.LoadIntoMatchScene(newMatchReport);
            });
    }

    private void StartNewOfflineAsyncGame()
    {
        MatchReport newMatchReport = new MatchReport
        {
            RoomID = 0,
            //RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 50
        };

        // LocalDataManager.Data.MatchReports.Add(newMatchReport);
        // LocalDataManager.Save();

        GameController.Instance.LoadIntoMatchScene(newMatchReport);
    }

    private void StartNewOfflineAsyncGameEasy()
    {
        MatchReport newMatchReport = new MatchReport
        {
            RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 25
        };

        // LocalDataManager.Data.MatchReports.Add(newMatchReport);
        // LocalDataManager.Save();

        GameController.Instance.LoadIntoMatchScene(newMatchReport);
    }

    private void StartNewOfflineAsyncGameMedium()
    {
        MatchReport newMatchReport = new MatchReport
        {
            RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 50
        };

        // LocalDataManager.Data.MatchReports.Add(newMatchReport);
        // LocalDataManager.Save();

        GameController.Instance.LoadIntoMatchScene(newMatchReport);
    }

    private void StartNewOfflineAsyncGameHard()
    {
        MatchReport newMatchReport = new MatchReport
        {
            RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 75
        };

        // LocalDataManager.Data.MatchReports.Add(newMatchReport);
        // LocalDataManager.Save();

        GameController.Instance.LoadIntoMatchScene(newMatchReport);
    }

    private void StartNewOfflineAsyncGameSuperHard()
    {
        MatchReport newMatchReport = new MatchReport
        {
            RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 100
        };

        // LocalDataManager.Data.MatchReports.Add(newMatchReport);
        // LocalDataManager.Save();

        GameController.Instance.LoadIntoMatchScene(newMatchReport);
    }

    private void RefreshMatchCards()
    {
        foreach (Transform child in _matchCardsContainer)
        {
            Destroy(child.gameObject);
        }

        _startNewOnlineGameBtn.interactable = AsyncConnector.ConnectionVerified;

        if (AsyncConnector.ConnectionVerified == false)
            return;

        RoomMethods.GetRoomsForUserWithID(CorePlayerData.Instance.UserID)
            .Then(rooms =>
            {
                foreach (Room room in rooms)
                {
                    MatchCard matchCard = Instantiate(_matchCardPrefab, _matchCardsContainer).GetComponent<MatchCard>();
                    matchCard.Initialise(room);
                }
            });
    }

}

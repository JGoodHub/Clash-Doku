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
    [SerializeField] private Button _startNewDevGameBtn;
    [Space]
    [SerializeField] private RectTransform _matchCardsContainer;
    [SerializeField] private GameObject _matchCardPrefab;

    private void Awake()
    {
        _startNewOnlineGameBtn.onClick.AddListener(StartNewOnlineAsyncGame);
        _startNewOfflineGameBtn.onClick.AddListener(StartNewOfflineAsyncGame);
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
            RoomID = Random.Range(100000, 999999),
            OpponentType = OpponentType.Bot,
            BotLevel = 60
        };

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

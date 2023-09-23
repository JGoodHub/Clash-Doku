using System;
using System.Collections;
using System.Collections.Generic;
using Async.Connector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _usernameText;
    [Space]
    [SerializeField] private Button _startNewGameBtn;
    [Space]
    [SerializeField] private RectTransform _matchCardsContainer;
    [SerializeField] private GameObject _matchCardPrefab;

    private void Start()
    {
        _usernameText.text = CorePlayerData.Instance.DisplayName;

        _startNewGameBtn.onClick.AddListener(StartNewAsyncGame);

        InvokeRepeating(nameof(RefreshMatchCards), 0f, 10f);
    }

    private void StartNewAsyncGame()
    {
        RoomMethods.CreateOrJoinRoom(CorePlayerData.Instance.UserID)
            .Then(room =>
            {
                MatchReport newMatchReport = new MatchReport();
                newMatchReport.RoomID = room.RoomID;

                LocalDataManager.Data.MatchReports.Add(newMatchReport);
                LocalDataManager.Save();

                GameController.Instance.LoadIntoMatchScene(newMatchReport);
            });
    }

    private void RefreshMatchCards()
    {
        foreach (Transform child in _matchCardsContainer)
        {
            Destroy(child.gameObject);
        }

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
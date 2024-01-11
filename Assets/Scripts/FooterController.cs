using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FooterController : SceneSingleton<FooterController>
{

    [SerializeField] private Button _homeBtn;
    [SerializeField] private Button _resetTilesButton;
    [SerializeField] private Button _endTurnButton;

    private void Start()
    {
        _homeBtn.onClick.AddListener(GoToHomeScene);
        _resetTilesButton.onClick.AddListener(ResetPlacedTiles);

        _endTurnButton.onClick.AddListener(EndTurn);
    }

    private void ResetPlacedTiles()
    {
        RackController.Instance.ResetPlacedTiles();
    }

    private void GoToHomeScene()
    {
        GameController.Instance.LoadHomeScene();
    }

    private void EndTurn()
    {
        MatchController.Instance.EndTurn();
    }

}

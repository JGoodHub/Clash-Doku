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
        RackController.Singleton.ResetPlacedTiles();
    }

    private void GoToHomeScene()
    {
        GameController.Singleton.LoadHomeScene();
    }

    private void EndTurn()
    {
        MatchController.Singleton.EndTurn();
    }

    public void EnableButtons()
    {
        _homeBtn.interactable = true;
        _resetTilesButton.interactable = true;
        _endTurnButton.interactable = true;
    }

    public void DisableButtons()
    {
        _homeBtn.interactable = false;
        _resetTilesButton.interactable = false;
        _endTurnButton.interactable = false;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime.PopupSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CustomGamePopup : PopupBase
{

    [SerializeField] private Button _closeBtn;

    [SerializeField] private TMP_InputField _seedInputField;
    [SerializeField] private Slider _botDifficultySlider;
    [SerializeField] private Slider _startingCoverageSlider;
    [SerializeField] private Button _startMatchButton;

    private void Start()
    {
        _seedInputField.text = Random.Range(100000, 999999).ToString();

        _botDifficultySlider.value = 0.6f;
        _startingCoverageSlider.value = 0.4f;

        _closeBtn.onClick.AddListener(() =>
        {
            PopupsController.Singleton.DismissActivePopup();
        });

        _startMatchButton.onClick.AddListener(StartCustomMatchWithParameters);
    }

    private void StartCustomMatchWithParameters()
    {
        MatchReport customMatchReport = new MatchReport
        {
            RoomID = int.Parse(_seedInputField.text),
            OpponentType = OpponentType.Bot,
            BotLevel = Mathf.RoundToInt(_botDifficultySlider.value * 100),
            StartingCoverage = _startingCoverageSlider.value,
        };

        GameController.Singleton.LoadIntoMatchScene(customMatchReport);

        PopupsController.Singleton.DismissActivePopup();
    }

}
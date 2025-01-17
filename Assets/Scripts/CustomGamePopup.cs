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
    [SerializeField] private Slider _blankCellsSlider;
    [SerializeField] private Button _startMatchButton;

    private void Start()
    {
        _seedInputField.text = Random.Range(100000, 999999).ToString();

        _botDifficultySlider.value = 0.6f;
        _blankCellsSlider.value = 0.4f;

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
            OpponentType = OpponentType.Bot
        };

        MatchConfig customMatchConfig = ScriptableObject.CreateInstance<MatchConfig>();
        //customMatchConfig.BotLevel = Mathf.RoundToInt(_botDifficultySlider.value * 100);
        customMatchConfig.BlankCellsCount = Mathf.RoundToInt(_blankCellsSlider.value * 100);

        GameController.Singleton.LoadIntoMatchScene(customMatchReport, customMatchConfig);

        PopupsController.Singleton.DismissActivePopup();
    }

}

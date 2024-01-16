using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;

public class PopupCanvas : SceneSingleton<PopupCanvas>
{
    [SerializeField] private GameObject _gameOverPopup;

    public GameOverPopup ShowGameOverPopup()
    {
        return Instantiate(_gameOverPopup, transform).GetComponent<GameOverPopup>();
    }
}
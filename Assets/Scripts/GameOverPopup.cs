using System;
using Async.Connector.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameOverPopup : MonoBehaviour
{
    [SerializeField] private GameObject _wonRoot;
    [SerializeField] private GameObject _lostRoot;
    [SerializeField] private TextMeshProUGUI _playerUsername;
    [SerializeField] private TextMeshProUGUI _opponentUsername;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private Button _menuButton;

    private void Awake()
    {
        _menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void Initialise(bool won, int playerScore, int opponentScore)
    {
        _wonRoot.SetActive(won);
        _lostRoot.SetActive(won == false);

        // _playerUsername.text = CorePlayerData.Singleton.DisplayName;
        // _opponentUsername.text = "Bot";

        _scoreText.text = $"{playerScore} to {opponentScore}";
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(1);
    }
}
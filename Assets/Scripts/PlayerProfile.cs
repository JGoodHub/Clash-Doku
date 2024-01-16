using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class PlayerProfile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _usernameText;
    [SerializeField] private TextMeshProUGUI _scoreValueText;
    [Space]
    [SerializeField] private GameObject _thinkingOverlay;
    [SerializeField] private RectTransform _roundScorePosition;

    private int _score;

    public Vector3 RoundScorePosition => _roundScorePosition.position;

    public Vector3 MatchScorePosition => _scoreValueText.transform.position;

    public void Initialise(string username, int score)
    {
        _usernameText.text = username;
        SetScore(score, true);
    }

    public void SetThinkingStatus(bool isThinking)
    {
        _thinkingOverlay.SetActive(isThinking);
    }

    public void SetScore(int newScore, bool snap = false)
    {
        if (snap)
        {
            _scoreValueText.text = newScore.ToString();
            _score = newScore;

            return;
        }

        DOVirtual
            .Int(_score, newScore, 0.67f, value =>
            {
                _scoreValueText.text = value.ToString();
            })
            .OnComplete(() =>
            {
                _score = newScore;
            });
    }
}
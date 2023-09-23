using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellTile : MonoBehaviour
{

    public enum ColourState
    {

        INITIAL_STATE,
        PROPOSED_PLACEMENT,
        PLAYER_BLUE,
        PLAYER_RED,
        INCORRECT

    }

    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private Image _background;
    [Space]
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _pendingColor;
    [SerializeField] private Color _playerOneColor;
    [SerializeField] private Color _playerTwoColor;
    [SerializeField] private Color _incorrectColour;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Value { get; private set; }

    public Vector2Int Position => new Vector2Int(X, Y);

    public void Initialise(int x, int y)
    {
        X = x;
        Y = y;

        SetValue(-1);
        SetColourState(ColourState.INITIAL_STATE);
    }

    public void SetValue(int newValue)
    {
        Value = newValue;
        _valueText.text = Value == -1 ? string.Empty : Value.ToString();
    }

    public void SetColourState(ColourState newState)
    {
        switch (newState)
        {
            case ColourState.INITIAL_STATE:
                _background.color = _defaultColor;
                break;
            case ColourState.PROPOSED_PLACEMENT:
                _background.color = _pendingColor;
                break;
            case ColourState.PLAYER_BLUE:
                _background.color = _playerOneColor;
                break;
            case ColourState.PLAYER_RED:
                _background.color = _playerTwoColor;
                break;
            case ColourState.INCORRECT:
                _background.color = _incorrectColour;
                break;
        }
    }

}
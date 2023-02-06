using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellTile : MonoBehaviour
{

    public enum ColourState
    {

        INITIAL_STATE,
        PROPOSED_CHANGE,
        CORRECT,
        WRONG

    }

    [SerializeField] private TextMeshProUGUI _valueText;
    [SerializeField] private Image _background;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _pendingColor;
    [SerializeField] private Color _playerPlacedGreenColor;
    [SerializeField] private Color _playerPlacedRedColor;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Value { get; private set; }

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
            case ColourState.PROPOSED_CHANGE:
                _background.color = _pendingColor;
                break;
            case ColourState.CORRECT:
                _background.color = _playerPlacedGreenColor;
                break;
            case ColourState.WRONG:
                _background.color = _playerPlacedRedColor;
                break;
        }
    }

}
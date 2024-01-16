using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardCell : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _valueText;

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Value { get; private set; }

    public Vector2Int Position => new Vector2Int(X, Y);

    public void Initialise(int x, int y)
    {
        X = x;
        Y = y;

        SetValue(-1);
    }

    public void SetValue(int newValue)
    {
        Value = newValue;
        _valueText.text = Value == -1 ? string.Empty : Value.ToString();
    }
}

public enum ColourState
{
    INITIAL_STATE,
    PROPOSED_PLACEMENT,
    PLAYER_BLUE,
    PLAYER_RED,
    INCORRECT
}
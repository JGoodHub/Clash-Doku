using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointsDingEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _pointsTexts;

    private int _value;

    public void Initialise(int pointsValue, bool prefixPlus)
    {
        _value = pointsValue;
        _pointsTexts.text = $"{(prefixPlus ? "+" : string.Empty)}{_value}";
    }

    public void IncrementValue(int amount)
    {
        _value += amount;
        _pointsTexts.text = $"+{_value}";
    }
}
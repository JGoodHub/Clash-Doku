using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusIcon : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [Space]
    [SerializeField] private GameObject _emptyState;
    [SerializeField] private Image _background;
    [SerializeField] private Shadow _shadow;
    [Space]
    [SerializeField] private GameObject _occupiedState;
    [SerializeField] private Image _cornersBackground;
    [Space]
    [SerializeField] private Color _x2Colour;
    [SerializeField] private Color _x2ColourShadow;
    [SerializeField] private Color _x4Colour;
    [SerializeField] private Color _x4ColourShadow;
    [SerializeField] private Color _x6Colour;
    [SerializeField] private Color _x6ColourShadow;

    public void Initialise(int multiplierValue)
    {
        _multiplierText.text = $"x{multiplierValue}";

        switch (multiplierValue)
        {
            case 2:
                _background.color = _x2Colour;
                _shadow.effectColor = _x2ColourShadow;
                _cornersBackground.color = _x2Colour;

                break;
            case 4:
                _background.color = _x4Colour;
                _shadow.effectColor = _x4ColourShadow;
                _cornersBackground.color = _x4Colour;

                break;
            case 6:
                _background.color = _x6Colour;
                _shadow.effectColor = _x6ColourShadow;
                _cornersBackground.color = _x6Colour;

                break;
        }

        SetOccupied(false);
    }

    public void SetOccupied(bool isOccupied)
    {
        _emptyState.SetActive(isOccupied == false);
        _occupiedState.SetActive(isOccupied);
    }
}
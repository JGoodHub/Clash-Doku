using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [SerializeField] private GameObject _graphicsRoot;
    [SerializeField] private GameObject _shadowObject;
    [SerializeField] private TextMeshProUGUI _valueText;
    [Space]
    [SerializeField] private float _boardScaleFactor;
    [SerializeField] private float _draggingScaleFactor;
    [SerializeField] private float _rackScaleFactor;
    [Space]
    [SerializeField] private Image _background;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _pendingColor;
    [SerializeField] private Color _playerOneColor;
    [SerializeField] private Color _playerTwoColor;
    [SerializeField] private Color _incorrectColour;

    private int _value;

    public int Value => _value;

    public void SetState(int value, bool isActive = true, bool showShadow = true)
    {
        gameObject.SetActive(isActive);

        _value = value;
        _valueText.text = _value.ToString();

        _shadowObject.SetActive(showShadow);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _shadowObject.SetActive(true);

        SortingLayerHandler.Instance.SetSortingLayer(transform, SortingLayer.MOVING);

        SetScaleFactor(SortingLayer.MOVING);
        SetColourState(ColourState.INITIAL_STATE);

        if (GridController.Instance.IsTileOnBoard(this))
        {
            GridController.Instance.HandleDraggingTileOffBoard(this);
        }
        else if (RackController.Instance.IsTileOnRack(this))
        {
            RackController.Instance.HandleDraggingTileOffRack(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 dragPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        dragPosition.z = 0;

        transform.position = dragPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _shadowObject.SetActive(false);

        if (GridController.Instance.IsTileWithinRangeOfValidCell(this, out BoardCell validCell))
        {
            GridController.Instance.HandleTilePlacedOnBoard(this, validCell);
        }
        else
        {
            RackController.Instance.HandleReturningTileToRack(this);
        }
    }

    public void SetScaleFactor(SortingLayer destination)
    {
        switch (destination)
        {
            case SortingLayer.BOARD:
                _graphicsRoot.transform.localScale = Vector3.one * _boardScaleFactor;
                break;
            case SortingLayer.RACK:
                _graphicsRoot.transform.localScale = Vector3.one * _rackScaleFactor;
                break;
            case SortingLayer.MOVING:
                _graphicsRoot.transform.localScale = Vector3.one * _draggingScaleFactor;
                break;
        }
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

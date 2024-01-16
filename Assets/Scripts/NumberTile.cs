using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NumberTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _maskImage;
    [SerializeField] private Mask _mask;
    [Space]
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

    private bool _locked;

    private int _value;

    public int Value => _value;

    public void SetState(int value, bool isActive = true)
    {
        gameObject.SetActive(isActive);

        _value = value;
        _valueText.text = _value.ToString();

        SetShadowState(false);
        SetMaskState(false);
    }

    public void SetLockedState(bool isLocked)
    {
        _locked = isLocked;
    }

    public void SetShadowState(bool showShadow)
    {
        _shadowObject.SetActive(showShadow);
    }

    public void SetMaskState(bool maskActive)
    {
        _maskImage.enabled = _mask.enabled = maskActive;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_locked)
            return;

        _shadowObject.SetActive(true);

        SortingLayerHandler.Singleton.SetSortingLayer(transform, SortingLayer.MOVING);

        SetScaleFactor(SortingLayer.MOVING);
        SetColourState(ColourState.INITIAL_STATE);

        if (BoardController.Singleton.IsTileOnBoard(this))
        {
            BoardController.Singleton.HandleDraggingTileOffBoard(this);
        }
        else if (RackController.Singleton.IsTileOnRack(this))
        {
            RackController.Singleton.HandleDraggingTileOffRack(this);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_locked)
            return;

        Vector3 dragPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        dragPosition.z = 0;

        transform.position = dragPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_locked)
            return;

        _shadowObject.SetActive(false);

        if (BoardController.Singleton.IsTileWithinRangeOfValidCell(this, out BoardCell validCell))
        {
            BoardController.Singleton.HandleTilePlacedOnBoard(this, validCell);
        }
        else
        {
            RackController.Singleton.HandleReturningTileToRack(this);
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
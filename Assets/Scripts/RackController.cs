using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RackController : SceneSingleton<RackController>
{
    [FormerlySerializedAs("_rackTilePrefab")]
    [SerializeField] private GameObject _numberTilePrefab;
    [Space]
    [SerializeField] private List<RectTransform> _rackSlotsOne = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsTwo = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsThree = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsFour = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsFive = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsSix = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsSeven = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _rackSlotsEight = new List<RectTransform>();
    [Space]
    [SerializeField] private int _tilesCount = 6;

    private Dictionary<int, List<RectTransform>> _rackSlotsByCount = new Dictionary<int, List<RectTransform>>();

    private List<NumberTile> _rackTiles = new List<NumberTile>();

    public List<NumberTile> RackTiles => _rackTiles;

    private void Awake()
    {
        _rackSlotsByCount.Add(1, _rackSlotsOne);
        _rackSlotsByCount.Add(2, _rackSlotsTwo);
        _rackSlotsByCount.Add(3, _rackSlotsThree);
        _rackSlotsByCount.Add(4, _rackSlotsFour);
        _rackSlotsByCount.Add(5, _rackSlotsFive);
        _rackSlotsByCount.Add(6, _rackSlotsSix);
        _rackSlotsByCount.Add(7, _rackSlotsSeven);
        _rackSlotsByCount.Add(8, _rackSlotsEight);
    }

    public void ClearRack()
    {
        for (int i = _rackTiles.Count - 1; i >= 0; i--)
        {
            Destroy(_rackTiles[i].gameObject);
        }

        _rackTiles.Clear();
    }

    public void PopulateRack()
    {
        NumberBag numberBag = MatchController.Singleton.GetBoardNumberBag();
        RectTransform rackSortingLayerRect = SortingLayerHandler.Singleton.GetTransformForSortingLayer(SortingLayer.RACK);

        List<int> nextNumbers = numberBag.PeekNextNumbers(_tilesCount - _rackTiles.Count);

        foreach (int number in nextNumbers)
        {
            NumberTile numberTile = Instantiate(_numberTilePrefab, rackSortingLayerRect).GetComponent<NumberTile>();
            numberTile.SetState(number);

            _rackTiles.Add(numberTile);
        }

        RefreshTilePositions();
    }

    private void RefreshTilePositions()
    {
        if (_rackTiles.Count == 0)
            return;

        List<RectTransform> rackSlots = _rackSlotsByCount[_rackTiles.Count];

        for (int i = 0; i < _rackTiles.Count; i++)
        {
            NumberTile rackTile = _rackTiles[i];
            rackTile.transform.position = rackSlots[i].transform.position;
        }
    }

    public void ResetPlacedTiles()
    {
        RefreshTilePositions();

        MatchController.Singleton.ResetAllProposedPlacements();
    }

    public RectTransform GetNearestRackSlot(Vector3 position, out int slotIndex)
    {
        RectTransform nearestSlot = null;
        float minDistance = float.MaxValue;
        slotIndex = -1;

        for (int i = 0; i < _rackSlotsByCount[_rackTiles.Count + 1].Count; i++)
        {
            RectTransform slot = _rackSlotsByCount[_rackTiles.Count + 1][i];
            float slotDistance = Vector2.Distance(position, slot.transform.position);

            if (slotDistance > minDistance)
                continue;

            nearestSlot = slot;
            minDistance = slotDistance;
            slotIndex = i;
        }

        return nearestSlot;
    }

    public bool IsTileOnRack(NumberTile tile)
    {
        return _rackTiles.Contains(tile);
    }

    public void HandleDraggingTileOffRack(NumberTile tile)
    {
        _rackTiles.Remove(tile);

        RefreshTilePositions();
    }

    public void HandleReturningTileToRack(NumberTile tile)
    {
        GetNearestRackSlot(tile.transform.position, out int slotIndex);
        _rackTiles.Insert(slotIndex, tile);

        tile.SetScaleFactor(SortingLayer.RACK);
        SortingLayerHandler.Singleton.SetSortingLayer(tile.transform, SortingLayer.RACK);

        RefreshTilePositions();
    }
}
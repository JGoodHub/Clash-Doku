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
    [SerializeField] private int _tilesCount = 7;

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

    private void Start()
    {
        MatchController.Instance.Board.OnProposedChangesCommitted += ProposedChangesCommitted;
    }

    private void ProposedChangesCommitted(SudokuBoard arg1, List<SudokuBoard.PlacementResult> arg2)
    {
        for (int i = _rackTiles.Count - 1; i >= 0; i--)
        {
            if (_rackTiles[i].gameObject.activeSelf == false)
            {
                Destroy(_rackTiles[i]);
                _rackTiles.RemoveAt(i);
            }
        }

        PopulateRack();
    }

    public void PopulateRack()
    {
        NumberBag numberBag = MatchController.Instance.GetBoardNumberBag();
        RectTransform rackSortingLayerRect = SortingLayerHandler.Instance.GetTransformForSortingLayer(SortingLayer.RACK);

        List<int> nextNumbers = numberBag.PeekNextNumbers(_tilesCount);

        foreach (int number in nextNumbers)
        {
            NumberTile numberTile = Instantiate(_numberTilePrefab, rackSortingLayerRect).GetComponent<NumberTile>();
            numberTile.SetState(number, true, false);

            _rackTiles.Add(numberTile);
        }

        RefreshTilePositions();
    }

    private void RefreshTilePositions()
    {
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

        MatchController.Instance.Board.ResetAllProposedValueChanges();
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

    public void StartDraggingTile(NumberTile tile)
    {
        _rackTiles.Remove(tile);

        RefreshTilePositions();
    }

    public void FinishedDraggingTile(NumberTile tile)
    {
        SudokuBoard board = MatchController.Instance.Board;

        CellTile nearestCell = GridController.Instance.GetNearestCellTile(tile.transform.position, out float cellTileDistance);
        board.BaseState.GetValueAndColour(nearestCell.Position, out int value, out ColourState state);

        // You can't override already correct slots
        if (cellTileDistance > 0.25f ||
            (value != -1 && state != ColourState.INCORRECT))
        {
            GetNearestRackSlot(tile.transform.position, out int slotIndex);
            _rackTiles.Insert(slotIndex, tile);

            tile.SetScaleFactor(SortingLayer.RACK);

            RefreshTilePositions();

            return;
        }

        SudokuBoard.ProposedPlacements proposedPlacements = new SudokuBoard.ProposedPlacements(new Vector2Int(nearestCell.X, nearestCell.Y), tile.Value);
        board.AddProposedValueChange(proposedPlacements);

        tile.transform.position = (Vector2)nearestCell.transform.position;
        tile.SetScaleFactor(SortingLayer.BOARD);
    }
}

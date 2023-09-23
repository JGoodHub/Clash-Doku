using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RackController : SceneSingleton<RackController>
{

    [SerializeField] private GameObject _rackTilePrefab;
    [SerializeField] private RectTransform _fixedContainer;
    [SerializeField] private RectTransform _freeContainer;
    [Space]
    [SerializeField] private int _tilesCount = 7;
    [Space]
    [SerializeField] private Button _homeBtn;
    [SerializeField] private Button _resetTilesButton;
    [SerializeField] private Button _endTurnButton;

    private RackTile _freeTile;
    private List<RackTile> _rackTiles = new List<RackTile>();

    public List<RackTile> RackTiles => _rackTiles;

    private void Start()
    {
        foreach (Transform child in _fixedContainer.transform)
            Destroy(child.gameObject);

        PopulateRack();

        _freeTile = Instantiate(_rackTilePrefab, _freeContainer).GetComponent<RackTile>();
        _freeTile.SetState(0, false);
        _freeTile.gameObject.SetActive(false);

        _homeBtn.onClick.AddListener(GoToHomeScene);
        _endTurnButton.onClick.AddListener(() => MatchController.Instance.EndTurn());
        _resetTilesButton.onClick.AddListener(ResetPlacedTiles);

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

    private void PopulateRack()
    {
        Dictionary<int, int> missingNumberCounts = MatchController.Instance.Board.GetMissingNumberCounts();
        List<int> numberBag = new List<int>();

        foreach (int key in missingNumberCounts.Keys)
            for (int i = 0; i < missingNumberCounts[key]; i++)
                numberBag.Add(key);

        Random.InitState(MatchController.Instance.RoundSeed);
        for (int i = 0; i < numberBag.Count * 10; i++)
        {
            int indexA = Random.Range(0, numberBag.Count);
            int indexB = Random.Range(0, numberBag.Count);

            int temp = numberBag[indexA];
            numberBag[indexA] = numberBag[indexB];
            numberBag[indexB] = temp;
        }

        for (int i = _rackTiles.Count, j = 0; i < _tilesCount && j < numberBag.Count; i++, j++)
        {
            RackTile rackTile = Instantiate(_rackTilePrefab, _fixedContainer).GetComponent<RackTile>();

            rackTile.SetState(numberBag[j], true, false);

            _rackTiles.Add(rackTile);
        }
    }

    private void ResetPlacedTiles()
    {
        _freeTile.gameObject.SetActive(false);

        foreach (RackTile rackTile in _rackTiles)
        {
            rackTile.gameObject.SetActive(true);
        }

        MatchController.Instance.Board.ResetAllProposedValueChanges();
    }

    public RackTile GetNearestRackTile(Vector2 position, out float distance)
    {
        RackTile nearestTile = null;
        distance = float.MaxValue;

        foreach (RackTile rackTile in _rackTiles)
        {
            float tileDistance = Vector2.Distance(position, rackTile.transform.position);
            if (tileDistance < distance)
            {
                nearestTile = rackTile;
                distance = tileDistance;
            }
        }

        return nearestTile;
    }

    public void StartedDraggingTile(RackTile tile)
    {
        _freeTile.SetState(tile.Value);
    }

    public void DraggingTile(RackTile tile, Vector2 position)
    {
        _freeTile.transform.position = position;
    }

    public void FinishedDraggingTile(RackTile tile)
    {
        _freeTile.SetState(0, false);

        CellTile nearestCellTile = GridController.Instance.GetNearestCellTile(_freeTile.transform.position, out float cellTileDistance);

        SudokuBoard board = MatchController.Instance.Board;

        if (cellTileDistance < 0.25f)
        {
            board.BaseState.GetValueAndColour(nearestCellTile.Position, out int value, out CellTile.ColourState state);

            if (value == -1 || state == CellTile.ColourState.INCORRECT)
            {
                SudokuBoard.ProposedPlacements proposedPlacements = new SudokuBoard.ProposedPlacements(new Vector2Int(nearestCellTile.X, nearestCellTile.Y), tile.Value);
                board.AddProposedValueChange(proposedPlacements);

                tile.gameObject.SetActive(false);
                return;
            }
        }

        tile.gameObject.SetActive(true);
    }

    private void GoToHomeScene()
    {
        GameController.Instance.LoadHomeScene();
    }

}
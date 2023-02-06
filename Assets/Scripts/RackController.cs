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

        _endTurnButton.onClick.AddListener(() => GameController.Instance.EndTurn());
        _resetTilesButton.onClick.AddListener(ResetPlacedTiles);
    }

    private void PopulateRack()
    {
        for (int i = _rackTiles.Count; i < _tilesCount; i++)
        {
            RackTile rackTile = Instantiate(_rackTilePrefab, _fixedContainer).GetComponent<RackTile>();

            rackTile.SetState(Random.Range(1, 10), true, false);

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

        GameController.Instance.Board.ResetAllProposedValueChanges();
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

        SudokuBoard board = GameController.Instance.Board;

        if (cellTileDistance < 0.25f)
        {
            if (board.BaseState.Values[nearestCellTile.X, nearestCellTile.Y] == -1)
            {
                SudokuBoard.ProposedChange proposedChange = new SudokuBoard.ProposedChange(new Vector2Int(nearestCellTile.X, nearestCellTile.Y), tile.Value);
                board.AddProposedValueChange(proposedChange);

                tile.gameObject.SetActive(false);
                return;
            }
        }

        tile.gameObject.SetActive(true);
    }

}
using System.Collections.Generic;
using UnityEngine;

public class Region : MonoBehaviour
{

    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private RectTransform _cellContainer;

    public List<CellTile> GenerateCells(int xCorner, int yCorner)
    {
        foreach (Transform child in _cellContainer.transform)
            Destroy(child.gameObject);

        List<CellTile> cells = new List<CellTile>();

        for (int yOff = 0; yOff < 3; yOff++)
        {
            for (int xOff = 0; xOff < 3; xOff++)
            {
                GameObject cellObject = Instantiate(_cellPrefab, _cellContainer);
                CellTile cell = cellObject.GetComponent<CellTile>();

                cell.Initialise(xCorner + xOff, yCorner + yOff);
                cell.SetValue(-1);
                cells.Add(cell);
            }
        }

        return cells;
    }

}
using System.Collections.Generic;
using UnityEngine;

public class BoardRegion : MonoBehaviour
{
    [SerializeField] private List<BoardCell> _cells = new List<BoardCell>();

    public List<BoardCell> GenerateCells(int xCorner, int yCorner)
    {
        int absIndex = 0;

        for (int yOff = 0; yOff < 3; yOff++)
        {
            for (int xOff = 0; xOff < 3; xOff++)
            {
                _cells[absIndex].Initialise(xCorner + xOff, yCorner + yOff);
                _cells[absIndex].SetValue(-1);

                absIndex++;
            }
        }

        return _cells;
    }
}
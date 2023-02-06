using GoodHub.Core.Runtime;
using UnityEngine;

public class GameController : SceneSingleton<GameController>
{

    private SudokuBoard _board;

    public SudokuBoard Board => _board;

    private void Start()
    {
        _board = SudokuHelper.GenerateSudokuBoard();
        GridController.Instance.MirrorBoardData(_board);
    }

    public void EndTurn()
    {
        _board.CommitProposedChanges();
    }

}
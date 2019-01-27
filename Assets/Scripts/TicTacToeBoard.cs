using UnityEngine;
using UnityEngine.UI;

namespace SCMTicTacToe
{
    /// <summary>
    /// All available Board Sizes.
    /// </summary>
    public enum BOARDSIZE
    {
        SIZE3X3,
        SIZE4X4
    }

    /// <summary>
    /// Definition of the Tic Tac Toe Board
    /// </summary>
    [System.Serializable]
    public class TicTacToeBoard
    {
        #region GAME BOARD VARIABLES
        /// <summary>
        /// Game Board is made of this.
        /// </summary>
        public BoardTile tilePrefab = null;

        /// <summary>
        /// Board Size definition.
        /// </summary>
        public BOARDSIZE BType { get; set; }

        /// <summary>
        /// 2 Dimentional Array which contains the Game Board
        /// </summary>
        public BoardTile[,] TileBoard { get; private set; }

        /// <summary>
        /// This is where the Board Pieces will be Placed.
        /// </summary>
        public RectTransform TilesParent { get; private set; }

        /// <summary>
        /// Grid Layout Group Containing our Board Pieces, needs to be setup according to size.
        /// </summary>
        public GridLayoutGroup TileBoardGrid { get; private set; }

        /// <summary>
        /// Player movements during the current game session.
        /// </summary>
        public int MoveCount { get; private set; }

        /// <summary>
        /// Current Board Size on int value
        /// </summary>
        public int Size { get; private set; }
        #endregion

        #region GAME BOARD INITIALIZATION

        /// <summary>
        /// Used to Initialize the Game Board
        /// </summary>
        /// <param name="Initializer">Where can the Board Tiles Parent be found?</param>
        public void Initialize(Transform Initializer)
        {
            TilesParent = Initializer.Find("TilesP").transform as RectTransform;

            if (TilesParent) { TileBoardGrid = TilesParent.GetComponent<GridLayoutGroup>(); }
        }
        #endregion

        #region GAMEBOARD RELATED METHODS

        /// <summary>
        /// Deactivates all of the Game Board Pieces.
        /// </summary>
        public void DeactivateGameBoard()
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    TileBoard[i, j].PButton.interactable = false;
                }
            }
        }

        /// <summary>
        /// Reset all the Board Tiles State, and terminates the Game Session.
        /// </summary>
        public void ResetGameBoard()
        {
            GameManager.Instance.GameSession++;
            MoveCount = 0;

            foreach (BoardTile B in TileBoard)
            {
                B.ResetBoardTile();
            }
        }

        /// <summary>
        /// Removes all of the Game Board Pieces in case the space is needed.
        /// </summary>
        public void ClearBoard()
        {
            if (TileBoard == null || TileBoard.Length <= 0) { return; }

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    MonoBehaviour.Destroy(TileBoard[i, j].gameObject);
                    TileBoard[i, j] = null;
                }
            }
        }

        /// <summary>
        /// Builds a new Game Board if the Size provided is different than the current existing Size
        /// </summary>
        public void BuildBoard()
        {
            // Same Size Board, No Need to build another one
            if (/*TileBoard != null &&*/ (BType == BOARDSIZE.SIZE3X3? 3: 4) == Size) { return; }

            ClearBoard();

            if (!TilesParent.gameObject.activeInHierarchy) { TilesParent.gameObject.SetActive(true); }

            Size = BType == BOARDSIZE.SIZE3X3 ? 3 : 4;

            GameManager.Instance.GameSession++;
            // Since we are only using 2 different Board Sizes

            TileBoardGrid.constraintCount = Size;

            TileBoard = new BoardTile[Size, Size];

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    BoardTile tile = UnityEngine.MonoBehaviour.Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, TilesParent);

                    tile.GridPos = new Vector2(i, j);

                    TileBoard[i, j] = tile;

                    tile.transform.localScale = Vector3.one;

                    tile.name = ("X = " + i + " Y =" + j);
                }
            }
        }
        #endregion

        #region WIN CONDITION CHECKS
        /// <summary>
        /// Checks the Game Board for a Win according to the Player decision
        /// </summary>
        /// <param name="X">Row affected by the Player action</param>
        /// <param name="Y">Column affected by the Player action</param>
        public void CheckGameBoard(int X, int Y)
        {
            MoveCount++;

            CheckRowOrColumn(X, Y);

            CheckRowOrColumn(X, Y, false);

            CheckDiagonal(X, Y);

            CheckDiagonal(X, Y, false);

            if (MoveCount == (Mathf.Pow(Size, 2)))
            {
                GameManager.Instance.GameHasEnded(true);
                GameManager.Instance.ModifyMovementHistoryOnGameOver(X, Y);
            }
        }

        /// <summary>
        /// Checks the Board or Column for a win according to the player input.
        /// </summary>
        /// <param name="X">Row affected by the Player action</param>
        /// <param name="Y">Column affected by the Player action</param>
        /// <param name="Row">Is it a Row? Or a Column?</param>
        public void CheckRowOrColumn(int x, int y, bool Row = true)
        {
            for (int i = 0; i < Size; i++)
            {
                if (TileBoard[Row == true ? i : x, Row == true ? y : i].MyPlayer != GameManager.Instance.CurPlayer)
                {
                    break;
                }
                if (i == Size - 1)
                {
                    GameManager.Instance.GameHasEnded();
                    GameManager.Instance.ModifyMovementHistoryOnGameOver(x, y, Row ==  true ? "Row" : "Column");
                    return;
                }
            }
        }

        /// <summary>
        /// Checks the Board Diagonally for a win according to the player input.
        /// </summary>
        /// <param name="X">Row affected by the Player action</param>
        /// <param name="Y">Column affected by the Player action</param>
        /// <param name="NotInverse">Is it a Diagonal? Or an Inverse Diagonal?</param>
        public void CheckDiagonal(int x, int y, bool NotInverse = true)
        {
            if (NotInverse == true ? x == y : x + y == Size - 1)
            {
                for (int i = 0; i < Size; i++)
                {
                    if (TileBoard[i, NotInverse == true ? i : (Size - 1) - i].MyPlayer != GameManager.Instance.CurPlayer)
                    {
                        break;
                    }

                    if (i == Size - 1)
                    {
                        GameManager.Instance.GameHasEnded();
                        GameManager.Instance.ModifyMovementHistoryOnGameOver(x, y, NotInverse == true ? "Diagonal" : "Inverse Diagonal");
                        return;
                    }
                }
            }
        }
        #endregion
    }
}
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SCMTicTacToe
{
    /// <summary>
    /// Contains all the neccesary information each player needs to be visualy identified on the game.
    /// </summary>
    [Serializable]
    public class Player
    {
        /// <summary>
        /// UI Image representing each player unique icon.
        /// </summary>
        [Tooltip("UI Image representing each player unique icon. Drag the Img here if Project Settings haven't been imported")]
        public Image IconHolder = null;

        /// <summary>
        /// Player unique Icon displayed on UI and In-Game.
        /// </summary>
        public Sprite Icon { get; private set; }

        /// <summary>
        /// Player current Icon ID number, used to decide what icon will be assigned to the player.
        /// </summary>
        public int CurINumb { get; private set; }

        /// <summary>
        /// Used to Initialize the Player Variables and References.
        /// </summary>
        /// <param name="Name">Name of the Icon Holder in the hierarchy in case it has not been assigned in the inspector.</param>
        /// <param name="CurINumb">Player Starting Icon ID number.</param>
        public void Initialize(string Name, int CurINumb)
        {
            if (!IconHolder) { IconHolder = GameObject.FindGameObjectWithTag(Name).GetComponent<Image>(); }
            this.CurINumb = CurINumb;
        }

        /// <summary>
        /// Used to increase the Player Icon ID number. This will continue until a new unique number is provided,
        /// thus players will always have a different Icon ID number.
        /// </summary>
        public void NextIcon()
        {
            CurINumb = CurINumb + 1 < GameManager.Instance.PossibleIcons.Length ?  CurINumb + 1 : 0;

            while (GameManager.Instance.Players[0].CurINumb == GameManager.Instance.Players[1].CurINumb)
            {
                NextIcon();
            }

            SetIconHolder(GameManager.Instance.PossibleIcons[CurINumb]);
        }

        /// <summary>
        /// Used to decrease the Player Icon ID number. This will continue until a new unique number is provided,
        /// thus players will always have a different Icon ID number.
        /// </summary>
        public void PreviousIcon()
        {
            CurINumb = CurINumb - 1 < 0 ? 3 : CurINumb - 1;

            while (GameManager.Instance.Players[0].CurINumb == GameManager.Instance.Players[1].CurINumb)
            {
                PreviousIcon();
            }

            SetIconHolder(GameManager.Instance.PossibleIcons[CurINumb]);
        }

        /// <summary>
        /// Setup the in-game Player Unique Icon.
        /// </summary>
        /// <param name="Target">Sprite value to pass.</param>
        public void SetIcon(Sprite Target)
        {
             Icon = Target;
        }

        /// <summary>
        /// Setup the UI Player Unique Icon.
        /// </summary>
        /// <param name="Target">Sprite value to pass.</param>
        public void SetIconHolder(Sprite Target)
        {
            IconHolder.sprite = Target;
        }
    }

    /// <summary>
    /// Static class containing readonly values of objects and interactions neccessary for gameplay.
    /// </summary>
    // Stored in this area so it can be easily access and modified.
    public static class GameTextValues
    {
        public static readonly string PLAYERWON = " HAS WON THE GAME!";
        public static readonly string DRAW = "IT'S A DRAW!";
        public static readonly string GAMESESSION = "GAME SESSION #";
        public static readonly string SESSIONID = "Session";
        public static readonly string CPLAYERICONIMG = "CurPlayer";
        public static readonly string GAMETEXTUGUI = "CurPlayer";
        public static readonly string PLAYER1ICONH = "Player 1 Icon Holder";
        public static readonly string PLAYER2ICONH = "Player 2 Icon Holder";
    }

    /// <summary>
    /// Manages GameBoard and Player Input interactions along with the Player Visual aspect<UI>.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region  GAME MANAGER VARIABLES
        /// <summary>
        /// Game Manager Singleton Instance, for easy accessibility.
        /// </summary>
        public static GameManager Instance;

        /// <summary>
        /// Speed at which Game Text will fade in after reaching Game Over state.
        /// </summary>
        [Tooltip("Speed at which Game Text will fade in after reaching Game Over state.")]
        [Range(0.25f, 1.5f)]
        public float TextFadeSpeed = 1f;

        /// <summary>
        /// Parent of all Gameplay, this canvas contains all of the Gameplay Elements.
        /// </summary>
        [Tooltip("Parent of all Gameplay, this canvas contains all of the Gameplay Elements.")]
        public Canvas gameCanvas;

        /// <summary>
        /// Simple UGUI Text used to provide Player with Game State Info.
        /// Using Text Mesh Pro because its better than UText. 
        /// </summary>
        [Tooltip("Simple UGUI Text used to provide Player with Game State Info.")]
        public TextMeshProUGUI gameText;

        /// <summary>
        /// Parent of UGUI Text Game Text. Used to control its Alpha and other related values.
        /// </summary>
        public CanvasGroup GameTextCG { get; private set; }

        /// <summary>
        /// Image used to display which player should make the next move.
        /// </summary>
        public Image CurPlayerIMG { get; private set; }

        /// <summary>
        /// Array of In-Game Players, 2 Players for Tic Tac Toe.
        /// </summary>
        [Tooltip("Array of In-Game Players, 2 Players for Tic Tac Toe.")]
        public Player[] Players = new Player[2];

        /// <summary>
        /// Array of all possible Icons players can choose from. This provides a more solid gameplay experience.
        /// </summary>
        [Tooltip("Provide any icon you would like players to use as in-game pieces.")]
        public Sprite[] PossibleIcons;

        /// <summary>
        /// In-Game Tic Tac Toe Board. Contains all information related to Board Interactions.
        /// </summary>
        [Tooltip("Contains all information related to Board Interactions.")]
        public TicTacToeBoard GameBoard;

        /// <summary>
        /// List of strings describing in detail each player interaction from beginning to end.
        /// </summary>
        List<string> movementHistory = new List<string>();

        /// <summary>
        /// Current # Game Session being played on the machine.
        /// </summary>
        public int GameSession { get; set; }

        /// <summary>
        /// Int definning which Player will make the next move.
        /// </summary>
        private int curPlayer = 1;

        public int CurPlayer { get { return curPlayer; } }

        /// <summary>
        /// Have we reached the Game Over state already? 
        /// </summary>
        private bool gameOver = false;
        #endregion

        #region INITIALIZATION
        /// <summary>
        /// Unity Initialization method this is where all starting variables and references should be set.
        /// </summary>
        private void Awake()
        {
            Initialize();
            GameBoard.Initialize(gameCanvas.transform);
        }

        /// <summary>
        /// Game Manager Initialization method, Game Manager and Players are initialized here!
        /// </summary>
        public void Initialize()
        {
            if (!Instance) { Instance = this; }
            else if (Instance != this) { Destroy(gameObject); Debug.LogWarning("Game Manager Instance Failed to Instantiate, Make Sure There is Only One Game Manager on the Scene"); }
            if (!gameCanvas) { gameCanvas = FindObjectOfType<Canvas>(); }
            if (!gameText) { gameText = gameCanvas.transform.Find(GameTextValues.GAMETEXTUGUI).GetComponent<TextMeshProUGUI>(); }
            if (gameText) { GameTextCG = gameText.GetComponent<CanvasGroup>(); }
            if (!CurPlayerIMG) { CurPlayerIMG = gameCanvas.transform.Find(GameTextValues.CPLAYERICONIMG).GetChild(0).GetComponent<Image>(); }
            GameSession = PlayerPrefs.GetInt(GameTextValues.SESSIONID, 0);
            Players[0].Initialize(GameTextValues.PLAYER1ICONH, 0);
            Players[1].Initialize(GameTextValues.PLAYER2ICONH, 1);
        }
        #endregion

        #region GAME RELATED METHODS

        /// <summary>
        /// Method used Start the Game
        /// </summary>
        public void StartGame()
        {
            ResetGame();
            GameBoard.BuildBoard();
            SetPlayerIcons();
        }

        /// <summary>
        /// Mehotd used to assing new Icons to each player
        /// </summary>
        public void SetPlayerIcons()
        {
            foreach (Player P in Players)
            {
                if (P.Icon == PossibleIcons[P.CurINumb]) { continue; }

                P.SetIcon(PossibleIcons[P.CurINumb]);
            }

            SetPlayerUGUIIcon();
        }

        /// <summary>
        /// Method used show which player will take the next turn.
        /// </summary>
        /// <param name="x">Define which player will be next, default player 1</param>
        private void SetPlayerUGUIIcon()
        {
            CurPlayerIMG.sprite = Players[curPlayer].Icon;
        }

        /// <summary>
        /// Method used to reset the Game to a Clean State
        /// </summary>
        public void ResetGame()
        {
            if (GameBoard.MoveCount == 0) { return; }

            else if (GameBoard.MoveCount > 0 && !gameOver) { ModifyMovementHistoryOnReset(); }

            StopAllCoroutines();
            gameOver = false;
            curPlayer = 1;
            GameTextCG.alpha = 0;
            GameBoard.ResetGameBoard();
            SetPlayerIcons();
            movementHistory.Clear();
        }

        /// <summary>
        /// Method used to define the Game Over State.
        /// </summary>
        /// <param name="Draw">Was the game a Draw/Tie?</param>
        public void GameHasEnded(bool Draw = false)
        {
            if (gameOver) { return; }
            gameOver = true;
            GameBoard.DeactivateGameBoard();
            gameText.text = Draw == false ? "PLAYER " + (curPlayer + 1) + GameTextValues.PLAYERWON : GameTextValues.DRAW;
            FadeText();
        }

        /// <summary>
        /// A player has made a move, used to trigger check of game information.
        /// </summary>
        /// <param name="B">Which board tile triggered on the action?</param>
        /// <returns>Returns the corresponding Player Icon to display</returns>
        public Sprite PlayerAction(BoardTile B)
        {
            SetPlayerUGUIIcon();
            curPlayer = curPlayer == 1 ? 0 : 1;
            B.MyPlayer = curPlayer;

            int c = (int)B.GridPos.x;
            int r = (int)B.GridPos.y;

            GameBoard.CheckGameBoard(c, r, curPlayer);
            ModifyMovementHistory(c, r);

            return Players[curPlayer].Icon;
        }
        #endregion

        #region  UI RELATED METHODS

        /// <summary>
        /// Start Fading-In Text Effect
        /// </summary>
        private void FadeText()
        {
            StartCoroutine(IFadeText());
        }

        /// <summary>
        /// Handles the Fading Text Effect
        /// </summary>
        private IEnumerator IFadeText()
        {
            while (GameTextCG.alpha < 1)
            {
                float rate = TextFadeSpeed * Time.deltaTime;
                GameTextCG.alpha += rate;
                yield return null;
            }
        }

        /// <summary>
        /// Used to Setup the Board Size on the UI according to provided Input
        /// </summary>
        /// <param name="Size">Value passed, Size will be defined by a condition </param>
        public void SetBoardSize(int Size = 3)
        {
            // Doing this assures the Board Size will always stay within range regarding of the provided input.
            GameBoard.BType = Size <= 3 ? BOARDSIZE.SIZE3X3 : BOARDSIZE.SIZE4X4;
        }

        /// <summary>
        /// Triggers the Selection of a new Icon for a Player
        /// </summary>
        /// <param name="Tplayer">Player whose Icon should be modified</param>
        public void SelectNextIcon(int Tplayer)
        {
            Players[Tplayer].NextIcon();
        }

        /// <summary>
        /// Triggers the Selection of a new Icon for a Player
        /// </summary>
        /// <param name="Tplayer">Player whose Icon should be modified</param>
        public void SelectPreviousIcon(int Tplayer)
        {
            Players[Tplayer].PreviousIcon();
        }
        #endregion


        #region MOVEMENT HISTORY STORAGE

        /// <summary>
        /// Modifies the movement history, and Starts a new Game Session if necessary
        /// </summary>
        /// <param name="c">Column affected by Player action</param>
        /// <param name="r">Row affected by Player action</param>
        private void ModifyMovementHistory(int c, int r)
        {
            if (movementHistory.Count == 0)
            {
                movementHistory.Add("Game Session #" + GameSession + ",  Size: " + GameBoard.Size + "x" + GameBoard.Size + " Has Started");
            }

            movementHistory.Add("Player " + (curPlayer + 1) + " Placed a Piece on Row = " + (r + 1) + ", Column = " + (c + 1));
        }

        /// <summary>
        /// Modifies the movement history, and finishes the game history it according to the last player action.
        /// </summary>
        /// <param name="c">Column affected by Player action</param>
        /// <param name="r">Row affected by Player action</param>
        /// <param name="WinType">What Type of Win happened?</param>
        public void ModifyMovementHistoryOnGameOver(int c, int r, string WinType)
        {
            movementHistory.Add("Player " + (curPlayer + 1) + " Placed a Piece on Row = " + (r + 1) + ", Column = " + (c + 1) + " and Won the Game with a " + WinType);
            SaveMovementHistory();
        }

        /// <summary>
        /// Modifies the movement history, and finishes the game history on a tie.
        /// </summary>
        /// <param name="c">c affected by Player action</param>
        /// <param name="r">r affected by Player action</param>
        public void ModifyMovementHistoryOnGameOver(int c, int r)
        {
            movementHistory.Add("Player " + (curPlayer + 1) + " Placed a Piece on Row = " + (r + 1) + ", Column = " + (c + 1) + ". The Game Ended in a Tie.");
            SaveMovementHistory();
        }

        /// <summary>
        /// Modifies the movement history when a Reset Game happens.
        /// </summary>
        public void ModifyMovementHistoryOnReset()
        {
            movementHistory.Add("Player " + (curPlayer + 1) + " Reset the Game");
            SaveMovementHistory();
        }

        /// <summary>
        /// Saves the Movement History to a JSon file on the Assets/Game Sessions History folder when a game session ends.
        /// </summary>
        private void SaveMovementHistory()
        {
            string path = null;
#   if UNITY_EDITOR
            path = "Assets/Game Sessions History/GameSession#" + GameSession + ".json";

#endif

#if !UNITY_EDITOR            
            path = Application.persistentDataPath + "GameSession#" + GameSession + ".json";
#endif

            string MovementHistoryToJSON = JsonHelper.ToJson(movementHistory);
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    writer.Write(MovementHistoryToJSON);
                    writer.Close();
                }
            }

            PlayerPrefs.SetInt(GameTextValues.SESSIONID, GameSession);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Deletes All Player Preferences.
        /// Used on Context Menu for Debugging Purposes 
        /// </summary>
        [ContextMenu("Delete Data")]
        public void DeleteData()
        {
            PlayerPrefs.DeleteAll();
        }
        #endregion
    }
}

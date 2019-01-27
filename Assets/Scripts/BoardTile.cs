using UnityEngine;
using UnityEngine.UI;

namespace SCMTicTacToe
{
    /// <summary>
    /// Board Tile Data and Variables
    /// </summary>
    public class BoardTile : MonoBehaviour
    {
        #region VARIABLES
        /// <summary>
        /// Player who made an action on this Tile
        /// </summary>
        public int MyPlayer = -1;

        /// <summary>
        /// Image to hold Icon of the player who triggered this Tile
        /// </summary>
        public Image PIcon { get; set; }

        /// <summary>
        /// Button which handles the Player input on this Tile.
        /// </summary>
        public Button PButton { get; private set; }

        /// <summary>
        /// Individual Tile Position on the 2 Dimentional Array of the Board
        /// </summary>
        public Vector2 GridPos { get; set; }
        #endregion

        #region INITIALIZATION
        private void Awake()
        {
            PIcon = transform.GetChild(0).GetComponent<Image>();
            PButton = GetComponent<Button>();
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Handle Player Input
        /// </summary>
        public void UIPlayerClicked()
        {
            PIcon.gameObject.SetActive(true);
            PButton.interactable = false;
            PIcon.sprite = GameManager.Instance.PlayerAction(this);
        }

        /// <summary>
        /// Reset this Board to its original state before any Input
        /// </summary>
        public void ResetBoardTile()
        {
            PIcon.gameObject.SetActive(false);
            PIcon.sprite = null;
            PButton.interactable = true;
            MyPlayer = -1;
        }
        #endregion
    }
}

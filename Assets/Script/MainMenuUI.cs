// Author: Sebuloan Johnson
// Description: Controls the main menu UI, including new game, continue, and difficulty selection.

using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatchGame
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button continueButton;
        public Button easyButton;
        public Button mediumButton;
        public Button hardButton;

        [Header("Scene References")]
        public GameObject difficultySelectionPanel;
        public GameObject mainButtonsPanel;

        #region Unity Methods

        // Called on startup. Initializes button states and assigns listeners.
        private void Start()
        {
            continueButton.gameObject.SetActive(GameManager.Instance.HasSavedGame);

            continueButton.onClick.AddListener(OnContinueClicked);
            easyButton.onClick.AddListener(() => OnNewGameClicked(Difficulty.Easy));
            mediumButton.onClick.AddListener(() => OnNewGameClicked(Difficulty.Medium));
            hardButton.onClick.AddListener(() => OnNewGameClicked(Difficulty.Hard));

            GameManager.Instance.OnReturnToModeSelect += UpdateContinueButton;

            difficultySelectionPanel.SetActive(false);
            mainButtonsPanel.SetActive(true);
        }

        #endregion

        #region Private Methods

        // Updates the visibility of the Continue button based on saved data availability.
        private void UpdateContinueButton()
        {
            continueButton.gameObject.SetActive(GameManager.Instance.HasSavedGame);
        }

        #endregion

        #region Public Methods

        // Displays the difficulty selection panel.
        public void OnNewGameButtonClicked()
        {
            ShowDifficultySelection();
        }

        // Called when selecting difficulty via inspector binding.
        public void OnDifficultySelected(int difficulty)
        {
            GameManager.Instance.StartNewGame((Difficulty)difficulty);
        }

        // Switches UI to show difficulty selection.
        public void ShowDifficultySelection()
        {
            mainButtonsPanel.SetActive(false);
            difficultySelectionPanel.SetActive(true);
        }

        // Starts a new game for the selected difficulty.
        public void OnNewGameClicked(Difficulty difficulty)
        {
            GameManager.Instance.StartNewGame(difficulty);
        }

        // Resumes an existing saved game.
        public void OnContinueClicked()
        {
            GameManager.Instance.ResumeGame();
        }

        // Quits the application.
        public void OnQuitClicked()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #endregion
    }
}

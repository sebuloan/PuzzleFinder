// Author: Sebuloan Johnson
// Description: Manages all UI updates such as score, high score, pairs, and completion text.

using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatchGame
{
    public class UIManager : MonoBehaviour
    {
        // UI References
        public Text scoreText;
        public Text highScoreText;
        public Text pairsText;
        public GameObject completedText;

        #region Unity Methods

        // Called once when the script starts
        private void Start()
        {
            InitializeUI();
        }

        // Re-initializes UI when re-enabled
        private void OnEnable()
        {
            if (GameManager.Instance != null)
                InitializeUI();
        }

        // Clean up event subscriptions
        private void OnDisable()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnScoreUpdated -= UpdateScore;
            GameManager.Instance.OnHighScoreUpdated -= UpdateHighScore;
            GameManager.Instance.OnPairsUpdated -= UpdatePairs;
            GameManager.Instance.OnGameCompleted -= ShowCompletedText;
        }

        #endregion

        #region Public Methods

        // Hides the completed text UI
        public void HideCompletedText()
        {
            if (completedText != null)
                completedText.SetActive(false);
        }

        #endregion

        #region Private Methods

        // Sets up UI listeners and initial values
        private void InitializeUI()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.OnScoreUpdated += UpdateScore;
            GameManager.Instance.OnHighScoreUpdated += UpdateHighScore;
            GameManager.Instance.OnPairsUpdated += UpdatePairs;
            GameManager.Instance.OnGameCompleted += ShowCompletedText;

            UpdateScore(0);
            UpdateHighScore(GameManager.Instance.GetHighScore());
            UpdatePairs(GameManager.Instance.GetMatchedPairs(), GameManager.Instance.GetTotalPairs());

            if (completedText != null)
                completedText.SetActive(false);
        }

        // Shows the completed message when game ends
        private void ShowCompletedText()
        {
            if (completedText != null)
                completedText.SetActive(true);
        }

        // Updates the score text
        private void UpdateScore(int score)
        {
            if (scoreText != null)
                scoreText.text = "Score: " + score;
        }

        // Updates the high score text
        private void UpdateHighScore(int highScore)
        {
            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore;
        }

        // Updates the pairs progress text
        private void UpdatePairs(int matched, int total)
        {
            if (pairsText != null)
                pairsText.text = "Pairs: " + matched + "/" + total;
        }

        #endregion
    }
}

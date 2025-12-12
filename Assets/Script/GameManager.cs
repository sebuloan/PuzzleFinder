using MemoryMatchGame; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameState
{
    public int score;
    public int pairsMatched;
    public int totalPairs;
    public List<string> matchedCardIDs = new List<string>();
    public List<string> remainingCardIDs = new List<string>();
    public List<Vector2> cardPositions = new List<Vector2>(); // New: Track card positions
    public Difficulty difficulty;
    public bool allCardsMatched;

}

public enum Difficulty
{
    Easy,
    Medium,
    Hard
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private List<Card> flippedCards = new List<Card>();
    private bool canFlip = true;
    private int pairsMatched = 0;
    private int totalPairs = 0;
    private int score = 0;
    private int highScore = 0;
    private Coroutine autoFlipCoroutine;
    private GameState currentGameState;
    private const string GameStateKey = "MemoryMatchGameState";
    public bool HasSavedGame { get; private set; }

    // Sound effects
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioClip victorySound;
    private AudioSource audioSource;

    public System.Action<int, int> OnPairsUpdated;
    public System.Action<int> OnScoreUpdated;
    public System.Action<int> OnHighScoreUpdated;
    public System.Action OnGameSaved;
    public System.Action OnGameLoaded;
    public System.Action OnReturnToModeSelect;
    public System.Action OnGameCompleted;
    public int GetTotalPairs() => totalPairs;
    public int GetMatchedPairs() => pairsMatched;


    private const string HighScoreKey = "MemoryMatchHighScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0;

        LoadHighScore();
        CheckForSavedGame();
    }

    private void LoadHighScore()
    {
        string key = GetHighScoreKey();
        highScore = PlayerPrefs.GetInt(key, 0);
        OnHighScoreUpdated?.Invoke(highScore);
    }


    private void SaveHighScore()
    {
        string key = GetHighScoreKey();
        PlayerPrefs.SetInt(key, highScore);
        PlayerPrefs.Save();
    }


    private void CheckForSavedGame()
    {
        HasSavedGame = PlayerPrefs.HasKey(GameStateKey);
    }

    public void SaveGameState()
    {
        if (CardSpawner.Instance == null || pairsMatched >= totalPairs)
        {
            ClearSavedGame();
            return;
        }

        currentGameState = new GameState
        {
            score = score,
            pairsMatched = pairsMatched,
            totalPairs = totalPairs,
            difficulty = CardSpawner.Instance.selectedDifficulty,
            allCardsMatched = false
        };

        List<Card> allCards = CardSpawner.Instance.GetAllCards();
        List<Vector2> allPositions = CardSpawner.Instance.GetAllCardPositions();

        int positionIndex = 0;
        foreach (Card card in allCards)
        {
            if (card == null) continue;

            // Record position for all cards
            if (positionIndex < allPositions.Count)
            {
                currentGameState.cardPositions.Add(allPositions[positionIndex]);
                positionIndex++;
            }

            if (card.IsMatched)
            {
                currentGameState.matchedCardIDs.Add(card.cardID);
            }
            else
            {
                currentGameState.remainingCardIDs.Add(card.cardID);
            }
        }

        string json = JsonUtility.ToJson(currentGameState);
        PlayerPrefs.SetString(GameStateKey, json);
        PlayerPrefs.Save();
        HasSavedGame = true;

        OnGameSaved?.Invoke();
    }

    public GameState LoadGameState()
    {
        if (!PlayerPrefs.HasKey(GameStateKey)) return null;

        string json = PlayerPrefs.GetString(GameStateKey);
        currentGameState = JsonUtility.FromJson<GameState>(json);

        if (currentGameState.allCardsMatched)
        {
            ClearSavedGame();
            return null;
        }

        HasSavedGame = true;
        score = currentGameState.score;
        pairsMatched = currentGameState.pairsMatched;
        totalPairs = currentGameState.totalPairs;

        OnScoreUpdated?.Invoke(score);
        OnPairsUpdated?.Invoke(pairsMatched, totalPairs);
        OnGameLoaded?.Invoke();

        Debug.Log("Game state loaded");
        return currentGameState;
    }

    public void ClearSavedGame()
    {
        PlayerPrefs.DeleteKey(GameStateKey);
        PlayerPrefs.Save();
        HasSavedGame = false;
        Debug.Log("Saved game cleared");
    }

    public void SetTotalPairs(int pairs)
    {
        totalPairs = pairs;
        OnPairsUpdated?.Invoke(pairsMatched, totalPairs);
    }

    public void CardFlipped(Card card)
    {
        if (!canFlip || flippedCards.Contains(card)) return;

        PlaySound(flipSound);
        flippedCards.Add(card);

        if (flippedCards.Count > 2)
        {
            Card oldestCard = flippedCards[0];
            oldestCard.FlipBack();
            flippedCards.Remove(oldestCard);
        }

        if (flippedCards.Count == 2)
        {
            CheckMatch();
        }

        if (autoFlipCoroutine != null)
        {
            StopCoroutine(autoFlipCoroutine);
        }
        autoFlipCoroutine = StartCoroutine(AutoFlipUnmatchedCards());
    }

    private IEnumerator AutoFlipUnmatchedCards()
    {
        yield return new WaitForSeconds(1f);

        if (flippedCards.Count > 0 && !CheckForMatch(flippedCards))
        {
            PlaySound(mismatchSound);
            foreach (Card card in flippedCards.ToArray())
            {
                card.FlipBack();
                flippedCards.Remove(card);
            }
        }
    }

    private void CheckMatch()
    {
        if (flippedCards.Count < 2) return;

        bool isMatch = flippedCards[0] != null &&
                      flippedCards[1] != null &&
                      flippedCards[0].cardID == flippedCards[1].cardID;

        if (isMatch)
        {
            PlaySound(matchSound);
            score += 100 * (flippedCards.Count + 1);
            OnScoreUpdated?.Invoke(score);

            foreach (Card card in flippedCards)
            {
                if (card != null)
                {
                    card.MarkAsMatched();
                }
            }

            foreach (Card card in flippedCards)
            {
                if (card != null)
                {
                    card.gameObject.SetActive(false);
                }
            }

            pairsMatched++;
            OnPairsUpdated?.Invoke(pairsMatched, totalPairs);
            SaveGameState();

            if (pairsMatched >= totalPairs)
            {
                GameOver();
            }
            flippedCards.Clear();
        }
    }

    private void GameOver()
    {
        PlaySound(victorySound);

        // Mark game state completed
        currentGameState.allCardsMatched = true;
        SaveGameState();

        // Update High Score
        if (score > highScore)
        {
            highScore = score;
            SaveHighScore();
            OnHighScoreUpdated?.Invoke(highScore);
        }

        // Trigger UI to show "Completed" message
        OnGameCompleted?.Invoke();

        StartCoroutine(ReturnToModeSelectAfterDelay(2f));
    }


    private IEnumerator ReturnToModeSelectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnReturnToModeSelect?.Invoke();
        ClearSavedGame();
    }

    public void QuitGame()
    {
        if (pairsMatched < totalPairs)
        {
            SaveGameState();
        }
        else
        {
            ClearSavedGame();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void StartNewGame(Difficulty difficulty)
    {
        ClearSavedGame();
        CardSpawner.Instance.selectedDifficulty = difficulty;
        CardSpawner.Instance.SpawnCardsWrapper(false);
    }

    public void ResumeGame()
    {
        GameState savedState = LoadGameState();
        if (savedState == null || savedState.allCardsMatched)
        {
            OnReturnToModeSelect?.Invoke();
            return;
        }
        CardSpawner.Instance.SpawnCardsWrapper(true);
    }
    public void GoHome()
    {
        SaveGameState();

        if (CardSpawner.Instance != null)
            CardSpawner.Instance.ClearExistingCards();

        score = 0;
        pairsMatched = 0;

        OnScoreUpdated?.Invoke(0);
        OnPairsUpdated?.Invoke(0, totalPairs);

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.HideCompletedText();

        OnReturnToModeSelect?.Invoke();
    }


    private bool CheckForMatch(List<Card> cards)
    {
        if (cards.Count < 2) return false;
        return cards[0].cardID == cards[1].cardID;
    }

    public bool CanFlipCard()
    {
        return canFlip;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public int GetHighScore()
    {
        string key = GetHighScoreKey();
        return PlayerPrefs.GetInt(key, 0);
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Clear All PlayerPrefs")]
    private static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs cleared");
    }
#endif
    public void UpdatePairs(int matched)
    {
        pairsMatched = matched;
        OnPairsUpdated?.Invoke(matched, totalPairs);
    }


    private string GetHighScoreKey()
    {
        if (CardSpawner.Instance == null)
            return "HighScore_Easy"; // fallback if spawner isn't initialized

        switch (CardSpawner.Instance.selectedDifficulty)
        {
            case Difficulty.Easy: return "HighScore_Easy";
            case Difficulty.Medium: return "HighScore_Medium";
            case Difficulty.Hard: return "HighScore_Hard";
        }
        return "HighScore_Easy";
    }

}
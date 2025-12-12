// Author: Sebuloan Johnson
// Description: Handles card spawning, layout setup, saved-game restoration, and card clearing.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatchGame
{
    public class CardSpawner : MonoBehaviour
    {
        public static CardSpawner Instance;

        public GameObject cardPrefab;
        public GameObject matchedCardPrefab;
        public Transform parentTransform;
        public List<Sprite> availableSprites;
        public Difficulty selectedDifficulty = Difficulty.Easy;

        private GridLayoutGroup gridLayout;
        private int rows;
        private int columns;
        private Dictionary<string, Sprite> spriteDictionary = new Dictionary<string, Sprite>();

        #region Unity Methods

        // Initializes instance and grid settings, builds sprite dictionary.
        private void Awake()
        {
            Instance = this;

            gridLayout = parentTransform.GetComponent<GridLayoutGroup>();
            if (gridLayout == null)
            {
                gridLayout = parentTransform.gameObject.AddComponent<GridLayoutGroup>();
            }

            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

            foreach (Sprite sprite in availableSprites)
            {
                spriteDictionary[sprite.name] = sprite;
            }
        }

        #endregion

        #region Public Methods

        // Wrapper to spawn cards, with an option to load previous game state.
        public void SpawnCardsWrapper(bool loadGame = false)
        {
            ClearExistingCards();
            SetLayoutByDifficulty();

            if (!loadGame)
            {
                int requiredPairs = (rows * columns) / 2;

                if (availableSprites.Count < requiredPairs)
                {
                    Debug.LogError("Not enough sprites available to generate card pairs.");
                    return;
                }

                SpawnCards();
            }
            else
            {
                SpawnCardsFromSavedGame();
            }
        }

        // Clears all existing card objects in the parent container.
        public void ClearExistingCards()
        {
            foreach (Transform child in parentTransform)
            {
                Destroy(child.gameObject);
            }
        }

        // Returns a list of all card components currently in the scene.
        public List<Card> GetAllCards()
        {
            List<Card> cards = new List<Card>();

            foreach (Transform child in parentTransform)
            {
                Card card = child.GetComponent<Card>();
                if (card != null)
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

        // Returns the anchored positions of all card objects.
        public List<Vector2> GetAllCardPositions()
        {
            List<Vector2> positions = new List<Vector2>();

            foreach (Transform child in parentTransform)
            {
                RectTransform rt = child.GetComponent<RectTransform>();
                positions.Add(rt.anchoredPosition);
            }

            return positions;
        }

        #endregion

        #region Private Methods

        // Sets grid layout size and spacing based on selected difficulty.
        private void SetLayoutByDifficulty()
        {
            switch (selectedDifficulty)
            {
                case Difficulty.Easy:
                    rows = 2;
                    columns = 2;
                    gridLayout.constraintCount = columns;
                    gridLayout.cellSize = new Vector2(200, 250);
                    gridLayout.spacing = new Vector2(50, 50);
                    break;

                case Difficulty.Medium:
                    rows = 2;
                    columns = 3;
                    gridLayout.constraintCount = columns;
                    gridLayout.cellSize = new Vector2(180, 230);
                    gridLayout.spacing = new Vector2(100, 100);
                    break;

                case Difficulty.Hard:
                    rows = 5;
                    columns = 6;
                    gridLayout.constraintCount = columns;
                    gridLayout.cellSize = new Vector2(120, 150);
                    gridLayout.spacing = new Vector2(150, 5);
                    break;
            }

            RectTransform rt = parentTransform.GetComponent<RectTransform>();
            float width = columns * (gridLayout.cellSize.x + gridLayout.spacing.x);
            float height = rows * (gridLayout.cellSize.y + gridLayout.spacing.y);
            rt.sizeDelta = new Vector2(width, height);

            GameManager.Instance?.SetTotalPairs((rows * columns) / 2);
        }

        // Spawns a randomized deck of card pairs.
        private void SpawnCards()
        {
            int totalCards = rows * columns;
            int pairCount = totalCards / 2;

            List<Sprite> selectedSprites = new List<Sprite>();
            List<Sprite> tempSprites = new List<Sprite>(availableSprites);

            for (int i = 0; i < pairCount; i++)
            {
                int randomIndex = Random.Range(0, tempSprites.Count);
                selectedSprites.Add(tempSprites[randomIndex]);
                tempSprites.RemoveAt(randomIndex);
            }

            List<Sprite> shuffledSprites = new List<Sprite>();
            foreach (Sprite sprite in selectedSprites)
            {
                shuffledSprites.Add(sprite);
                shuffledSprites.Add(sprite);
            }

            Shuffle(shuffledSprites);

            List<Vector2> positions = CalculateCardPositions(shuffledSprites.Count);

            for (int i = 0; i < shuffledSprites.Count; i++)
            {
                SpawnCard(shuffledSprites[i], positions[i]);
            }

            gridLayout.enabled = false;
        }

        // Spawns cards based on saved game data.
        private void SpawnCardsFromSavedGame()
        {
            GameState savedState = GameManager.Instance.LoadGameState();
            if (savedState == null)
            {
                return;
            }

            selectedDifficulty = savedState.difficulty;
            SetLayoutByDifficulty();

            HashSet<string> matchedIDs = new HashSet<string>(savedState.matchedCardIDs);

            List<Vector2> positions = CalculateCardPositions(savedState.remainingCardIDs.Count + savedState.matchedCardIDs.Count);

            int index = 0;

            foreach (string cardID in savedState.remainingCardIDs)
            {
                if (spriteDictionary.TryGetValue(cardID, out Sprite sprite))
                {
                    GameObject cardObj = Instantiate(cardPrefab, parentTransform);

                    Card card = cardObj.GetComponent<Card>();
                    card.SetFrontImage(sprite);
                    card.cardID = cardID;

                    RectTransform rt = cardObj.GetComponent<RectTransform>();
                    rt.anchoredPosition = positions[index + savedState.matchedCardIDs.Count];

                    card.FlipBack();
                    index++;
                }
            }

            gridLayout.enabled = false;

            GameManager.Instance?.SetTotalPairs(savedState.totalPairs);
            GameManager.Instance.OnPairsUpdated?.Invoke(savedState.pairsMatched, savedState.totalPairs);
        }

        // Creates a card instance and sets its position and sprite.
        private void SpawnCard(Sprite sprite, Vector2 position)
        {
            GameObject card = Instantiate(cardPrefab, parentTransform);
            card.transform.localScale = Vector3.one;

            Card cardComponent = card.GetComponent<Card>();
            cardComponent.SetFrontImage(sprite);
            cardComponent.cardID = sprite.name;

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchoredPosition = position;
        }

        // Calculates all card positions based on grid layout.
        private List<Vector2> CalculateCardPositions(int cardCount)
        {
            List<Vector2> positions = new List<Vector2>();

            float offsetX = (columns - 1) * (gridLayout.cellSize.x + gridLayout.spacing.x) * 0.5f;
            float offsetY = (rows - 1) * (gridLayout.cellSize.y + gridLayout.spacing.y) * 0.5f;

            for (int i = 0; i < cardCount; i++)
            {
                int row = i / columns;
                int col = i % columns;

                float x = col * (gridLayout.cellSize.x + gridLayout.spacing.x) - offsetX;
                float y = -row * (gridLayout.cellSize.y + gridLayout.spacing.y) + offsetY;

                positions.Add(new Vector2(x, y));
            }

            return positions;
        }

        // Randomizes the list of sprites.
        private void Shuffle(List<Sprite> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                Sprite temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        #endregion
    }
}

// Author: Sebuloan Johnson
// Description: Handles difficulty selection and triggers card spawning.

using UnityEngine;

namespace MemoryMatchGame
{
    public class ModeSelector : MonoBehaviour
    {
        // Reference to CardSpawner assigned in Inspector
        public CardSpawner cardSpawner;

        #region Public Methods

        // Sets the difficulty to Easy and spawns cards
        public void SetEasyMode()
        {
            cardSpawner.selectedDifficulty = Difficulty.Easy;
            cardSpawner.SpawnCardsWrapper();
        }

        // Sets the difficulty to Medium and spawns cards
        public void SetMediumMode()
        {
            cardSpawner.selectedDifficulty = Difficulty.Medium;
            cardSpawner.SpawnCardsWrapper();
        }

        // Sets the difficulty to Hard and spawns cards
        public void SetHardMode()
        {
            cardSpawner.selectedDifficulty = Difficulty.Hard;
            cardSpawner.SpawnCardsWrapper();
        }

        #endregion
    }
}

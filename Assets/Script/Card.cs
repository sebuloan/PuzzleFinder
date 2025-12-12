// Author: Sebuloan Johnson
// Description: Handles card flipping, matching behavior, and visuals.

using UnityEngine;
using UnityEngine.UI;

namespace MemoryMatchGame
{
    public class Card : MonoBehaviour
    {
        public GameObject front;
        public GameObject back;

        public bool isFlipped = false;
        public string cardID;

        public bool IsMatched { get; private set; } = false;

        private Image cardImage;
        private bool isBeingDestroyed = false;

        #region Unity Methods

        // Initializes card visuals and ensures the card starts face-down.
        private void Awake()
        {
            cardImage = GetComponent<Image>();
            FlipBack();
        }

        // Ensures no actions run while the card is being destroyed.
        private void OnDestroy()
        {
            isBeingDestroyed = true;
        }

        // Used for mouse input-based card flipping.
        private void OnMouseDown()
        {
            Flip();
        }

        #endregion

        #region Public Methods

        // Sets the sprite displayed on the front of the card.
        public void SetFrontImage(Sprite sprite)
        {
            if (front == null)
                return;

            Image img = front.GetComponent<Image>();

            if (img != null)
            {
                img.sprite = sprite;
            }

            cardID = sprite.name;
        }

        // Flips the card to show its front side.
        public void Flip()
        {
            if (isFlipped || IsMatched || !GameManager.Instance.CanFlipCard())
                return;

            isFlipped = true;

            if (front != null) front.SetActive(true);
            if (back != null) back.SetActive(false);

            GameManager.Instance.CardFlipped(this);
        }

        // Flips the card back to show its back side.
        public void FlipBack()
        {
            if (IsMatched || isBeingDestroyed)
                return;

            isFlipped = false;

            if (front != null) front.SetActive(false);
            if (back != null) back.SetActive(true);
        }

        // Marks the card as matched and applies visual feedback.
        public void MarkAsMatched()
        {
            if (isBeingDestroyed)
                return;

            IsMatched = true;

            if (cardImage != null)
            {
                cardImage.color = new Color(0.5f, 1f, 0.5f, 0.7f);
            }

            if (front != null) front.SetActive(true);
            if (back != null) back.SetActive(false);
        }

        #endregion
    }
}

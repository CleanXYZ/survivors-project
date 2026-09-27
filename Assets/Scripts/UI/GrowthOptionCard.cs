using System;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.UI
{
    public sealed class GrowthOptionCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Button rerollButton;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;

        private Action<int> selected;
        private Action<int> rerolled;
        private int optionIndex;

        public void SetReferences(
            Button cardButton,
            Button cardRerollButton,
            Text cardNameText,
            Text cardDescriptionText)
        {
            button = cardButton;
            rerollButton = cardRerollButton;
            nameText = cardNameText;
            descriptionText = cardDescriptionText;
        }

        public void Bind(
            int index,
            string displayName,
            string description,
            Action<int> onSelected,
            Action<int> onRerolled,
            bool canReroll)
        {
            optionIndex = index;
            selected = onSelected;
            rerolled = onRerolled;

            if (nameText != null)
            {
                nameText.text = displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = description;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
                button.interactable = true;
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(HandleRerollClick);
                rerollButton.onClick.AddListener(HandleRerollClick);
                rerollButton.interactable = canReroll;
            }
        }

        public void SetInteractable(bool selectionInteractable, bool rerollInteractable)
        {
            if (button != null)
            {
                button.interactable = selectionInteractable;
            }

            if (rerollButton != null)
            {
                rerollButton.interactable = rerollInteractable;
            }
        }

        private void HandleClick()
        {
            selected?.Invoke(optionIndex);
        }

        private void HandleRerollClick()
        {
            rerolled?.Invoke(optionIndex);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(HandleRerollClick);
            }
        }
    }
}

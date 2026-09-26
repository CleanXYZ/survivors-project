using System;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.UI
{
    public sealed class GrowthOptionCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text nameText;
        [SerializeField] private Text descriptionText;

        private Action<int> selected;
        private int optionIndex;

        public void SetReferences(Button cardButton, Text cardNameText, Text cardDescriptionText)
        {
            button = cardButton;
            nameText = cardNameText;
            descriptionText = cardDescriptionText;
        }

        public void Bind(int index, string displayName, string description, Action<int> onSelected)
        {
            optionIndex = index;
            selected = onSelected;

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
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        private void HandleClick()
        {
            selected?.Invoke(optionIndex);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }
    }
}

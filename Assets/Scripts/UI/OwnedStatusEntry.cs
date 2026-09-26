using UnityEngine;
using UnityEngine.UI;

namespace Survivors.UI
{
    public sealed class OwnedStatusEntry : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;

        public void SetReferences(Image icon, Text itemName, Text itemLevel)
        {
            iconImage = icon;
            nameText = itemName;
            levelText = itemLevel;
        }

        public void Bind(
            Sprite icon,
            Sprite placeholderIcon,
            Color missingIconColor,
            string displayName,
            int? level)
        {
            if (iconImage != null)
            {
                bool hasIcon = icon != null;
                iconImage.sprite = hasIcon ? icon : placeholderIcon;
                iconImage.color = hasIcon || placeholderIcon != null
                    ? Color.white
                    : missingIconColor;
                iconImage.preserveAspect = true;
            }

            if (nameText != null)
            {
                nameText.text = displayName;
            }

            if (levelText != null)
            {
                levelText.gameObject.SetActive(level.HasValue);
                levelText.text = level.HasValue ? $"Lv.{level.Value}" : string.Empty;
            }
        }
    }
}

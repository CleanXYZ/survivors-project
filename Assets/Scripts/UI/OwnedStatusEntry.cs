using UnityEngine;
using UnityEngine.UI;

namespace Survivors.UI
{
    public sealed class OwnedStatusEntry : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text detailsText;
        [SerializeField] private LayoutElement rootLayout;

        private float collapsedHeight = 48f;
        private float detailLineHeight = 18f;

        public void SetReferences(
            Image icon,
            Text itemName,
            Text itemLevel,
            Text itemDetails,
            LayoutElement layout,
            float baseHeight,
            float lineHeight)
        {
            iconImage = icon;
            nameText = itemName;
            levelText = itemLevel;
            detailsText = itemDetails;
            rootLayout = layout;
            collapsedHeight = baseHeight;
            detailLineHeight = lineHeight;
        }

        public void Bind(
            Sprite icon,
            Sprite placeholderIcon,
            Color missingIconColor,
            string displayName,
            int? level,
            string details = null)
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

            bool hasDetails = !string.IsNullOrWhiteSpace(details);
            if (detailsText != null)
            {
                detailsText.gameObject.SetActive(hasDetails);
                detailsText.text = hasDetails ? details : string.Empty;
            }

            if (rootLayout != null)
            {
                int detailLineCount = hasDetails ? details.Split('\n').Length : 0;
                rootLayout.preferredHeight = collapsedHeight
                    + detailLineCount * detailLineHeight
                    + (hasDetails ? 4f : 0f);
            }
        }
    }
}

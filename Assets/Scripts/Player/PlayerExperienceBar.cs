using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerExperience))]
    public sealed class PlayerExperienceBar : MonoBehaviour
    {
        private const string HudRootName = "__ExperienceHUD";

        [Header("Screen Layout")]
        [SerializeField, Min(1f)] private float barHeight = 28f;
        [SerializeField, Min(0f)] private float horizontalMargin = 24f;
        [SerializeField, Min(0f)] private float bottomOffset = 16f;
        [SerializeField, Min(0f)] private float borderSize = 2f;
        [SerializeField, Min(1f)] private float levelBadgeWidth = 48f;
        [SerializeField, Min(0f)] private float levelBadgeGap = 6f;

        [Header("Colors")]
        [SerializeField] private Color borderColor = new(0.04f, 0.09f, 0.16f, 1f);
        [SerializeField] private Color backgroundColor = new(0.06f, 0.12f, 0.2f, 0.95f);
        [SerializeField] private Color fillColor = new(0.12f, 0.52f, 1f, 1f);
        [SerializeField] private Color levelTextColor = Color.white;

        [Header("Level Label")]
        [SerializeField, Min(1)] private int levelFontSize = 18;

        private PlayerExperience experience;
        private RectTransform fillRect;
        private Text levelText;

        private void Awake()
        {
            experience = GetComponent<PlayerExperience>();
            BuildHud();
        }

        private void OnEnable()
        {
            experience.ExperienceChanged += Refresh;
            Refresh(experience.Level, experience.CurrentExperience, experience.ExperienceToNextLevel);
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                experience.ExperienceChanged -= Refresh;
            }
        }

        private void BuildHud()
        {
            Transform existing = transform.Find(HudRootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject hudRoot = new(HudRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            hudRoot.transform.SetParent(transform, false);

            Canvas canvas = hudRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = hudRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform levelBadge = CreateImage(hudRoot.transform, "Level Badge", borderColor);
            SetBottomLeftBoxLayout(
                levelBadge,
                horizontalMargin,
                bottomOffset,
                levelBadgeWidth,
                barHeight);

            RectTransform levelBackground = CreateImage(levelBadge, "Background", backgroundColor);
            StretchWithInset(levelBackground, borderSize);

            RectTransform border = CreateImage(hudRoot.transform, "Border", borderColor);
            SetBottomBarLayout(
                border,
                horizontalMargin + levelBadgeWidth + levelBadgeGap,
                horizontalMargin,
                bottomOffset,
                barHeight);

            RectTransform background = CreateImage(border, "Background", backgroundColor);
            StretchWithInset(background, borderSize);

            fillRect = CreateImage(background, "Fill", fillColor);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject labelObject = new("Level", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(levelBackground, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            levelText = labelObject.GetComponent<Text>();
            levelText.raycastTarget = false;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            levelText.fontSize = levelFontSize;
            levelText.fontStyle = FontStyle.Bold;
            levelText.color = levelTextColor;
        }

        private static RectTransform CreateImage(Transform parent, string objectName, Color color)
        {
            GameObject imageObject = new(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return imageObject.GetComponent<RectTransform>();
        }

        private static void SetBottomBarLayout(
            RectTransform rect,
            float leftMargin,
            float rightMargin,
            float bottom,
            float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(leftMargin, bottom);
            rect.offsetMax = new Vector2(-rightMargin, bottom + height);
        }

        private static void SetBottomLeftBoxLayout(
            RectTransform rect,
            float left,
            float bottom,
            float width,
            float height)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(left, bottom);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void StretchWithInset(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private void Refresh(int level, int currentExperience, int requiredExperience)
        {
            float ratio = requiredExperience > 0
                ? Mathf.Clamp01((float)currentExperience / requiredExperience)
                : 0f;

            if (fillRect != null)
            {
                fillRect.anchorMax = new Vector2(ratio, 1f);
            }

            if (levelText != null)
            {
                levelText.text = level.ToString();
            }
        }

        private void OnValidate()
        {
            barHeight = Mathf.Max(1f, barHeight);
            horizontalMargin = Mathf.Max(0f, horizontalMargin);
            bottomOffset = Mathf.Max(0f, bottomOffset);
            borderSize = Mathf.Clamp(borderSize, 0f, barHeight * 0.49f);
            levelBadgeWidth = Mathf.Max(1f, levelBadgeWidth);
            levelBadgeGap = Mathf.Max(0f, levelBadgeGap);
            levelFontSize = Mathf.Max(1, levelFontSize);
        }
    }
}

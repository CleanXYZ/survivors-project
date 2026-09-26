using System;
using Survivors.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Survivors.UI
{
    [RequireComponent(typeof(PlayerExperience))]
    public sealed class LevelUpSelectionUI : MonoBehaviour
    {
        [Serializable]
        private struct TemporaryGrowthOption
        {
            public string id;
            public string displayName;
            [TextArea] public string description;

            public TemporaryGrowthOption(string id, string displayName, string description)
            {
                this.id = id;
                this.displayName = displayName;
                this.description = description;
            }
        }

        private const string RuntimeRootName = "__LevelUpSelectionUI";
        private const int MaximumCardCount = 3;

        [Header("UI References")]
        [SerializeField] private GameObject uiRoot;
        [SerializeField] private GrowthOptionCard[] cards = new GrowthOptionCard[MaximumCardCount];

        [Header("Temporary Options")]
        [SerializeField] private TemporaryGrowthOption[] temporaryOptions =
        {
            new("attack_power", "공격력 증가", "모든 공격의 피해량이 증가합니다. (임시 선택지)"),
            new("move_speed", "이동속도 증가", "플레이어의 이동속도가 증가합니다. (임시 선택지)"),
            new("attack_speed", "공격속도 증가", "자동 공격의 공격속도가 증가합니다. (임시 선택지)")
        };

        [Header("Runtime Fallback Layout")]
        [SerializeField] private Color overlayColor = new(0.015f, 0.025f, 0.05f, 0.88f);
        [SerializeField] private Color cardColor = new(0.08f, 0.14f, 0.24f, 1f);
        [SerializeField] private Color cardHighlightColor = new(0.13f, 0.32f, 0.55f, 1f);

        private PlayerExperience experience;
        private bool acceptingSelection;

        private void Awake()
        {
            experience = GetComponent<PlayerExperience>();
            EnsureTemporaryOptions();

            if (!HasConfiguredCards())
            {
                BuildRuntimeUi();
            }

            if (uiRoot != null)
            {
                uiRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            experience.LeveledUp += HandleLeveledUp;
            experience.StateChanged += HandleStateChanged;
        }

        private void Start()
        {
            if (experience.IsLevelUpPending)
            {
                Show();
            }
        }

        private void Update()
        {
            if (!acceptingSelection || uiRoot == null || !uiRoot.activeSelf)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
            {
                TrySelect(0);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
            {
                TrySelect(1);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
            {
                TrySelect(2);
            }
        }

        private void HandleLeveledUp(int level)
        {
            Show();
        }

        private void HandleStateChanged(PlayerExperienceState state)
        {
            if (state == PlayerExperienceState.Normal)
            {
                Hide();
            }
        }

        private void Show()
        {
            if (uiRoot == null)
            {
                return;
            }

            EnsureEventSystem();

            int visibleCardCount = Mathf.Min(
                MaximumCardCount,
                Mathf.Min(cards.Length, temporaryOptions.Length));

            for (int i = 0; i < cards.Length; i++)
            {
                GrowthOptionCard card = cards[i];
                if (card == null)
                {
                    continue;
                }

                bool isVisible = i < visibleCardCount;
                card.gameObject.SetActive(isVisible);

                if (isVisible)
                {
                    TemporaryGrowthOption option = temporaryOptions[i];
                    card.Bind(i, option.displayName, option.description, TrySelect);
                }
            }

            acceptingSelection = visibleCardCount > 0;
            uiRoot.SetActive(true);
        }

        private void TrySelect(int optionIndex)
        {
            if (!acceptingSelection
                || optionIndex < 0
                || optionIndex >= temporaryOptions.Length
                || optionIndex >= cards.Length
                || cards[optionIndex] == null
                || !cards[optionIndex].gameObject.activeInHierarchy)
            {
                return;
            }

            acceptingSelection = false;
            SetCardsInteractable(false);

            TemporaryGrowthOption selectedOption = temporaryOptions[optionIndex];
            Debug.Log(
                $"Level {experience.Level} growth selected: "
                + $"{selectedOption.displayName} ({selectedOption.id})",
                this);

            bool completed = experience.CompleteLevelUp();

            if (!completed || !experience.IsLevelUpPending)
            {
                Hide();
            }
        }

        private void Hide()
        {
            acceptingSelection = false;
            SetCardsInteractable(false);

            if (uiRoot != null)
            {
                uiRoot.SetActive(false);
            }
        }

        private void SetCardsInteractable(bool interactable)
        {
            if (cards == null)
            {
                return;
            }

            foreach (GrowthOptionCard card in cards)
            {
                if (card != null)
                {
                    card.SetInteractable(interactable);
                }
            }
        }

        private bool HasConfiguredCards()
        {
            if (uiRoot == null || cards == null || cards.Length == 0)
            {
                return false;
            }

            int expectedCount = Mathf.Min(MaximumCardCount, temporaryOptions.Length);
            if (cards.Length < expectedCount)
            {
                return false;
            }

            for (int i = 0; i < expectedCount; i++)
            {
                if (cards[i] == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void EnsureTemporaryOptions()
        {
            if (temporaryOptions != null && temporaryOptions.Length > 0)
            {
                return;
            }

            temporaryOptions = new[]
            {
                new TemporaryGrowthOption(
                    "attack_power",
                    "공격력 증가",
                    "모든 공격의 피해량이 증가합니다. (임시 선택지)"),
                new TemporaryGrowthOption(
                    "move_speed",
                    "이동속도 증가",
                    "플레이어의 이동속도가 증가합니다. (임시 선택지)"),
                new TemporaryGrowthOption(
                    "attack_speed",
                    "공격속도 증가",
                    "자동 공격의 공격속도가 증가합니다. (임시 선택지)")
            };
        }

        private void BuildRuntimeUi()
        {
            Transform existing = transform.Find(RuntimeRootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject root = new(RuntimeRootName, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            uiRoot = root;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform overlay = CreateImage(root.transform, "Overlay", overlayColor);
            Stretch(overlay, Vector2.zero, Vector2.zero);

            Text title = CreateText(overlay, "Title", "LEVEL UP", 48, FontStyle.Bold);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 250f);
            titleRect.sizeDelta = new Vector2(800f, 80f);

            cards = new GrowthOptionCard[MaximumCardCount];
            const float cardWidth = 420f;
            const float cardHeight = 360f;
            const float cardGap = 36f;

            for (int i = 0; i < cards.Length; i++)
            {
                float x = (i - 1) * (cardWidth + cardGap);
                cards[i] = CreateCard(overlay, i, x, cardWidth, cardHeight);
            }
        }

        private GrowthOptionCard CreateCard(
            Transform parent,
            int index,
            float x,
            float width,
            float height)
        {
            RectTransform cardRect = CreateImage(parent, $"Growth Card {index + 1}", cardColor);
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.anchoredPosition = new Vector2(x, 0f);
            cardRect.sizeDelta = new Vector2(width, height);

            Image cardImage = cardRect.GetComponent<Image>();
            Button button = cardRect.gameObject.AddComponent<Button>();
            button.targetGraphic = cardImage;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = cardHighlightColor;
            colors.pressedColor = new Color(0.72f, 0.82f, 1f, 1f);
            colors.selectedColor = cardHighlightColor;
            button.colors = colors;

            Text shortcut = CreateText(cardRect, "Shortcut", (index + 1).ToString(), 28, FontStyle.Bold);
            SetRect(shortcut.rectTransform, new Vector2(24f, -20f), new Vector2(48f, 48f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), TextAnchor.UpperLeft);

            Text nameLabel = CreateText(cardRect, "Name", string.Empty, 32, FontStyle.Bold);
            SetRect(nameLabel.rectTransform, new Vector2(24f, -82f), new Vector2(width - 48f, 70f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), TextAnchor.MiddleCenter);

            Text descriptionLabel = CreateText(cardRect, "Description", string.Empty, 22, FontStyle.Normal);
            SetRect(descriptionLabel.rectTransform, new Vector2(32f, 34f), new Vector2(width - 64f, 190f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), TextAnchor.MiddleCenter);

            GrowthOptionCard card = cardRect.gameObject.AddComponent<GrowthOptionCard>();
            card.SetReferences(button, nameLabel, descriptionLabel);
            return card;
        }

        private static RectTransform CreateImage(Transform parent, string objectName, Color color)
        {
            GameObject imageObject = new(objectName, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return imageObject.GetComponent<RectTransform>();
        }

        private static Text CreateText(
            Transform parent,
            string objectName,
            string content,
            int fontSize,
            FontStyle fontStyle)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            Text text = textObject.GetComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect, Vector2 minimumOffset, Vector2 maximumOffset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = minimumOffset;
            rect.offsetMax = maximumOffset;
        }

        private static void SetRect(
            RectTransform rect,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 anchor,
            Vector2 pivot,
            TextAnchor alignment)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.GetComponent<Text>().alignment = alignment;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null || FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new("EventSystem", typeof(EventSystem));
            InputSystemUIInputModule inputModule =
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private void OnDisable()
        {
            if (experience != null)
            {
                experience.LeveledUp -= HandleLeveledUp;
                experience.StateChanged -= HandleStateChanged;
            }

            Hide();
        }
    }
}

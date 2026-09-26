using System.Collections.Generic;
using Survivors.Growth;
using UnityEngine;
using UnityEngine.UI;

namespace Survivors.UI
{
    [RequireComponent(typeof(GrowthContentLibrary), typeof(GrowthRuntimeState))]
    public sealed class OwnedStatusUI : MonoBehaviour
    {
        private const string RuntimeRootName = "__OwnedStatusUI";

        [Header("Data")]
        [SerializeField] private GrowthContentLibrary contentLibrary;
        [SerializeField] private GrowthRuntimeState growthState;

        [Header("UI References")]
        [SerializeField] private GameObject uiRoot;
        [SerializeField] private Transform weaponContainer;
        [SerializeField] private Transform passiveContainer;
        [SerializeField] private OwnedStatusEntry weaponEntryPrefab;
        [SerializeField] private OwnedStatusEntry passiveEntryPrefab;
        [SerializeField] private Sprite placeholderIcon;

        [Header("Layout")]
        [SerializeField, Min(16f)] private float panelWidth = 300f;
        [SerializeField, Min(8f)] private float iconSize = 38f;
        [SerializeField, Min(18f)] private float entryHeight = 48f;
        [SerializeField] private Vector2 topLeftOffset = new(24f, -90f);
        [SerializeField] private Color panelColor = new(0.025f, 0.045f, 0.08f, 0.82f);
        [SerializeField] private Color entryColor = new(0.08f, 0.12f, 0.19f, 0.86f);
        [SerializeField] private Color missingIconColor = new(0.25f, 0.3f, 0.38f, 1f);

        private readonly List<OwnedStatusEntry> weaponEntries = new();
        private readonly List<OwnedStatusEntry> passiveEntries = new();

        private void Awake()
        {
            contentLibrary = EnsureComponent(contentLibrary);
            growthState = EnsureComponent(growthState);

            if (uiRoot == null || weaponContainer == null || passiveContainer == null)
            {
                BuildRuntimeUi();
            }
        }

        private void OnEnable()
        {
            if (growthState != null)
            {
                growthState.Changed += Refresh;
            }
        }

        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (contentLibrary == null || growthState == null)
            {
                return;
            }

            IReadOnlyList<WeaponGrowthState> weapons = growthState.OwnedWeapons;
            EnsureEntryCount(
                weaponEntries,
                weaponContainer,
                weaponEntryPrefab,
                weapons.Count,
                "Weapon");

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponGrowthState state = weapons[i];
                WeaponDefinition definition = state == null
                    ? null
                    : contentLibrary.FindWeapon(state.WeaponId);
                string displayName = definition?.DisplayName
                    ?? state?.WeaponId
                    ?? "알 수 없는 무기";
                weaponEntries[i].Bind(
                    definition?.Icon,
                    placeholderIcon,
                    missingIconColor,
                    displayName,
                    null);
            }

            List<PassiveGrowthState> visiblePassives = new();
            foreach (PassiveGrowthState passive in growthState.OwnedPassives)
            {
                if (passive != null && passive.Level > 0)
                {
                    visiblePassives.Add(passive);
                }
            }

            EnsureEntryCount(
                passiveEntries,
                passiveContainer,
                passiveEntryPrefab,
                visiblePassives.Count,
                "Passive");

            for (int i = 0; i < visiblePassives.Count; i++)
            {
                PassiveGrowthState state = visiblePassives[i];
                PassiveDefinition definition = contentLibrary.FindPassive(state.PassiveId);
                passiveEntries[i].Bind(
                    definition?.Icon,
                    placeholderIcon,
                    missingIconColor,
                    definition?.DisplayName ?? state.PassiveId,
                    state.Level);
            }
        }

        private void EnsureEntryCount(
            List<OwnedStatusEntry> entries,
            Transform container,
            OwnedStatusEntry prefab,
            int activeCount,
            string entryName)
        {
            if (container == null)
            {
                return;
            }

            while (entries.Count < activeCount)
            {
                OwnedStatusEntry entry = prefab != null
                    ? Instantiate(prefab, container)
                    : CreateRuntimeEntry(container, $"{entryName} Entry {entries.Count + 1}");
                entries.Add(entry);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].gameObject.SetActive(i < activeCount);
            }
        }

        private void BuildRuntimeUi()
        {
            Transform existing = transform.Find(RuntimeRootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            GameObject root = new(RuntimeRootName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            root.transform.SetParent(transform, false);
            uiRoot = root;

            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform panel = CreateImage(root.transform, "Owned Status Panel", panelColor);
            panel.anchorMin = new Vector2(0f, 1f);
            panel.anchorMax = new Vector2(0f, 1f);
            panel.pivot = new Vector2(0f, 1f);
            panel.anchoredPosition = topLeftOffset;
            panel.sizeDelta = new Vector2(panelWidth, 0f);

            VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(12, 12, 12, 12);
            panelLayout.spacing = 8f;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            ContentSizeFitter panelFitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateLabel(panel, "Title", "보유 현황", 24, FontStyle.Bold, 36f);
            weaponContainer = CreateSection(panel, "Weapons", "무기");
            passiveContainer = CreateSection(panel, "Passives", "패시브");
        }

        private Transform CreateSection(Transform parent, string objectName, string heading)
        {
            GameObject section = new(objectName, typeof(RectTransform), typeof(VerticalLayoutGroup));
            section.transform.SetParent(parent, false);
            VerticalLayoutGroup layout = section.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateLabel(section.transform, "Heading", heading, 19, FontStyle.Bold, 28f);

            GameObject entries = new("Entries", typeof(RectTransform), typeof(VerticalLayoutGroup));
            entries.transform.SetParent(section.transform, false);
            VerticalLayoutGroup entryLayout = entries.GetComponent<VerticalLayoutGroup>();
            entryLayout.spacing = 4f;
            entryLayout.childControlWidth = true;
            entryLayout.childControlHeight = true;
            entryLayout.childForceExpandWidth = true;
            entryLayout.childForceExpandHeight = false;
            return entries.transform;
        }

        private OwnedStatusEntry CreateRuntimeEntry(Transform parent, string objectName)
        {
            RectTransform row = CreateImage(parent, objectName, entryColor);
            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = entryHeight;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 8, 5, 5);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconObject.transform.SetParent(row, false);
            Image icon = iconObject.GetComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            LayoutElement iconLayout = iconObject.GetComponent<LayoutElement>();
            iconLayout.preferredWidth = iconSize;
            iconLayout.preferredHeight = iconSize;

            Text itemName = CreateText(row, "Name", string.Empty, 17, FontStyle.Normal);
            itemName.alignment = TextAnchor.MiddleLeft;
            LayoutElement nameLayout = itemName.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1f;

            Text level = CreateText(row, "Level", string.Empty, 16, FontStyle.Bold);
            level.alignment = TextAnchor.MiddleRight;
            LayoutElement levelLayout = level.gameObject.AddComponent<LayoutElement>();
            levelLayout.preferredWidth = 56f;

            OwnedStatusEntry entry = row.gameObject.AddComponent<OwnedStatusEntry>();
            entry.SetReferences(icon, itemName, level);
            return entry;
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

        private static Text CreateLabel(
            Transform parent,
            string objectName,
            string content,
            int fontSize,
            FontStyle fontStyle,
            float height)
        {
            Text text = CreateText(parent, objectName, content, fontSize, fontStyle);
            LayoutElement layout = text.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = height;
            return text;
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
            text.raycastTarget = false;
            return text;
        }

        private T EnsureComponent<T>(T configuredComponent) where T : Component
        {
            if (configuredComponent != null)
            {
                return configuredComponent;
            }

            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }

        private void OnDisable()
        {
            if (growthState != null)
            {
                growthState.Changed -= Refresh;
            }
        }

        private void OnValidate()
        {
            panelWidth = Mathf.Max(16f, panelWidth);
            iconSize = Mathf.Max(8f, iconSize);
            entryHeight = Mathf.Max(18f, entryHeight);
        }
    }
}

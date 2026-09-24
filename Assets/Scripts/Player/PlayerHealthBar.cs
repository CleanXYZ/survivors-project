using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerHealth))]
    public sealed class PlayerHealthBar : MonoBehaviour
    {
        private const string VisualRootName = "__HealthBar";

        [Header("Layout")]
        [Tooltip("X is the minimum bar width. Y is the bar height.")]
        [SerializeField] private Vector2 barSize = new(1.2f, 0.14f);
        [SerializeField, Min(0.01f)] private float widthPerHealth = 0.12f;
        [SerializeField, Min(0.01f)] private float maximumBarWidth = 4f;
        [SerializeField, Min(0f)] private float borderSize = 0.025f;
        [SerializeField] private float verticalOffset = 0.8f;

        [Header("Health Dividers")]
        [SerializeField, Min(1)] private int majorDividerInterval = 10;
        [SerializeField, Min(0.001f)] private float minorDividerWidth = 0.01f;
        [SerializeField, Min(0.001f)] private float majorDividerWidth = 0.025f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new(0.12f, 0.12f, 0.12f, 1f);
        [SerializeField] private Color fillColor = new(0.2f, 0.9f, 0.32f, 1f);
        [SerializeField] private Color minorDividerColor = new(0.04f, 0.07f, 0.05f, 0.9f);
        [SerializeField] private Color majorDividerColor = new(1f, 0.78f, 0.16f, 1f);

        private PlayerHealth health;
        private GameObject visualRoot;
        private Transform fillTransform;
        private Sprite generatedSprite;
        private float fillFullWidth;
        private int displayedMaxHealth = -1;

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            generatedSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            generatedSprite.name = "RuntimeHealthBarSprite";
        }

        private void OnEnable()
        {
            health.HealthChanged += Refresh;
            Refresh(health.CurrentHealth, health.MaxHealth);
        }

        private void LateUpdate()
        {
            if (displayedMaxHealth != health.MaxHealth)
            {
                Refresh(health.CurrentHealth, health.MaxHealth);
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= Refresh;
            }
        }

        private void OnDestroy()
        {
            if (generatedSprite != null)
            {
                Destroy(generatedSprite);
            }
        }

        private void RebuildVisuals(int maxHealth)
        {
            if (visualRoot != null)
            {
                Destroy(visualRoot);
            }

            float barWidth = Mathf.Min(
                Mathf.Max(barSize.x, maxHealth * widthPerHealth),
                maximumBarWidth);

            visualRoot = new GameObject(VisualRootName);
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = new Vector3(0f, verticalOffset, 0f);

            CreateBarPart(visualRoot.transform, "Background", new Vector2(barWidth, barSize.y), backgroundColor, 20);

            fillFullWidth = Mathf.Max(0.01f, barWidth - borderSize * 2f);
            float innerHeight = Mathf.Max(0.01f, barSize.y - borderSize * 2f);
            fillTransform = CreateBarPart(
                visualRoot.transform,
                "Fill",
                new Vector2(fillFullWidth, innerHeight),
                fillColor,
                21).transform;

            CreateDividers(maxHealth, innerHeight);
            displayedMaxHealth = maxHealth;
        }

        private void CreateDividers(int maxHealth, float innerHeight)
        {
            if (maxHealth <= 1)
            {
                return;
            }

            float unitWidth = fillFullWidth / maxHealth;

            for (int healthUnit = 1; healthUnit <= maxHealth; healthUnit++)
            {
                bool isMajor = healthUnit % majorDividerInterval == 0;
                float requestedWidth = isMajor ? majorDividerWidth : minorDividerWidth;
                float dividerWidth = Mathf.Min(requestedWidth, unitWidth * 0.5f);
                Color dividerColor = isMajor ? majorDividerColor : minorDividerColor;

                GameObject divider = CreateBarPart(
                    visualRoot.transform,
                    isMajor ? $"Major Divider {healthUnit}" : $"Divider {healthUnit}",
                    new Vector2(dividerWidth, innerHeight),
                    dividerColor,
                    22);

                float normalizedPosition = (float)healthUnit / maxHealth;
                divider.transform.localPosition = new Vector3(
                    -fillFullWidth * 0.5f + fillFullWidth * normalizedPosition,
                    0f,
                    0f);
            }
        }

        private GameObject CreateBarPart(
            Transform parent,
            string objectName,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            GameObject part = new(objectName);
            part.transform.SetParent(parent, false);
            part.transform.localScale = new Vector3(size.x, size.y, 1f);

            SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = generatedSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return part;
        }

        private void Refresh(int currentHealth, int maxHealth)
        {
            maxHealth = Mathf.Max(1, maxHealth);

            if (visualRoot == null || displayedMaxHealth != maxHealth)
            {
                RebuildVisuals(maxHealth);
            }

            float ratio = Mathf.Clamp01((float)currentHealth / maxHealth);
            Vector3 scale = fillTransform.localScale;
            scale.x = fillFullWidth * ratio;
            fillTransform.localScale = scale;
            fillTransform.localPosition = new Vector3(-fillFullWidth * (1f - ratio) * 0.5f, 0f, 0f);
        }

        private void OnValidate()
        {
            barSize.x = Mathf.Max(0.01f, barSize.x);
            barSize.y = Mathf.Max(0.01f, barSize.y);
            widthPerHealth = Mathf.Max(0.01f, widthPerHealth);
            maximumBarWidth = Mathf.Max(barSize.x, maximumBarWidth);
            borderSize = Mathf.Clamp(borderSize, 0f, Mathf.Min(barSize.x, barSize.y) * 0.49f);
            majorDividerInterval = Mathf.Max(1, majorDividerInterval);
            minorDividerWidth = Mathf.Max(0.001f, minorDividerWidth);
            majorDividerWidth = Mathf.Max(minorDividerWidth, majorDividerWidth);
        }
    }
}

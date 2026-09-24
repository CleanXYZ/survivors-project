using UnityEngine;
using UnityEngine.UI;

namespace Survivors.Combat
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class AttackCooldownRadialVisual : MonoBehaviour
    {
        private const string VisualRootName = "__AttackCooldownRadial";

        [Header("Cooldown Appearance")]
        [SerializeField, Range(0f, 1f)] private float cooldownLightenAmount = 0.65f;
        [SerializeField, Min(0.01f)] private float radialFillScale = 1f;
        [SerializeField] private SpriteRenderer targetRenderer;

        private MonoBehaviour cooldownSourceBehaviour;
        private IAttackCooldownSource cooldownSource;
        private Color normalColor;
        private GameObject visualRoot;
        private Image radialFill;

        protected abstract MonoBehaviour GetCooldownSourceBehaviour();

        protected virtual void Awake()
        {
            cooldownSourceBehaviour = GetCooldownSourceBehaviour();
            cooldownSource = cooldownSourceBehaviour as IAttackCooldownSource;

            if (cooldownSource == null)
            {
                throw new MissingComponentException(
                    $"{GetType().Name} on '{name}' needs an {nameof(IAttackCooldownSource)} component.");
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<SpriteRenderer>();
            }

            normalColor = targetRenderer.color;
            CreateRadialFill();
            ShowReadyState();
        }

        private void Update()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (cooldownSourceBehaviour == null || !cooldownSourceBehaviour.isActiveAndEnabled)
            {
                StopVisual();
                return;
            }

            if (!visualRoot.activeSelf)
            {
                visualRoot.SetActive(true);
            }

            if (!cooldownSource.IsAttackOnCooldown)
            {
                ShowReadyState();
                return;
            }

            Color cooldownColor = Color.Lerp(normalColor, Color.white, cooldownLightenAmount);
            cooldownColor.a = targetRenderer.color.a;
            targetRenderer.color = cooldownColor;

            radialFill.gameObject.SetActive(true);
            radialFill.fillAmount = cooldownSource.AttackCooldownProgress;
        }

        protected virtual void OnDisable()
        {
            StopVisual();
        }

        private void CreateRadialFill()
        {
            visualRoot = new GameObject(VisualRootName, typeof(RectTransform), typeof(Canvas));
            visualRoot.transform.SetParent(transform, false);
            visualRoot.transform.localPosition = Vector3.zero;

            RectTransform rootRect = (RectTransform)visualRoot.transform;
            Vector2 spriteSize = targetRenderer.sprite.bounds.size;
            rootRect.sizeDelta = spriteSize * radialFillScale;

            Canvas canvas = visualRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingLayerID = targetRenderer.sortingLayerID;
            canvas.sortingOrder = targetRenderer.sortingOrder + 1;

            GameObject fillObject = new("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObject.transform.SetParent(visualRoot.transform, false);

            RectTransform fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            radialFill = fillObject.GetComponent<Image>();
            radialFill.sprite = targetRenderer.sprite;
            radialFill.color = normalColor;
            radialFill.type = Image.Type.Filled;
            radialFill.fillMethod = Image.FillMethod.Radial360;
            radialFill.fillOrigin = (int)Image.Origin360.Top;
            radialFill.fillClockwise = true;
            radialFill.fillAmount = 1f;
            radialFill.raycastTarget = false;
        }

        private void ShowReadyState()
        {
            if (targetRenderer != null)
            {
                Color readyColor = normalColor;
                readyColor.a = targetRenderer.color.a;
                targetRenderer.color = readyColor;
            }

            if (radialFill != null)
            {
                radialFill.fillAmount = 1f;
                radialFill.gameObject.SetActive(false);
            }
        }

        private void StopVisual()
        {
            ShowReadyState();

            if (visualRoot != null)
            {
                visualRoot.SetActive(false);
            }
        }

        protected virtual void OnValidate()
        {
            cooldownLightenAmount = Mathf.Clamp01(cooldownLightenAmount);
            radialFillScale = Mathf.Max(0.01f, radialFillScale);
        }
    }
}

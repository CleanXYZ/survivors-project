using Survivors.Combat;
using Survivors.Enemies;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerHealth), typeof(SpriteRenderer))]
    public sealed class PlayerBowAttack : MonoBehaviour, IAttackCooldownSource
    {
        [Header("Bow Attack")]
        [SerializeField, Min(1)] private int damage = 1;
        [SerializeField, Min(0.01f)] private float attackCooldown = 0.75f;

        [Header("Arrow")]
        [SerializeField, Min(0.01f)] private float arrowSpeed = 12f;
        [SerializeField, Min(0.01f)] private float arrowLifetime = 4f;
        [SerializeField, Min(0f)] private float launchOffset = 0.55f;
        [SerializeField] private Vector2 arrowScale = new(0.7f, 0.16f);
        [SerializeField] private Color arrowColor = new(0.75f, 0.48f, 0.18f, 1f);
        [SerializeField] private Sprite arrowSprite;

        private PlayerHealth health;
        private PlayerPassiveEffects passiveEffects;
        private SpriteRenderer sourceRenderer;
        private float nextAttackTime;
        private float activeAttackCooldown;
        private float damageRemainder;

        public bool IsAttackOnCooldown => Time.time < nextAttackTime;
        public float BaseDamage => damage;
        public float BaseAttackCooldown => attackCooldown;
        public float AttackCooldown => passiveEffects != null
            ? passiveEffects.CalculateWeaponCooldown(attackCooldown)
            : Mathf.Max(0.01f, attackCooldown);

        public float AttackCooldownProgress
        {
            get
            {
                if (!IsAttackOnCooldown || activeAttackCooldown <= 0f)
                {
                    return 1f;
                }

                float remainingCooldown = nextAttackTime - Time.time;
                return 1f - Mathf.Clamp01(remainingCooldown / activeAttackCooldown);
            }
        }

        private void Awake()
        {
            health = GetComponent<PlayerHealth>();
            passiveEffects = GetComponent<PlayerPassiveEffects>();
            sourceRenderer = GetComponent<SpriteRenderer>();
            activeAttackCooldown = AttackCooldown;
        }

        private void OnEnable()
        {
            health.Died += HandleDeath;
            if (passiveEffects != null)
            {
                passiveEffects.ModifiersChanged += HandleModifiersChanged;
            }

            if (health.IsDead)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || health.IsDead || IsAttackOnCooldown)
            {
                return;
            }

            EnemyHealth target = EnemyHealth.FindNearest(transform.position);

            if (target == null)
            {
                return;
            }

            Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            FireArrow(direction.normalized);
            activeAttackCooldown = AttackCooldown;
            nextAttackTime = Time.time + activeAttackCooldown;
        }

        private void FireArrow(Vector2 direction)
        {
            GameObject arrowObject = new("Arrow");
            arrowObject.transform.SetPositionAndRotation(
                (Vector2)transform.position + direction * launchOffset,
                Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            arrowObject.transform.localScale = new Vector3(arrowScale.x, arrowScale.y, 1f);

            SpriteRenderer arrowRenderer = arrowObject.AddComponent<SpriteRenderer>();
            arrowRenderer.sprite = arrowSprite != null ? arrowSprite : sourceRenderer.sprite;
            arrowRenderer.color = arrowColor;
            arrowRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            arrowRenderer.sortingOrder = sourceRenderer.sortingOrder + 1;

            Rigidbody2D arrowBody = arrowObject.AddComponent<Rigidbody2D>();
            arrowBody.bodyType = RigidbodyType2D.Kinematic;
            arrowBody.gravityScale = 0f;
            arrowBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            arrowBody.interpolation = RigidbodyInterpolation2D.Interpolate;

            BoxCollider2D arrowCollider = arrowObject.AddComponent<BoxCollider2D>();
            arrowCollider.isTrigger = true;

            ArrowProjectile projectile = arrowObject.AddComponent<ArrowProjectile>();
            float modifiedDamage = passiveEffects != null
                ? passiveEffects.CalculateWeaponDamage(damage)
                : damage;
            float accumulatedDamage = modifiedDamage + damageRemainder;
            int finalDamage = Mathf.Max(1, Mathf.FloorToInt(accumulatedDamage));
            damageRemainder = accumulatedDamage - finalDamage;
            projectile.Initialize(direction, arrowSpeed, arrowLifetime, finalDamage);
        }

        private void HandleModifiersChanged()
        {
            float newCooldown = AttackCooldown;
            if (IsAttackOnCooldown && activeAttackCooldown > 0f)
            {
                float remainingRatio = Mathf.Clamp01(
                    (nextAttackTime - Time.time) / activeAttackCooldown);
                nextAttackTime = Time.time + newCooldown * remainingRatio;
            }

            activeAttackCooldown = newCooldown;
        }

        private void HandleDeath()
        {
            enabled = false;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= HandleDeath;
            }

            if (passiveEffects != null)
            {
                passiveEffects.ModifiersChanged -= HandleModifiersChanged;
            }
        }

        private void OnValidate()
        {
            damage = Mathf.Max(1, damage);
            attackCooldown = Mathf.Max(0.01f, attackCooldown);
            arrowSpeed = Mathf.Max(0.01f, arrowSpeed);
            arrowLifetime = Mathf.Max(0.01f, arrowLifetime);
            launchOffset = Mathf.Max(0f, launchOffset);
            arrowScale.x = Mathf.Max(0.01f, arrowScale.x);
            arrowScale.y = Mathf.Max(0.01f, arrowScale.y);
        }
    }
}

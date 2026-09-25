using Survivors.Combat;
using Survivors.Player;
using Survivors.World;
using UnityEngine;

namespace Survivors.Enemies
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(EnemyHealth))]
    public sealed class MeleeEnemy : MonoBehaviour, IAttackCooldownSource
    {
        [Header("Target")]
        [SerializeField] private PlayerHealth player;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 2.5f;
        [SerializeField] private MapBounds2D mapBounds;

        [Header("Contact Attack")]
        [SerializeField, Min(0)] private int contactDamage = 1;
        [SerializeField, Min(0f)] private float attackCooldown = 0.75f;

        private Rigidbody2D body;
        private Collider2D enemyCollider;
        private EnemyHealth health;
        private float nextAttackTime;

        public void Initialize(PlayerHealth targetPlayer, MapBounds2D worldBounds)
        {
            player = targetPlayer;
            mapBounds = worldBounds;
            health.ConfigureExperienceTarget(targetPlayer.GetComponent<PlayerExperience>());
        }

        public bool IsAttackOnCooldown => Time.time < nextAttackTime;

        public float AttackCooldownProgress
        {
            get
            {
                if (!IsAttackOnCooldown || attackCooldown <= 0f)
                {
                    return 1f;
                }

                float remainingCooldown = nextAttackTime - Time.time;
                return 1f - Mathf.Clamp01(remainingCooldown / attackCooldown);
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            enemyCollider = GetComponent<Collider2D>();
            health = GetComponent<EnemyHealth>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void FixedUpdate()
        {
            if (player == null)
            {
                body.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = ((Vector2)player.transform.position - body.position).normalized;
            Vector2 desiredVelocity = direction * moveSpeed;

            if (mapBounds == null)
            {
                body.linearVelocity = desiredVelocity;
                return;
            }

            Vector2 padding = enemyCollider.bounds.extents;
            Vector2 nextPosition = body.position + desiredVelocity * Time.fixedDeltaTime;
            Vector2 clampedPosition = mapBounds.ClampPoint(nextPosition, padding);
            body.linearVelocity = (clampedPosition - body.position) / Time.fixedDeltaTime;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryContactAttack(collision.collider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryContactAttack(collision.collider);
        }

        private void TryContactAttack(Collider2D other)
        {
            if (player == null || Time.time < nextAttackTime)
            {
                return;
            }

            PlayerHealth contactedPlayer = other.GetComponentInParent<PlayerHealth>();
            if (contactedPlayer != player)
            {
                return;
            }

            if (player.TryTakeDamage(contactDamage))
            {
                nextAttackTime = Time.time + attackCooldown;
            }
        }

        private void OnDisable()
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
            contactDamage = Mathf.Max(0, contactDamage);
            attackCooldown = Mathf.Max(0f, attackCooldown);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 10;

        [Header("Damage Protection")]
        [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.35f;

        [Header("Death")]
        [SerializeField, Min(0f)] private float pauseDelayAfterDeath = 2f;

        private PlayerMovement movement;
        private Rigidbody2D body;
        private Collider2D playerCollider;
        private float invulnerableUntil;

        public event Action<int, int> HealthChanged;
        public event Action Died;
        public event Action GameOverReady;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => !IsDead && Time.time < invulnerableUntil;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            CurrentHealth = maxHealth;
        }

        private void Start()
        {
            HealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        public bool TryTakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || IsInvulnerable)
            {
                return false;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
            invulnerableUntil = Time.time + invulnerabilityDuration;
            HealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (CurrentHealth == 0)
            {
                Die();
            }

            return true;
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            movement.enabled = false;
            playerCollider.enabled = false;
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
            Died?.Invoke();
            StartCoroutine(PauseGameAfterDeath());
        }

        private IEnumerator PauseGameAfterDeath()
        {
            if (pauseDelayAfterDeath > 0f)
            {
                yield return new WaitForSeconds(pauseDelayAfterDeath);
            }

            Time.timeScale = 0f;
            GameOverReady?.Invoke();
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            pauseDelayAfterDeath = Mathf.Max(0f, pauseDelayAfterDeath);

            if (Application.isPlaying && !IsDead)
            {
                CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);
            }
        }
    }
}

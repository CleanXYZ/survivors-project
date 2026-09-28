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

        [Header("Base Regeneration")]
        [SerializeField, Min(0f)] private float regenerationDelayAfterDamage = 5f;
        [SerializeField, Min(0f)] private float healthRegenerationPerSecond = 1f;

        [Header("Death")]
        [SerializeField, Min(0f)] private float pauseDelayAfterDeath = 2f;

        private PlayerMovement movement;
        private Rigidbody2D body;
        private Collider2D playerCollider;
        private PlayerPassiveEffects passiveEffects;
        private float invulnerableUntil;
        private float lastDamageTime = float.NegativeInfinity;
        private float regenerationProgress;
        private int finalMaxHealth;

        public event Action<int, int> HealthChanged;
        public event Action Died;
        public event Action GameOverReady;

        public int CurrentHealth { get; private set; }
        public int BaseMaxHealth => maxHealth;
        public int MaxHealth => finalMaxHealth;
        public float BaseInvulnerabilityDuration => invulnerabilityDuration;
        public float InvulnerabilityDuration => invulnerabilityDuration
            * (passiveEffects?.InvulnerabilityDurationMultiplier ?? 1f);
        public float BaseHealthRegenerationPerSecond => healthRegenerationPerSecond;
        public float HealthRegenerationPerSecond => healthRegenerationPerSecond
            * (passiveEffects?.HealthRegenerationMultiplier ?? 1f);
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => !IsDead && Time.time < invulnerableUntil;

        private void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            passiveEffects = GetComponent<PlayerPassiveEffects>();
            finalMaxHealth = maxHealth;
            CurrentHealth = finalMaxHealth;
        }

        private void OnEnable()
        {
            if (passiveEffects != null)
            {
                passiveEffects.ModifiersChanged += RefreshPassiveStats;
            }
        }

        private void Start()
        {
            RefreshPassiveStats();
        }

        private void Update()
        {
            RegenerateHealth();
        }

        public bool TryTakeDamage(int amount)
        {
            if (amount <= 0 || IsDead || IsInvulnerable)
            {
                return false;
            }

            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, MaxHealth);
            lastDamageTime = Time.time;
            regenerationProgress = 0f;
            invulnerableUntil = Time.time + InvulnerabilityDuration;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth == 0)
            {
                Die();
            }

            return true;
        }

        private void RegenerateHealth()
        {
            if (IsDead
                || CurrentHealth >= MaxHealth
                || HealthRegenerationPerSecond <= 0f
                || Time.time - lastDamageTime < regenerationDelayAfterDamage)
            {
                return;
            }

            regenerationProgress += HealthRegenerationPerSecond * Time.deltaTime;
            int recoveredHealth = Mathf.FloorToInt(regenerationProgress);
            if (recoveredHealth <= 0)
            {
                return;
            }

            regenerationProgress -= recoveredHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + recoveredHealth);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);

            if (CurrentHealth >= MaxHealth)
            {
                regenerationProgress = 0f;
            }
        }

        private void RefreshPassiveStats()
        {
            int previousMaxHealth = Mathf.Max(1, finalMaxHealth);
            float multiplier = passiveEffects?.MaxHealthMultiplier ?? 1f;
            finalMaxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * multiplier));

            int maximumHealthChange = finalMaxHealth - previousMaxHealth;
            CurrentHealth = maximumHealthChange > 0
                ? Mathf.Min(finalMaxHealth, CurrentHealth + maximumHealthChange)
                : Mathf.Clamp(CurrentHealth, 0, finalMaxHealth);

            if (IsInvulnerable)
            {
                invulnerableUntil = lastDamageTime + InvulnerabilityDuration;
            }

            HealthChanged?.Invoke(CurrentHealth, finalMaxHealth);
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

        private void OnDisable()
        {
            if (passiveEffects != null)
            {
                passiveEffects.ModifiersChanged -= RefreshPassiveStats;
            }
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            regenerationDelayAfterDamage = Mathf.Max(0f, regenerationDelayAfterDamage);
            healthRegenerationPerSecond = Mathf.Max(0f, healthRegenerationPerSecond);
            pauseDelayAfterDeath = Mathf.Max(0f, pauseDelayAfterDeath);

            if (Application.isPlaying && !IsDead)
            {
                RefreshPassiveStats();
            }
        }
    }
}

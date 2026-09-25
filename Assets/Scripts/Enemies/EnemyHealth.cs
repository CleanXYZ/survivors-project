using System;
using System.Collections.Generic;
using Survivors.Experience;
using Survivors.Player;
using UnityEngine;

namespace Survivors.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyHealth : MonoBehaviour
    {
        private static readonly HashSet<EnemyHealth> ActiveEnemies = new();

        [SerializeField, Min(1)] private int maxHealth = 3;

        [Header("Experience Drop")]
        [SerializeField] private ExperiencePickup experiencePickupPrefab;
        [SerializeField, Min(1)] private int experienceReward = 1;

        private Collider2D targetCollider;
        private PlayerExperience experienceTarget;

        public event Action Died;

        public int CurrentHealth { get; private set; }
        public int MaxHealth => maxHealth;
        public bool IsAlive { get; private set; }
        public bool CanBeTargeted =>
            IsAlive &&
            isActiveAndEnabled &&
            gameObject.activeInHierarchy &&
            targetCollider != null &&
            targetCollider.enabled;

        private void Awake()
        {
            targetCollider = GetComponent<Collider2D>();
            CurrentHealth = maxHealth;
            IsAlive = true;
        }

        private void OnEnable()
        {
            if (IsAlive)
            {
                ActiveEnemies.Add(this);
            }
        }

        public bool TryTakeDamage(int amount)
        {
            if (amount <= 0 || !CanBeTargeted)
            {
                return false;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

            if (CurrentHealth == 0)
            {
                Die();
            }

            return true;
        }

        public void ConfigureExperienceTarget(PlayerExperience target)
        {
            experienceTarget = target;
        }

        public static EnemyHealth FindNearest(Vector2 position)
        {
            EnemyHealth nearest = null;
            float nearestDistanceSquared = float.PositiveInfinity;

            foreach (EnemyHealth enemy in ActiveEnemies)
            {
                if (enemy == null || !enemy.CanBeTargeted)
                {
                    continue;
                }

                float distanceSquared = ((Vector2)enemy.transform.position - position).sqrMagnitude;

                if (distanceSquared < nearestDistanceSquared)
                {
                    nearest = enemy;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            return nearest;
        }

        private void Die()
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            ActiveEnemies.Remove(this);
            DropExperience();
            Died?.Invoke();
            Destroy(gameObject);
        }

        private void DropExperience()
        {
            if (experiencePickupPrefab == null)
            {
                Debug.LogError($"{name} has no experience pickup prefab assigned.", this);
                return;
            }

            if (experienceTarget == null)
            {
                experienceTarget = FindAnyObjectByType<PlayerExperience>();
            }

            if (experienceTarget == null)
            {
                Debug.LogError($"{name} could not find a player experience target.", this);
                return;
            }

            ExperiencePickup.Spawn(
                experiencePickupPrefab,
                transform.position,
                experienceTarget,
                experienceReward);
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            experienceReward = Mathf.Max(1, experienceReward);

            if (Application.isPlaying && IsAlive)
            {
                CurrentHealth = Mathf.Clamp(CurrentHealth, 1, maxHealth);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegistry()
        {
            ActiveEnemies.Clear();
        }
    }
}

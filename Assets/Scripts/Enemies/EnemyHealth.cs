using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivors.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class EnemyHealth : MonoBehaviour
    {
        private static readonly HashSet<EnemyHealth> ActiveEnemies = new();

        [SerializeField, Min(1)] private int maxHealth = 3;

        private Collider2D targetCollider;

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
            Died?.Invoke();
            Destroy(gameObject);
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);

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

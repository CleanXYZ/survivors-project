using System.Collections.Generic;
using Survivors.Enemies;
using Survivors.World;
using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class ArrowProjectile : MonoBehaviour
    {
        private Rigidbody2D body;
        private float remainingLifetime;
        private PlayerBowAttack owner;
        private BowProjectileSnapshot snapshot;
        private readonly HashSet<EnemyHealth> hitEnemies = new();
        private Vector2 direction;
        private int remainingPierces;
        private bool hasHitEnemy;
        private bool isDestroyed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        internal void Initialize(
            PlayerBowAttack attackOwner,
            Vector2 shotDirection,
            float speed,
            float lifetime,
            BowProjectileSnapshot projectileSnapshot,
            EnemyHealth ignoredEnemy = null)
        {
            owner = attackOwner;
            snapshot = projectileSnapshot;
            direction = shotDirection.normalized;
            remainingLifetime = lifetime;
            remainingPierces = snapshot.RemainingPierces;
            hitEnemies.Clear();
            if (ignoredEnemy != null)
            {
                hitEnemies.Add(ignoredEnemy);
            }

            body.linearVelocity = direction * speed;
        }

        private void Update()
        {
            remainingLifetime -= Time.deltaTime;

            if (remainingLifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isDestroyed)
            {
                return;
            }

            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();

            if (enemy != null && hitEnemies.Add(enemy) && ApplyDirectDamage(enemy))
            {
                if (!hasHitEnemy)
                {
                    hasHitEnemy = true;
                    if (snapshot.CanSplit)
                    {
                        owner.SpawnSplitArrows(
                            transform.position,
                            direction,
                            snapshot,
                            enemy);
                    }
                }

                if (remainingPierces > 0)
                {
                    remainingPierces--;
                    return;
                }

                owner.CreateExplosion(transform.position, snapshot);
                DestroyProjectile();
                return;
            }

            if (other.GetComponent<MapBoundary2D>() != null)
            {
                DestroyProjectile();
            }
        }

        private void DestroyProjectile()
        {
            isDestroyed = true;
            body.linearVelocity = Vector2.zero;
            Destroy(gameObject);
        }

        private bool ApplyDirectDamage(EnemyHealth enemy)
        {
            if (owner != null)
            {
                BowDamageSource source = snapshot.IsSplitProjectile
                    ? BowDamageSource.SplitArrowDirect
                    : BowDamageSource.OriginalArrowDirect;
                return owner.ApplyBowDamage(
                    enemy,
                    snapshot.DirectDamage,
                    source,
                    snapshot.AppliesMark,
                    snapshot.MarkDamage);
            }

            return enemy.TryTakeDamage(snapshot.DirectDamage);
        }
    }
}

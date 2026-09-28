using System.Collections.Generic;
using Survivors.Combat;
using Survivors.Enemies;
using Survivors.Growth;
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

        [Header("Bow Trait Balance (Prototype)")]
        [SerializeField, Min(0f)] private float extraShotSpreadAngle = 10f;
        [SerializeField, Min(0f)] private float splitAngle = 25f;
        [SerializeField, Min(0.01f)] private float splitDamageMultiplier = 0.6f;
        [SerializeField, Min(0.01f)] private float splitScaleMultiplier = 0.7f;
        [SerializeField, Min(0.01f)] private float splitSpeedMultiplier = 1f;
        [SerializeField, Min(0f)] private float splitLaunchOffset = 0.08f;
        [SerializeField, Min(0.01f)] private float markDuration = 4f;
        [SerializeField, Min(0.01f)] private float markDamageMultiplier = 1f;
        [SerializeField, Min(0.01f)] private float explosionRadius = 1.5f;
        [SerializeField, Min(0.01f)] private float explosionDamageMultiplier = 0.75f;
        [SerializeField, Min(0f)] private float focusedFireDamageBonus = 0.25f;
        [SerializeField, Range(0f, 1f)] private float rapidFireCooldownReduction = 0.2f;

        [Header("Explosion Range QA Visual")]
        [SerializeField] private bool showExplosionRange = true;
        [SerializeField, Min(0.01f)] private float explosionRangeVisualDuration = 0.35f;
        [SerializeField] private Color explosionRangeVisualColor =
            new(1f, 0.25f, 0.05f, 0.22f);
        [SerializeField, Range(12, 128)] private int explosionRangeVisualSegments = 64;

        private PlayerHealth health;
        private PlayerPassiveEffects passiveEffects;
        private GrowthRuntimeState growthState;
        private SpriteRenderer sourceRenderer;
        private float nextAttackTime;
        private float activeAttackCooldown;
        private float damageRemainder;
        private readonly Dictionary<EnemyHealth, MarkState> marks = new();
        private readonly List<EnemyHealth> expiredMarkTargets = new();
        private readonly HashSet<EnemyHealth> explosionTargets = new();
        private float nextMarkCleanupTime;

        public bool IsAttackOnCooldown => Time.time < nextAttackTime;
        public float BaseDamage => damage;
        public float BaseAttackCooldown => attackCooldown;
        public float AttackCooldown => passiveEffects != null
            ? passiveEffects.CalculateWeaponCooldown(
                attackCooldown,
                HasTrait(BowTraitIds.RapidFire) ? rapidFireCooldownReduction : 0f)
            : Mathf.Max(
                0.01f,
                attackCooldown * (1f -
                    (HasTrait(BowTraitIds.RapidFire) ? rapidFireCooldownReduction : 0f)));

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
            growthState = GetComponent<GrowthRuntimeState>();
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
            CleanupExpiredMarks();

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

            FireArrows(direction.normalized);
            activeAttackCooldown = AttackCooldown;
            nextAttackTime = Time.time + activeAttackCooldown;
        }

        private void FireArrows(Vector2 direction)
        {
            float weaponDamageBonus = HasTrait(BowTraitIds.FocusedFire)
                ? focusedFireDamageBonus
                : 0f;
            float modifiedDamage = passiveEffects != null
                ? passiveEffects.CalculateWeaponDamage(damage, weaponDamageBonus)
                : damage * (1f + weaponDamageBonus);
            float accumulatedDamage = modifiedDamage + damageRemainder;
            int finalDamage = Mathf.Max(1, Mathf.FloorToInt(accumulatedDamage));
            damageRemainder = accumulatedDamage - finalDamage;

            BowProjectileSnapshot snapshot = new(
                finalDamage,
                HasTrait(BowTraitIds.Piercing) ? 1 : 0,
                HasTrait(BowTraitIds.SplitShot),
                HasTrait(BowTraitIds.MarkingShot),
                HasTrait(BowTraitIds.ExplosiveShot),
                ScaleDamage(finalDamage, markDamageMultiplier),
                ScaleDamage(finalDamage, explosionDamageMultiplier));

            if (!HasTrait(BowTraitIds.ExtraShot))
            {
                SpawnArrow(transform.position, direction, arrowScale, arrowSpeed, snapshot);
                return;
            }

            float halfSpread = extraShotSpreadAngle * 0.5f;
            SpawnArrow(
                transform.position,
                Rotate(direction, -halfSpread),
                arrowScale,
                arrowSpeed,
                snapshot);
            SpawnArrow(
                transform.position,
                Rotate(direction, halfSpread),
                arrowScale,
                arrowSpeed,
                snapshot);
        }

        private void SpawnArrow(
            Vector2 origin,
            Vector2 direction,
            Vector2 scale,
            float speed,
            BowProjectileSnapshot snapshot,
            EnemyHealth ignoredEnemy = null,
            float offset = -1f)
        {
            float appliedOffset = offset >= 0f ? offset : launchOffset;
            GameObject arrowObject = new("Arrow");
            arrowObject.transform.SetPositionAndRotation(
                origin + direction * appliedOffset,
                Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
            arrowObject.transform.localScale = new Vector3(scale.x, scale.y, 1f);

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
            projectile.Initialize(
                this,
                direction,
                speed,
                arrowLifetime,
                snapshot,
                ignoredEnemy);
        }

        internal bool ApplyBowDamage(
            EnemyHealth enemy,
            int amount,
            BowDamageSource source,
            bool appliesMark = false,
            int markDamage = 0)
        {
            if (enemy == null || !enemy.TryTakeDamage(amount))
            {
                return false;
            }

            if ((source == BowDamageSource.OriginalArrowDirect
                    || source == BowDamageSource.SplitArrowDirect)
                && appliesMark
                && enemy.IsAlive)
            {
                ApplyMark(enemy, markDamage);
            }

            return true;
        }

        internal void SpawnSplitArrows(
            Vector2 hitPosition,
            Vector2 incomingDirection,
            BowProjectileSnapshot parentSnapshot,
            EnemyHealth hitEnemy)
        {
            int splitDamage = ScaleDamage(
                parentSnapshot.DirectDamage,
                splitDamageMultiplier);
            BowProjectileSnapshot splitSnapshot = new(
                splitDamage,
                parentSnapshot.RemainingPierces,
                false,
                parentSnapshot.AppliesMark,
                parentSnapshot.Explodes,
                parentSnapshot.MarkDamage,
                ScaleDamage(splitDamage, explosionDamageMultiplier),
                true);
            Vector2 splitScale = arrowScale * splitScaleMultiplier;
            float splitSpeed = arrowSpeed * splitSpeedMultiplier;

            SpawnArrow(
                hitPosition,
                Rotate(incomingDirection, -splitAngle),
                splitScale,
                splitSpeed,
                splitSnapshot,
                hitEnemy,
                splitLaunchOffset);
            SpawnArrow(
                hitPosition,
                Rotate(incomingDirection, splitAngle),
                splitScale,
                splitSpeed,
                splitSnapshot,
                hitEnemy,
                splitLaunchOffset);
        }

        internal void CreateExplosion(
            Vector2 position,
            BowProjectileSnapshot snapshot)
        {
            if (!snapshot.Explodes)
            {
                return;
            }

            if (showExplosionRange)
            {
                BowExplosionRangeVisual.Spawn(
                    position,
                    explosionRadius,
                    explosionRangeVisualDuration,
                    explosionRangeVisualColor,
                    explosionRangeVisualSegments,
                    sourceRenderer.sortingLayerID,
                    sourceRenderer.sortingOrder + 2);
            }

            explosionTargets.Clear();
            foreach (Collider2D hit in Physics2D.OverlapCircleAll(position, explosionRadius))
            {
                EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
                if (enemy != null && explosionTargets.Add(enemy))
                {
                    ApplyBowDamage(
                        enemy,
                        snapshot.ExplosionDamage,
                        BowDamageSource.Explosion);
                }
            }
        }

        private void ApplyMark(EnemyHealth enemy, int markDamage)
        {
            float now = Time.time;
            if (!marks.TryGetValue(enemy, out MarkState mark) || now > mark.ExpiresAt)
            {
                mark = new MarkState();
            }

            mark.Stacks++;
            mark.ExpiresAt = now + markDuration;

            if (mark.Stacks >= 3)
            {
                marks.Remove(enemy);
                ApplyBowDamage(
                    enemy,
                    markDamage,
                    BowDamageSource.MarkDetonation);
                return;
            }

            marks[enemy] = mark;
        }

        private void CleanupExpiredMarks()
        {
            if (marks.Count == 0 || Time.time < nextMarkCleanupTime)
            {
                return;
            }

            nextMarkCleanupTime = Time.time + 0.5f;
            expiredMarkTargets.Clear();
            foreach (KeyValuePair<EnemyHealth, MarkState> entry in marks)
            {
                if (entry.Key == null || Time.time > entry.Value.ExpiresAt)
                {
                    expiredMarkTargets.Add(entry.Key);
                }
            }

            foreach (EnemyHealth enemy in expiredMarkTargets)
            {
                marks.Remove(enemy);
            }
        }

        private bool HasTrait(string traitId)
        {
            return growthState != null
                && growthState.IsTraitSelected(BowTraitIds.Weapon, traitId);
        }

        private static int ScaleDamage(int baseDamage, float multiplier)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * multiplier));
        }

        private static Vector2 Rotate(Vector2 direction, float degrees)
        {
            return Quaternion.Euler(0f, 0f, degrees) * direction;
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
            extraShotSpreadAngle = Mathf.Max(0f, extraShotSpreadAngle);
            splitAngle = Mathf.Max(0f, splitAngle);
            splitDamageMultiplier = Mathf.Max(0.01f, splitDamageMultiplier);
            splitScaleMultiplier = Mathf.Max(0.01f, splitScaleMultiplier);
            splitSpeedMultiplier = Mathf.Max(0.01f, splitSpeedMultiplier);
            splitLaunchOffset = Mathf.Max(0f, splitLaunchOffset);
            markDuration = Mathf.Max(0.01f, markDuration);
            markDamageMultiplier = Mathf.Max(0.01f, markDamageMultiplier);
            explosionRadius = Mathf.Max(0.01f, explosionRadius);
            explosionDamageMultiplier = Mathf.Max(0.01f, explosionDamageMultiplier);
            focusedFireDamageBonus = Mathf.Max(0f, focusedFireDamageBonus);
            rapidFireCooldownReduction = Mathf.Clamp01(rapidFireCooldownReduction);
            explosionRangeVisualDuration = Mathf.Max(0.01f, explosionRangeVisualDuration);
            explosionRangeVisualSegments = Mathf.Clamp(
                explosionRangeVisualSegments,
                12,
                128);
        }

        private sealed class MarkState
        {
            public int Stacks;
            public float ExpiresAt;
        }
    }

    internal enum BowDamageSource
    {
        OriginalArrowDirect,
        SplitArrowDirect,
        MarkDetonation,
        Explosion
    }

    internal readonly struct BowProjectileSnapshot
    {
        public readonly int DirectDamage;
        public readonly int RemainingPierces;
        public readonly bool CanSplit;
        public readonly bool AppliesMark;
        public readonly bool Explodes;
        public readonly int MarkDamage;
        public readonly int ExplosionDamage;
        public readonly bool IsSplitProjectile;

        public BowProjectileSnapshot(
            int directDamage,
            int remainingPierces,
            bool canSplit,
            bool appliesMark,
            bool explodes,
            int markDamage,
            int explosionDamage,
            bool isSplitProjectile = false)
        {
            DirectDamage = directDamage;
            RemainingPierces = remainingPierces;
            CanSplit = canSplit;
            AppliesMark = appliesMark;
            Explodes = explodes;
            MarkDamage = markDamage;
            ExplosionDamage = explosionDamage;
            IsSplitProjectile = isSplitProjectile;
        }
    }

    internal sealed class BowExplosionRangeVisual : MonoBehaviour
    {
        private Mesh circleMesh;
        private Material circleMaterial;
        private Color initialColor;
        private float duration;
        private float remainingDuration;

        internal static void Spawn(
            Vector2 position,
            float radius,
            float visualDuration,
            Color color,
            int segments,
            int sortingLayerId,
            int sortingOrder)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("Bow explosion range visual could not find Sprites/Default shader.");
                return;
            }

            GameObject visualObject = new("__BowExplosionRangeQA");
            visualObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            visualObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            visualObject.transform.localScale = new Vector3(radius, radius, 1f);

            BowExplosionRangeVisual visual = visualObject.AddComponent<BowExplosionRangeVisual>();
            visual.Build(shader, visualDuration, color, segments, sortingLayerId, sortingOrder);
        }

        private void Build(
            Shader shader,
            float visualDuration,
            Color color,
            int segments,
            int sortingLayerId,
            int sortingOrder)
        {
            duration = Mathf.Max(0.01f, visualDuration);
            remainingDuration = duration;
            initialColor = color;

            circleMesh = CreateCircleMesh(Mathf.Clamp(segments, 12, 128));
            circleMesh.name = "RuntimeBowExplosionRangeMesh";
            circleMesh.hideFlags = HideFlags.HideAndDontSave;

            circleMaterial = new Material(shader)
            {
                color = initialColor,
                hideFlags = HideFlags.HideAndDontSave
            };

            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = circleMesh;

            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = circleMaterial;
            renderer.sortingLayerID = sortingLayerId;
            renderer.sortingOrder = sortingOrder;
        }

        private void Update()
        {
            remainingDuration -= Time.deltaTime;
            if (remainingDuration <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            Color fadedColor = initialColor;
            fadedColor.a *= Mathf.Clamp01(remainingDuration / duration);
            circleMaterial.color = fadedColor;
        }

        private static Mesh CreateCircleMesh(int segments)
        {
            Vector3[] vertices = new Vector3[segments + 1];
            int[] triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = (i + 1) % segments + 1;
            }

            Mesh mesh = new();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (circleMesh != null)
            {
                Destroy(circleMesh);
            }

            if (circleMaterial != null)
            {
                Destroy(circleMaterial);
            }
        }
    }
}

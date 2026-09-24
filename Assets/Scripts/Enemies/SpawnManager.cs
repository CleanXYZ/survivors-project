using System.Collections.Generic;
using Survivors.Player;
using Survivors.World;
using UnityEngine;

namespace Survivors.Enemies
{
    public sealed class SpawnManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MeleeEnemy enemyPrefab;
        [SerializeField] private PlayerHealth player;
        [SerializeField] private Camera spawnCamera;
        [SerializeField] private MapBounds2D mapBounds;

        [Header("Spawn Timing")]
        [SerializeField, Min(0.01f)] private float spawnInterval = 3f;
        [SerializeField, Min(1)] private int spawnCount = 1;

        [Header("Spawn Position")]
        [SerializeField, Min(0.01f)] private float spawnBandWidth = 4f;
        [SerializeField, Min(0f)] private float offscreenPadding = 0.5f;
        [SerializeField, Min(0f)] private float minimumPlayerDistance = 6f;
        [SerializeField, Min(0f)] private float minimumSpawnSpacing = 1.5f;
        [SerializeField, Min(0f)] private float mapEdgePadding = 0.5f;
        [SerializeField, Min(1)] private int maxPositionAttempts = 30;

        private readonly List<Vector2> positionsThisCycle = new();
        private float spawnTimer;

        private void Awake()
        {
            if (enemyPrefab == null || player == null || spawnCamera == null || mapBounds == null)
            {
                Debug.LogError("SpawnManager requires an enemy prefab, player, camera, and map bounds.", this);
                enabled = false;
                return;
            }

            if (!spawnCamera.orthographic)
            {
                Debug.LogError("SpawnManager requires an orthographic camera.", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (Time.timeScale <= 0f || player.IsDead)
            {
                return;
            }

            spawnTimer += Time.deltaTime;

            if (spawnTimer < spawnInterval)
            {
                return;
            }

            spawnTimer = 0f;
            SpawnCycle();
        }

        private void SpawnCycle()
        {
            positionsThisCycle.Clear();

            for (int i = 0; i < spawnCount; i++)
            {
                if (!TryFindSpawnPosition(out Vector2 spawnPosition))
                {
                    continue;
                }

                positionsThisCycle.Add(spawnPosition);
                CreateEnemy(spawnPosition);
            }
        }

        private bool TryFindSpawnPosition(out Vector2 spawnPosition)
        {
            GetCameraRects(out Rect visibleRect, out Rect expandedRect);
            Rect offscreenExclusionRect = Rect.MinMaxRect(
                visibleRect.xMin - offscreenPadding,
                visibleRect.yMin - offscreenPadding,
                visibleRect.xMax + offscreenPadding,
                visibleRect.yMax + offscreenPadding);
            Bounds worldBounds = mapBounds.WorldBounds;
            Vector2 edgePadding = Vector2.one * mapEdgePadding;
            float minimumPlayerDistanceSquared = minimumPlayerDistance * minimumPlayerDistance;
            float minimumSpawnSpacingSquared = minimumSpawnSpacing * minimumSpawnSpacing;

            for (int attempt = 0; attempt < maxPositionAttempts; attempt++)
            {
                Vector2 candidate = new(
                    Random.Range(expandedRect.xMin, expandedRect.xMax),
                    Random.Range(expandedRect.yMin, expandedRect.yMax));

                if (candidate.x >= offscreenExclusionRect.xMin &&
                    candidate.x <= offscreenExclusionRect.xMax &&
                    candidate.y >= offscreenExclusionRect.yMin &&
                    candidate.y <= offscreenExclusionRect.yMax)
                {
                    continue;
                }

                Vector2 clampedCandidate = mapBounds.ClampPoint(candidate, edgePadding);

                if ((clampedCandidate - candidate).sqrMagnitude > Mathf.Epsilon ||
                    !worldBounds.Contains(new Vector3(candidate.x, candidate.y, worldBounds.center.z)))
                {
                    continue;
                }

                if ((candidate - (Vector2)player.transform.position).sqrMagnitude < minimumPlayerDistanceSquared)
                {
                    continue;
                }

                if (IsTooCloseToCurrentCycle(candidate, minimumSpawnSpacingSquared))
                {
                    continue;
                }

                spawnPosition = candidate;
                return true;
            }

            spawnPosition = default;
            return false;
        }

        private void GetCameraRects(out Rect visibleRect, out Rect expandedRect)
        {
            Vector2 center = spawnCamera.transform.position;
            float halfHeight = spawnCamera.orthographicSize;
            float halfWidth = halfHeight * spawnCamera.aspect;

            visibleRect = Rect.MinMaxRect(
                center.x - halfWidth,
                center.y - halfHeight,
                center.x + halfWidth,
                center.y + halfHeight);
            expandedRect = Rect.MinMaxRect(
                visibleRect.xMin - spawnBandWidth,
                visibleRect.yMin - spawnBandWidth,
                visibleRect.xMax + spawnBandWidth,
                visibleRect.yMax + spawnBandWidth);
        }

        private bool IsTooCloseToCurrentCycle(Vector2 candidate, float minimumSpacingSquared)
        {
            foreach (Vector2 existingPosition in positionsThisCycle)
            {
                if ((candidate - existingPosition).sqrMagnitude < minimumSpacingSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private void CreateEnemy(Vector2 spawnPosition)
        {
            MeleeEnemy enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.Initialize(player, mapBounds);
        }

        private void OnValidate()
        {
            spawnInterval = Mathf.Max(0.01f, spawnInterval);
            spawnCount = Mathf.Max(1, spawnCount);
            spawnBandWidth = Mathf.Max(0.01f, spawnBandWidth);
            offscreenPadding = Mathf.Max(0f, offscreenPadding);
            minimumPlayerDistance = Mathf.Max(0f, minimumPlayerDistance);
            minimumSpawnSpacing = Mathf.Max(0f, minimumSpawnSpacing);
            mapEdgePadding = Mathf.Max(0f, mapEdgePadding);
            maxPositionAttempts = Mathf.Max(1, maxPositionAttempts);
        }
    }
}

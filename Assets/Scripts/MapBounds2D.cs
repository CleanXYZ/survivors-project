using UnityEngine;

namespace Survivors.World
{
    public sealed class MapBounds2D : MonoBehaviour
    {
        private const string VisualRootName = "__MapDebugVisuals";

        [Header("Map")]
        [SerializeField] private Vector2 mapSize = new(40f, 30f);

        [Header("Prototype Grid")]
        [SerializeField, Min(0.1f)] private float gridSpacing = 1f;
        [SerializeField, Min(0.001f)] private float gridLineWidth = 0.025f;
        [SerializeField, Min(0.001f)] private float borderLineWidth = 0.12f;
        [SerializeField] private Color floorColor = new(0.07f, 0.09f, 0.12f, 1f);
        [SerializeField] private Color gridColor = new(0.2f, 0.32f, 0.42f, 0.65f);
        [SerializeField] private Color borderColor = new(1f, 0.55f, 0.12f, 1f);

        [Header("Physics Boundary")]
        [SerializeField, Min(0.1f)] private float boundaryThickness = 1f;

        private Material lineMaterial;
        private Sprite floorSprite;

        public Bounds WorldBounds => new(transform.position, new Vector3(mapSize.x, mapSize.y, 0f));

        public Vector2 ClampPoint(Vector2 point, Vector2 padding)
        {
            Bounds bounds = WorldBounds;
            padding = new Vector2(Mathf.Max(0f, padding.x), Mathf.Max(0f, padding.y));

            float minX = bounds.min.x + padding.x;
            float maxX = bounds.max.x - padding.x;
            float minY = bounds.min.y + padding.y;
            float maxY = bounds.max.y - padding.y;

            float x = minX <= maxX ? Mathf.Clamp(point.x, minX, maxX) : bounds.center.x;
            float y = minY <= maxY ? Mathf.Clamp(point.y, minY, maxY) : bounds.center.y;
            return new Vector2(x, y);
        }

        private void OnEnable()
        {
            RebuildVisuals();
        }

        private void OnDisable()
        {
            ClearVisuals();
        }

        private void OnValidate()
        {
            mapSize.x = Mathf.Max(0.1f, mapSize.x);
            mapSize.y = Mathf.Max(0.1f, mapSize.y);
            gridSpacing = Mathf.Max(0.1f, gridSpacing);
            gridLineWidth = Mathf.Max(0.001f, gridLineWidth);
            borderLineWidth = Mathf.Max(0.001f, borderLineWidth);
            boundaryThickness = Mathf.Max(0.1f, boundaryThickness);
        }

        private void RebuildVisuals()
        {
            ClearVisuals();

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("MapBounds2D could not find the Sprites/Default shader.", this);
                return;
            }

            lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            GameObject root = new(VisualRootName);
            root.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            root.transform.SetParent(transform, false);

            CreateFloor(root.transform);
            CreateGrid(root.transform);
            CreateBorder(root.transform);
            CreateBoundaryColliders(root.transform);
        }

        private void CreateFloor(Transform parent)
        {
            GameObject floor = new("Floor");
            floor.transform.SetParent(parent, false);
            floorSprite = Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f),
                1f);
            floorSprite.hideFlags = HideFlags.HideAndDontSave;

            SpriteRenderer renderer = floor.AddComponent<SpriteRenderer>();
            renderer.sprite = floorSprite;
            renderer.color = floorColor;
            renderer.sortingOrder = -100;
            floor.transform.localScale = mapSize;
        }

        private void CreateGrid(Transform parent)
        {
            Vector2 halfSize = mapSize * 0.5f;

            for (float x = -halfSize.x + gridSpacing; x < halfSize.x; x += gridSpacing)
            {
                CreateLine(parent, new Vector2(x, -halfSize.y), new Vector2(x, halfSize.y), gridColor, gridLineWidth, -90);
            }

            for (float y = -halfSize.y + gridSpacing; y < halfSize.y; y += gridSpacing)
            {
                CreateLine(parent, new Vector2(-halfSize.x, y), new Vector2(halfSize.x, y), gridColor, gridLineWidth, -90);
            }
        }

        private void CreateBorder(Transform parent)
        {
            Vector2 halfSize = mapSize * 0.5f;
            CreateLine(parent, new Vector2(-halfSize.x, -halfSize.y), new Vector2(halfSize.x, -halfSize.y), borderColor, borderLineWidth, -80);
            CreateLine(parent, new Vector2(halfSize.x, -halfSize.y), new Vector2(halfSize.x, halfSize.y), borderColor, borderLineWidth, -80);
            CreateLine(parent, new Vector2(halfSize.x, halfSize.y), new Vector2(-halfSize.x, halfSize.y), borderColor, borderLineWidth, -80);
            CreateLine(parent, new Vector2(-halfSize.x, halfSize.y), new Vector2(-halfSize.x, -halfSize.y), borderColor, borderLineWidth, -80);
        }

        private void CreateBoundaryColliders(Transform parent)
        {
            Vector2 halfSize = mapSize * 0.5f;
            float halfThickness = boundaryThickness * 0.5f;

            CreateBoundaryCollider(
                parent,
                "Bottom Boundary",
                new Vector2(0f, -halfSize.y - halfThickness),
                new Vector2(mapSize.x + boundaryThickness * 2f, boundaryThickness));
            CreateBoundaryCollider(
                parent,
                "Top Boundary",
                new Vector2(0f, halfSize.y + halfThickness),
                new Vector2(mapSize.x + boundaryThickness * 2f, boundaryThickness));
            CreateBoundaryCollider(
                parent,
                "Left Boundary",
                new Vector2(-halfSize.x - halfThickness, 0f),
                new Vector2(boundaryThickness, mapSize.y));
            CreateBoundaryCollider(
                parent,
                "Right Boundary",
                new Vector2(halfSize.x + halfThickness, 0f),
                new Vector2(boundaryThickness, mapSize.y));
        }

        private static void CreateBoundaryCollider(
            Transform parent,
            string objectName,
            Vector2 localPosition,
            Vector2 size)
        {
            GameObject boundary = new(objectName);
            boundary.transform.SetParent(parent, false);
            boundary.transform.localPosition = localPosition;

            BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private void CreateLine(
            Transform parent,
            Vector2 start,
            Vector2 end,
            Color color,
            float width,
            int sortingOrder)
        {
            GameObject lineObject = new("Line");
            lineObject.transform.SetParent(parent, false);

            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.startWidth = width;
            line.endWidth = width;
            line.startColor = color;
            line.endColor = color;
            line.sharedMaterial = lineMaterial;
            line.numCapVertices = 0;
            line.sortingOrder = sortingOrder;
        }

        private void ClearVisuals()
        {
            Transform existing = transform.Find(VisualRootName);
            if (existing != null)
            {
                DestroyGeneratedObject(existing.gameObject);
            }

            if (floorSprite != null)
            {
                DestroyGeneratedObject(floorSprite);
                floorSprite = null;
            }

            if (lineMaterial != null)
            {
                DestroyGeneratedObject(lineMaterial);
                lineMaterial = null;
            }
        }

        private static void DestroyGeneratedObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = borderColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(mapSize.x, mapSize.y, 0f));
        }
    }
}

using UnityEngine;

namespace Survivors.Player
{
    [RequireComponent(typeof(PlayerExperience))]
    public sealed class PlayerExperienceRangeVisual : MonoBehaviour
    {
        private const string VisualRootName = "__ExperiencePickupRange";

        [Header("Appearance")]
        [SerializeField] private Color rangeColor = new(0.15f, 0.65f, 1f, 0.12f);
        [SerializeField, Range(12, 128)] private int circleSegments = 64;
        [SerializeField] private int sortingOrder = -70;

        private PlayerExperience experience;
        private GameObject visualRoot;
        private Mesh circleMesh;
        private Material circleMaterial;
        private float displayedRadius = -1f;

        private void Awake()
        {
            experience = GetComponent<PlayerExperience>();
            BuildVisual();
            RefreshRadius();
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(displayedRadius, experience.PickupRadius))
            {
                RefreshRadius();
            }
        }

        private void BuildVisual()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogError("PlayerExperienceRangeVisual could not find the Sprites/Default shader.", this);
                enabled = false;
                return;
            }

            visualRoot = new GameObject(VisualRootName);
            visualRoot.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            visualRoot.transform.SetParent(transform, false);

            circleMesh = CreateCircleMesh(circleSegments);
            circleMesh.name = "RuntimeExperiencePickupRangeMesh";
            circleMesh.hideFlags = HideFlags.HideAndDontSave;

            circleMaterial = new Material(shader)
            {
                color = rangeColor,
                hideFlags = HideFlags.HideAndDontSave
            };

            MeshFilter filter = visualRoot.AddComponent<MeshFilter>();
            filter.sharedMesh = circleMesh;

            MeshRenderer renderer = visualRoot.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = circleMaterial;
            renderer.sortingOrder = sortingOrder;
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

        private void RefreshRadius()
        {
            displayedRadius = experience.PickupRadius;

            if (visualRoot == null)
            {
                return;
            }

            visualRoot.SetActive(displayedRadius > 0f);
            visualRoot.transform.localScale = new Vector3(displayedRadius, displayedRadius, 1f);
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

        private void OnValidate()
        {
            circleSegments = Mathf.Clamp(circleSegments, 12, 128);
        }
    }
}

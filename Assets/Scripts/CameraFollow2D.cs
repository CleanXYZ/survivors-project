using UnityEngine;

namespace Survivors.World
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private MapBounds2D mapBounds;

        private Camera targetCamera;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null || mapBounds == null || !targetCamera.orthographic)
            {
                return;
            }

            float halfHeight = targetCamera.orthographicSize;
            float halfWidth = halfHeight * targetCamera.aspect;
            Vector2 cameraCenter = mapBounds.ClampPoint(target.position, new Vector2(halfWidth, halfHeight));

            transform.position = new Vector3(cameraCenter.x, cameraCenter.y, transform.position.z);
        }
    }
}

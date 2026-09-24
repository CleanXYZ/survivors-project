using UnityEngine;
using UnityEngine.InputSystem;
using Survivors.World;

namespace Survivors.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";

        [Header("Map Boundary")]
        [SerializeField] private MapBounds2D mapBounds;

        private Rigidbody2D body;
        private Collider2D playerCollider;
        private InputAction moveAction;
        private Vector2 moveInput;
        private bool enabledActionHere;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            playerCollider = GetComponent<Collider2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void OnEnable()
        {
            moveAction = FindMoveAction();
            enabledActionHere = !moveAction.enabled;

            if (enabledActionHere)
            {
                moveAction.Enable();
            }
        }

        private void Update()
        {
            moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        }

        private void FixedUpdate()
        {
            Vector2 desiredVelocity = moveInput * moveSpeed;

            if (mapBounds == null)
            {
                body.linearVelocity = desiredVelocity;
                return;
            }

            Vector2 padding = playerCollider != null ? playerCollider.bounds.extents : Vector2.zero;
            Vector2 nextPosition = body.position + desiredVelocity * Time.fixedDeltaTime;
            Vector2 clampedPosition = mapBounds.ClampPoint(nextPosition, padding);

            body.linearVelocity = ResolveBoundaryVelocity(
                desiredVelocity,
                body.position,
                nextPosition,
                clampedPosition,
                Time.fixedDeltaTime);
        }

        private static Vector2 ResolveBoundaryVelocity(
            Vector2 desiredVelocity,
            Vector2 currentPosition,
            Vector2 desiredPosition,
            Vector2 clampedPosition,
            float fixedDeltaTime)
        {
            Vector2 clampedVelocity = (clampedPosition - currentPosition) / fixedDeltaTime;
            bool blockedX = !Mathf.Approximately(clampedPosition.x, desiredPosition.x);
            bool blockedY = !Mathf.Approximately(clampedPosition.y, desiredPosition.y);

            if (blockedX == blockedY)
            {
                return blockedX ? clampedVelocity : desiredVelocity;
            }

            float desiredSpeedSquared = desiredVelocity.sqrMagnitude;

            if (blockedX && !Mathf.Approximately(desiredVelocity.y, 0f))
            {
                float parallelSpeed = Mathf.Sqrt(Mathf.Max(
                    0f,
                    desiredSpeedSquared - clampedVelocity.x * clampedVelocity.x));
                clampedVelocity.y = Mathf.Sign(desiredVelocity.y) * parallelSpeed;
            }
            else if (blockedY && !Mathf.Approximately(desiredVelocity.x, 0f))
            {
                float parallelSpeed = Mathf.Sqrt(Mathf.Max(
                    0f,
                    desiredSpeedSquared - clampedVelocity.y * clampedVelocity.y));
                clampedVelocity.x = Mathf.Sign(desiredVelocity.x) * parallelSpeed;
            }

            return clampedVelocity;
        }

        private void OnDisable()
        {
            moveInput = Vector2.zero;

            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }

            if (enabledActionHere && moveAction != null)
            {
                moveAction.Disable();
            }

            enabledActionHere = false;
            moveAction = null;
        }

        private InputAction FindMoveAction()
        {
            if (inputActions == null)
            {
                throw new MissingReferenceException(
                    $"{nameof(PlayerMovement)} on '{name}' needs an Input Action Asset.");
            }

            InputActionMap actionMap = inputActions.FindActionMap(actionMapName, true);
            return actionMap.FindAction(moveActionName, true);
        }

        private void OnValidate()
        {
            moveSpeed = Mathf.Max(0f, moveSpeed);
        }
    }
}

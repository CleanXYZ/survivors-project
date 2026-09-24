using UnityEngine;
using UnityEngine.InputSystem;

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

        private Rigidbody2D body;
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
            body.linearVelocity = moveInput * moveSpeed;
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

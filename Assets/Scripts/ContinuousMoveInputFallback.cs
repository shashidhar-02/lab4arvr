using UnityEngine;
using UnityEngine.InputSystem;

namespace Assignment4
{
    /// <summary>
    /// Auxiliary continuous movement driver for editor Play Mode testing with XR Device Simulator.
    /// Allows immediate keyboard WASD / Arrow key movement relative to the XR Camera heading,
    /// driving the CharacterController with full physical obstacle collision.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ContinuousMoveInputFallback : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Movement speed in meters per second.")]
        public float moveSpeed = 3.0f;

        [Tooltip("Transform providing the forward direction (XR Camera).")]
        public Transform forwardSource;

        [Tooltip("Enable gravity simulation for grounding on ramps and floors.")]
        public bool useGravity = true;
        public float gravity = 9.81f;

        private CharacterController _characterController;
        private float _verticalVelocity = 0f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            if (forwardSource == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null)
                {
                    forwardSource = cam.transform;
                }
            }
        }

        private void Update()
        {
            if (_characterController == null) return;

            Vector2 moveInput = Vector2.zero;

            // Read keyboard input via InputSystem
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveInput.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveInput.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput.x += 1f;
            }

            // Fallback to legacy input if active
            if (moveInput.sqrMagnitude < 0.01f)
            {
                try
                {
                    float h = Input.GetAxisRaw("Horizontal");
                    float v = Input.GetAxisRaw("Vertical");
                    moveInput = new Vector2(h, v);
                }
                catch { }
            }

            // Determine heading relative to camera/headset
            Vector3 forward = forwardSource != null ? forwardSource.forward : transform.forward;
            Vector3 right = forwardSource != null ? forwardSource.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * moveInput.y + right * moveInput.x).normalized;
            Vector3 motion = direction * (moveSpeed * Time.deltaTime);

            // Apply gravity
            if (useGravity)
            {
                if (_characterController.isGrounded)
                {
                    _verticalVelocity = -0.5f; // Keep grounded on slopes
                }
                else
                {
                    _verticalVelocity -= gravity * Time.deltaTime;
                }
                motion.y = _verticalVelocity * Time.deltaTime;
            }

            _characterController.Move(motion);
        }
    }
}

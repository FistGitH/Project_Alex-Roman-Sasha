using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace OriginalWater
{
    /// <summary>
    /// Lightweight free-fly camera controller designed for exploring the demo scene
    /// and testing underwater transitions. Supports the new Unity Input System.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Original Water/Demo Camera Controller")]
    public class DemoCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float sprintMultiplier = 2.2f;
        [SerializeField] private float lookSensitivity = 1.8f;
        [SerializeField] private bool holdRightClickToLook = true;

        private float _yaw;
        private float _pitch;

        private void Start()
        {
            Vector3 angles = transform.eulerAngles;
            _pitch = angles.x;
            _yaw = angles.y;
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (kb == null || mouse == null) return;

            // Rotation (Mouse)
            bool canLook = !holdRightClickToLook || mouse.rightButton.isPressed;
            if (canLook)
            {
                Vector2 delta = mouse.delta.ReadValue() * 0.1f * lookSensitivity;
                _yaw += delta.x;
                _pitch -= delta.y;
                _pitch = Mathf.Clamp(_pitch, -85f, 85f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            // Movement direction
            Vector3 direction = Vector3.zero;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) direction += transform.forward;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) direction -= transform.forward;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) direction += transform.right;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) direction -= transform.right;

            // Up / Down
            if (kb.spaceKey.isPressed || kb.eKey.isPressed) direction += Vector3.up;
            if (kb.qKey.isPressed || kb.cKey.isPressed) direction -= Vector3.up;

            // Sprint
            float currentSpeed = moveSpeed;
            if (kb.leftShiftKey.isPressed)
            {
                currentSpeed *= sprintMultiplier;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.position += direction.normalized * (currentSpeed * Time.deltaTime);
            }
#else
            // Fallback for legacy input
            bool canLook = !holdRightClickToLook || Input.GetMouseButton(1);
            if (canLook)
            {
                _yaw += Input.GetAxis("Mouse X") * lookSensitivity * 2f;
                _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity * 2f;
                _pitch = Mathf.Clamp(_pitch, -85f, 85f);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }

            Vector3 dir = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) dir += transform.forward;
            if (Input.GetKey(KeyCode.S)) dir -= transform.forward;
            if (Input.GetKey(KeyCode.D)) dir += transform.right;
            if (Input.GetKey(KeyCode.A)) dir -= transform.right;
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) dir += Vector3.up;
            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.C)) dir -= Vector3.up;

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.position += dir.normalized * (speed * Time.deltaTime);
            }
#endif
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class CarThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform car;

    [Header("Input")]
    [SerializeField] private InputActionReference look;

    [Header("Camera")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2.2f;

    [SerializeField] private float sensitivityX = 120f;
    [SerializeField] private float sensitivityY = 80f;

    [SerializeField] private float smoothSpeed = 10f;

    [Header("Vertical Rotation")]
    [SerializeField] private float minY = -15f;
    [SerializeField] private float maxY = 55f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.3f;

    private float yaw;
    private float pitch = 15f;

    private void OnEnable()
    {
        if (look != null)
            look.action.Enable();

        if (car != null)
            yaw = car.eulerAngles.y;
    }

    private void OnDisable()
    {
        if (look != null)
            look.action.Disable();
    }

    private void LateUpdate()
    {
        if (car == null)
            return;

        Vector2 lookInput =
            look.action.ReadValue<Vector2>();

        yaw +=
            lookInput.x *
            sensitivityX *
            Time.deltaTime;

        pitch -=
            lookInput.y *
            sensitivityY *
            Time.deltaTime;

        pitch =
            Mathf.Clamp(
                pitch,
                minY,
                maxY
            );

        Quaternion rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        Vector3 targetPosition =
            car.position +
            Vector3.up * height;

        Vector3 desiredPosition =
            targetPosition -
            rotation * Vector3.forward * distance;

        // Не даём камере проходить через стены
        Vector3 direction =
            desiredPosition - targetPosition;

        float targetDistance =
            direction.magnitude;

        direction.Normalize();

        if (Physics.SphereCast(
                targetPosition,
                collisionRadius,
                direction,
                out RaycastHit hit,
                targetDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            desiredPosition =
                targetPosition +
                direction *
                Mathf.Max(
                    hit.distance - 0.15f,
                    0.5f
                );
        }

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime
            );

        Quaternion targetRotation =
            Quaternion.LookRotation(
                targetPosition -
                transform.position
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                smoothSpeed * Time.deltaTime
            );
    }
}
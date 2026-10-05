using UnityEngine;
using UnityEngine.InputSystem;

public class movement1 : MonoBehaviour
{
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference interact;

    public float speed = 5f;
    public float jumpForce = 10f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField]
    private Vector3 cameraOffset =
        new Vector3(0f, 0.8f, 0f);

    [SerializeField] private float mouseSensitivity = 2f;

    private Rigidbody rb;

    private Vector2 input;

    private float rotX;
    private float rotY;

    private bool jumpPressed;

    private bool nearCar;
    private bool inCar;

    private Car car;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        move.action.Enable();
        jump.action.Enable();
        interact.action.Enable();

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        if (inCar)
            return;

        input =
            move.action.ReadValue<Vector2>();

        if (jump.action.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        // Войти в машину
        if (interact.action.WasPressedThisFrame())
        {
            if (nearCar && car != null)
            {
                car.EnterCar(gameObject);
            }
        }

        if (Mouse.current != null)
        {
            Vector2 mouse =
                Mouse.current.delta.ReadValue();

            rotY +=
                mouse.x *
                mouseSensitivity *
                0.1f;

            rotX -=
                mouse.y *
                mouseSensitivity *
                0.1f;

            rotX =
                Mathf.Clamp(
                    rotX,
                    -80f,
                    80f
                );
        }
    }

    private void FixedUpdate()
    {
        if (inCar)
            return;

        Vector3 forward =
            Quaternion.Euler(
                0f,
                rotY,
                0f
            ) * Vector3.forward;

        Vector3 right =
            Quaternion.Euler(
                0f,
                rotY,
                0f
            ) * Vector3.right;

        Vector3 direction =
            (
                forward *
                input.y
                +
                right *
                input.x
            ) * speed;

        direction.y =
            rb.linearVelocity.y;

        rb.linearVelocity =
            direction;

        rb.MoveRotation(
            Quaternion.Euler(
                0f,
                rotY,
                0f
            )
        );

        if (jumpPressed)
        {
            rb.AddForce(
                Vector3.up *
                jumpForce,
                ForceMode.Impulse
            );

            jumpPressed = false;
        }
    }

    private void LateUpdate()
    {
        if (inCar)
            return;

        if (cameraTransform == null)
            return;

        cameraTransform.position =
            transform.position +
            cameraOffset;

        cameraTransform.rotation =
            Quaternion.Euler(
                rotX,
                rotY,
                0f
            );
    }

    public void SetInCar(bool value)
    {
        inCar = value;

        input =
            Vector2.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            nearCar = true;

            car =
                other.GetComponentInParent<Car>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            nearCar = false;

            car = null;
        }
    }
}
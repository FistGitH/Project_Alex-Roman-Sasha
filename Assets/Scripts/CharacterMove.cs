using UnityEngine;
using UnityEngine.InputSystem;

public class movement1 : MonoBehaviour
{
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;
   
    public float speed = 5f;
    public float jumpForce = 10f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 0.8f, 0f);
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float clampAngleMin = -80f;
    [SerializeField] private float clampAngleMax = 80f;

    private float rotX = 0f;
    private float rotY = 0f;

    private Rigidbody rb;
    private Vector2 input = Vector2.zero;
    private bool jumpPressed = false;
    private int jumps = 0;
    private Vector3 cameraVelocity = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        move.action.Enable();
        jump.action.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        input = move.action.ReadValue<Vector2>();

        if (jump.action.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        rotY += mouseDelta.x * mouseSensitivity * 0.1f;
        rotX -= mouseDelta.y * mouseSensitivity * 0.1f;
        rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax); ;
    }

    private void FixedUpdate()
    {
        Vector3 forward = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;

        Vector3 moveDir = (forward * input.y + right * input.x) * speed;
        moveDir.y = rb.linearVelocity.y;
        rb.linearVelocity = moveDir;

        rb.MoveRotation(Quaternion.Euler(0f, rotY, 0f));

        if (jumpPressed)
        {
            if (jumps < 2)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumps++;
            }
            jumpPressed = false;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null) return;

        cameraTransform.position = transform.position + cameraOffset;

        cameraTransform.rotation = Quaternion.Euler(rotX, rotY, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            jumps = 0;
        }
    }
}
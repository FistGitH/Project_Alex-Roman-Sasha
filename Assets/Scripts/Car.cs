using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    public Action onbreakrules;

    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference interact;
    private Rigidbody rb;

    [SerializeField] public float speed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    public Vector3 disposition;

    private Vector2 input = Vector2.zero;
    public float rotY = 0f;

    private bool nearplayer;
    private bool incar;

    private GameObject player;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        move.action.Enable();
        interact.action.Enable();
    }
    void Update()
    {
        if (incar)
            player.transform.position = gameObject.transform.position + disposition;
        input = move.action.ReadValue<Vector2>();

        if (interact.action.WasPressedThisFrame())
        {
            if(nearplayer && !incar)
            {
                incar = true;
            }
            else if (nearplayer)
            {
                incar = false;
                nearplayer = false;
                player.transform.position = gameObject.transform.position - disposition;
            }
        }
    }
    private void FixedUpdate()
    {
        if (incar)
        {
            player.transform.position = gameObject.transform.position + disposition;
            Vector3 forward = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;

            Vector3 moveDir = (forward * input.y + right * input.x) * speed;
            moveDir.y = rb.linearVelocity.y;
            rb.linearVelocity = moveDir;

            rb.MoveRotation(Quaternion.Euler(0f, rotY, 0f));
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            nearplayer = true;
            player = collision.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "road")
        {
            breakroadrules();
        }
    }

    public void breakroadrules()
    {
        onbreakrules.Invoke();
    }
}

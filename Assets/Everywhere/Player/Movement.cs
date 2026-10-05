using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Movement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference move;
    [SerializeField] private InputActionReference jump;

    [Header("Player Sounds")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkSounds;
    [SerializeField] private AudioClip[] runSounds;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip tiredSound;

    private float stepTimer;

    [Header("Footsteps")]
    public float walkStepDelay = 1f;
    public float runStepDelay = 1.5f;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2.5f;

    [Header("Stamina")]
    public float maxStamina = 5f;
    public float staminaDrain = 1f;
    public float staminaRecover = 1.5f;
    public float tiredTime = 2f;

    [SerializeField] private UnityEngine.UI.Slider staminaSlider;

    private float stamina;
    private bool exhausted;
    private float exhaustedTimer;

    [Header("Movement Smooth")]
    public float acceleration = 12f;
    public float deceleration = 18f;
    public float airControl = 0.35f;

    [Header("Low Stamina Effects")]
    public float lowStaminaPercent = 0.2f;
    public float tiredRunMultiplier = 0.5f;
    public float staminaUIFadeSpeed = 3f;

    private CanvasGroup staminaCanvas;

    [Header("Jump")]
    public float jumpForce = 7f;
    public int maxJumps = 2;

    [Header("Ladder")]
    [SerializeField] private Transform ladderDirection;
    [SerializeField] private float ladderUpSpeed = 5f;
    [SerializeField] private float ladderDownSpeed = 3f;
    [SerializeField] private float ladderForce = 30f;

    private bool isOnLadder;
    private ConstantForce constantForce;
    private Vector3 ladderNormal;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    public float mouseSensitivity = 2f;
    public float minLook = -80f;
    public float maxLook = 80f;
    public float cameraHeight = 1.7f;

    [Header("FOV")]
    [SerializeField] private Camera playerCamera;

    public float normalFOV = 75f;
    public float runFOV = 85f;
    public float fovSmooth = 8f;

    [Header("Crouch")]
    public float standHeight = 2f;
    public float crouchHeight = 1.1f;
    public float crouchSmooth = 10f;

    [Header("Camera Crouch")]
    public float standCameraHeight = 1.7f;
    public float crouchCameraHeight = 0.9f;
    public float cameraCrouchSmooth = 10f;

    private float currentCameraHeight;

    [Header("Head Bob")]
    public float bobSpeed = 12f;
    public float bobAmount = 0.05f;

    [Header("Jump Camera")]
    public float jumpCameraOffset = -0.12f;
    public float landCameraOffset = 0.15f;
    public float cameraAnimSmooth = 8f;

    private Rigidbody rb;
    private CapsuleCollider capsule;

    private Vector2 input;

    private float rotX;
    private float rotY;

    private bool jumpPressed;
    private bool isRunning;
    private bool isCrouching;

    private int jumps;

    private float currentHeight;
    private float bobTimer;
    private float cameraOffsetY;

    private Vector3 currentVelocity;

    // ==============================
    // CAR
    // ==============================

    private bool inCar;
    private bool nearCar;

    private Car currentCar;

    // ==============================
    // AWAKE
    // ==============================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;

        move.action.Enable();
        jump.action.Enable();

        currentHeight = standHeight;
        currentCameraHeight = standCameraHeight;

        stamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = stamina;

            staminaCanvas =
                staminaSlider.GetComponent<CanvasGroup>();

            if (staminaCanvas == null)
            {
                staminaCanvas =
                    staminaSlider.gameObject.AddComponent<CanvasGroup>();
            }

            staminaCanvas.alpha = 0f;
        }

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFOV;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        constantForce = GetComponent<ConstantForce>();

        if (constantForce != null)
        {
            constantForce.enabled = false;
        }
    }

    // ==============================
    // UPDATE
    // ==============================

    private void Update()
    {
        if (inCar)
            return;

        // E = войти в машину
        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (nearCar && currentCar != null)
            {
                currentCar.EnterCar(gameObject);
                return;
            }
        }

        input = move.action.ReadValue<Vector2>();

        // Прыжок
        if (jump.action.WasPressedThisFrame())
        {
            jumpPressed = true;
        }

        // Мышь
        if (Mouse.current != null)
        {
            Vector2 mouse =
                Mouse.current.delta.ReadValue();

            rotY += mouse.x *
                    mouseSensitivity *
                    0.1f;

            rotX -= mouse.y *
                    mouseSensitivity *
                    0.1f;

            rotX =
                Mathf.Clamp(
                    rotX,
                    minLook,
                    maxLook
                );
        }

        if (Keyboard.current != null)
        {
            // Присед
            isCrouching =
                Keyboard.current.leftCtrlKey.isPressed;

            // Бег
            isRunning =
                Keyboard.current.leftShiftKey.isPressed &&
                input.sqrMagnitude > 0.01f &&
                !isCrouching &&
                !exhausted;
        }

        HandleStamina();
        HandleFootsteps();
    }

    // ==============================
    // FIXED UPDATE
    // ==============================

    private void FixedUpdate()
    {
        if (inCar)
            return;

        float targetSpeed = walkSpeed;

        if (isOnLadder)
        {
            targetSpeed =
                walkSpeed * 0.5f;
        }

        if (isRunning && !isOnLadder)
        {
            targetSpeed = runSpeed;

            float staminaPercent =
                stamina / maxStamina;

            if (staminaPercent <=
                lowStaminaPercent)
            {
                targetSpeed *=
                    tiredRunMultiplier;
            }
        }

        if (isCrouching &&
            !isOnLadder)
        {
            targetSpeed =
                crouchSpeed;
        }

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

        Vector3 targetVelocity =
            (
                forward * input.y +
                right * input.x
            ).normalized *
            targetSpeed;

        float control =
            IsGrounded() || isOnLadder
                ? 1f
                : airControl;

        float smooth =
            targetVelocity.magnitude >
            currentVelocity.magnitude
                ? acceleration
                : deceleration;

        currentVelocity =
            Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                smooth *
                control *
                Time.fixedDeltaTime
            );

        rb.linearVelocity =
            new Vector3(
                currentVelocity.x,
                rb.linearVelocity.y,
                currentVelocity.z
            );

        rb.MoveRotation(
            Quaternion.Euler(
                0f,
                rotY,
                0f
            )
        );

        // LADDER
        if (isOnLadder &&
            ladderDirection != null)
        {
            float speed =
                input.y > 0f
                    ? ladderUpSpeed
                    : ladderDownSpeed;

            Vector3 climbVelocity =
                ladderDirection.forward *
                (input.y * speed);

            rb.linearVelocity =
                climbVelocity;
        }

        // JUMP
        if (jumpPressed &&
            !isOnLadder)
        {
            if (jumps < maxJumps)
            {
                rb.linearVelocity =
                    new Vector3(
                        rb.linearVelocity.x,
                        0f,
                        rb.linearVelocity.z
                    );

                rb.AddForce(
                    Vector3.up * jumpForce,
                    ForceMode.Impulse
                );

                if (jumpSound != null &&
                    audioSource != null)
                {
                    audioSource.PlayOneShot(
                        jumpSound
                    );
                }

                jumps++;

                cameraOffsetY =
                    jumpCameraOffset;
            }

            jumpPressed = false;
        }

        // LADDER GRAVITY
        if (isOnLadder)
        {
            rb.useGravity = false;

            rb.AddForce(
                -ladderNormal *
                ladderForce,
                ForceMode.Force
            );
        }
        else
        {
            rb.useGravity = true;
        }
    }

    // ==============================
    // CAMERA
    // ==============================

    private void LateUpdate()
    {
        if (inCar)
            return;

        if (cameraTransform == null)
            return;

        HandleCrouch();
        HandleHeadBob();
        HandleCameraAnimation();
        HandleFOV();

        Vector3 cameraPosition =
            transform.position +
            Vector3.up *
            currentCameraHeight;

        cameraPosition.y +=
            cameraOffsetY;

        cameraTransform.position =
            cameraPosition;

        cameraTransform.rotation =
            Quaternion.Euler(
                rotX,
                rotY,
                0f
            );
    }

    // ==============================
    // CAR
    // ==============================

    public void SetInCar(bool value)
    {
        inCar = value;

        input = Vector2.zero;
        currentVelocity = Vector3.zero;

        jumpPressed = false;
        isRunning = false;
        isCrouching = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        // Выключаем обычную камеру игрока
        if (playerCamera != null)
        {
            playerCamera.gameObject
                .SetActive(!value);
        }
        else if (cameraTransform != null)
        {
            cameraTransform.gameObject
                .SetActive(!value);
        }
    }

    // ==============================
    // FOV
    // ==============================

    private void HandleFOV()
    {
        if (playerCamera == null)
            return;

        float targetFOV =
            isRunning
                ? runFOV
                : normalFOV;

        playerCamera.fieldOfView =
            Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                Time.deltaTime *
                fovSmooth
            );
    }

    // ==============================
    // HEAD BOB
    // ==============================

    private void HandleHeadBob()
    {
        Vector3 horizontalVelocity =
            rb.linearVelocity;

        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude >
            0.2f)
        {
            float speedMultiplier = 1f;

            if (isRunning)
            {
                speedMultiplier = 1.5f;

                float staminaPercent =
                    stamina / maxStamina;

                if (staminaPercent <=
                    lowStaminaPercent)
                {
                    speedMultiplier =
                        2.5f;
                }
            }

            bobTimer +=
                Time.deltaTime *
                bobSpeed *
                speedMultiplier;

            float bob =
                Mathf.Sin(bobTimer) *
                bobAmount;

            cameraTransform.localPosition =
                new Vector3(
                    0f,
                    bob,
                    0f
                );
        }
        else
        {
            cameraTransform.localPosition =
                Vector3.Lerp(
                    cameraTransform.localPosition,
                    Vector3.zero,
                    Time.deltaTime *
                    8f
                );

            bobTimer = 0f;
        }
    }

    // ==============================
    // CAMERA ANIMATION
    // ==============================

    private void HandleCameraAnimation()
    {
        cameraOffsetY =
            Mathf.Lerp(
                cameraOffsetY,
                0f,
                Time.deltaTime *
                cameraAnimSmooth
            );
    }

    // ==============================
    // CROUCH
    // ==============================

    private void HandleCrouch()
    {
        bool canStand =
            CheckCanStand();

        float targetHeight;

        if (isCrouching ||
            !canStand)
        {
            targetHeight =
                crouchHeight;
        }
        else
        {
            targetHeight =
                standHeight;
        }

        currentHeight =
            Mathf.Lerp(
                currentHeight,
                targetHeight,
                Time.deltaTime *
                crouchSmooth
            );

        capsule.height =
            currentHeight;

        capsule.center =
            new Vector3(
                0f,
                currentHeight / 2f,
                0f
            );

        float targetCameraHeight =
            isCrouching
                ? crouchCameraHeight
                : standCameraHeight;

        currentCameraHeight =
            Mathf.Lerp(
                currentCameraHeight,
                targetCameraHeight,
                Time.deltaTime *
                cameraCrouchSmooth
            );
    }

    private bool CheckCanStand()
    {
        float radius =
            capsule.radius * 0.9f;

        float distance =
            standHeight -
            radius * 2f;

        Vector3 start =
            transform.position +
            Vector3.up *
            radius;

        return !Physics.SphereCast(
            start,
            radius,
            Vector3.up,
            out RaycastHit hit,
            distance
        );
    }

    // ==============================
    // GROUND
    // ==============================

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            currentHeight / 2f +
            0.15f
        );
    }

    // ==============================
    // STAMINA
    // ==============================

    private void HandleStamina()
    {
        bool runningNow =
            isRunning &&
            input.sqrMagnitude >
            0.01f;

        if (runningNow)
        {
            stamina -=
                staminaDrain *
                Time.deltaTime;

            if (staminaSlider != null)
            {
                ShowStaminaUI();
            }

            if (stamina <= 0f)
            {
                stamina = 0f;

                exhausted = true;
                exhaustedTimer =
                    tiredTime;

                if (tiredSound != null &&
                    audioSource != null)
                {
                    audioSource.PlayOneShot(
                        tiredSound
                    );
                }

                if (staminaSlider != null)
                {
                    staminaSlider
                        .fillRect
                        .GetComponent<
                            UnityEngine.UI.Image>()
                        .color =
                        Color.red;
                }
            }
        }
        else
        {
            if (!exhausted)
            {
                stamina +=
                    staminaRecover *
                    Time.deltaTime;
            }
        }

        if (exhausted)
        {
            exhaustedTimer -=
                Time.deltaTime;

            if (exhaustedTimer <= 0f)
            {
                exhausted = false;

                if (staminaSlider != null)
                {
                    staminaSlider
                        .fillRect
                        .GetComponent<
                            UnityEngine.UI.Image>()
                        .color =
                        Color.white;
                }
            }
        }

        stamina =
            Mathf.Clamp(
                stamina,
                0f,
                maxStamina
            );

        if (staminaSlider != null)
        {
            staminaSlider.value =
                stamina;
        }

        if (stamina >= maxStamina)
        {
            HideStaminaUI();
        }
        else
        {
            ShowStaminaUI();
        }
    }

    private void ShowStaminaUI()
    {
        if (staminaCanvas == null)
            return;

        staminaSlider.gameObject
            .SetActive(true);

        staminaCanvas.alpha =
            Mathf.Lerp(
                staminaCanvas.alpha,
                1f,
                Time.deltaTime *
                staminaUIFadeSpeed
            );
    }

    private void HideStaminaUI()
    {
        if (staminaCanvas == null)
            return;

        staminaCanvas.alpha =
            Mathf.Lerp(
                staminaCanvas.alpha,
                0f,
                Time.deltaTime *
                staminaUIFadeSpeed
            );

        if (staminaCanvas.alpha <=
            0.01f)
        {
            staminaSlider.gameObject
                .SetActive(false);
        }
    }

    // ==============================
    // FOOTSTEPS
    // ==============================

    private void HandleFootsteps()
    {
        Vector3 horizontalVelocity =
            rb.linearVelocity;

        horizontalVelocity.y = 0f;

        bool moving =
            horizontalVelocity.magnitude >
            0.5f &&
            IsGrounded();

        if (!moving)
            return;

        stepTimer -= Time.deltaTime;

        float delay =
            isRunning
                ? runStepDelay
                : walkStepDelay;

        if (stepTimer <= 0f)
        {
            PlayStepSound();

            stepTimer =
                delay +
                Random.Range(
                    -0.15f,
                    0.15f
                );
        }
    }

    private void PlayStepSound()
    {
        if (audioSource == null)
            return;

        AudioClip[] clips =
            isRunning
                ? runSounds
                : walkSounds;

        if (clips == null ||
            clips.Length == 0)
            return;

        int random =
            Random.Range(
                0,
                clips.Length
            );

        audioSource.PlayOneShot(
            clips[random]
        );
    }

    // ==============================
    // COLLISIONS
    // ==============================

    private void OnCollisionEnter(
        Collision collision)
    {
        if (collision.collider
            .CompareTag("Ground"))
        {
            jumps = 0;

            cameraOffsetY =
                landCameraOffset;
        }
    }

    private void OnCollisionStay(
        Collision collision)
    {
        if (collision.collider
            .CompareTag("Ground"))
        {
            jumps = 0;
        }
    }

    // ==============================
    // TRIGGERS
    // ==============================

    private void OnTriggerEnter(
        Collider other)
    {
        // LADDER
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;

            ladderNormal =
                -other.transform.forward;

            if (constantForce != null)
            {
                constantForce.enabled =
                    true;
            }
        }

        // CAR
        if (other.CompareTag("Car"))
        {
            Car foundCar =
                other.GetComponentInParent<Car>();

            if (foundCar != null)
            {
                nearCar = true;
                currentCar = foundCar;
            }
        }
    }

    private void OnTriggerExit(
        Collider other)
    {
        // LADDER
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;

            if (constantForce != null)
            {
                constantForce.enabled =
                    false;
            }
        }

        // CAR
        if (other.CompareTag("Car"))
        {
            Car foundCar =
                other.GetComponentInParent<Car>();

            if (foundCar ==
                currentCar)
            {
                nearCar = false;
                currentCar = null;
            }
        }
    }
}
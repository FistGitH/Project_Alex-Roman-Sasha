using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class SubmarineController : MonoBehaviour
{
    [Header("”правление")]
    [SerializeField] private bool controlsEnabled = true;

    [Tooltip("ƒочерний объект: син€€ ось Z смотрит вперЄд из кабины.")]
    [SerializeField] private Transform movementReference;

    [Header("ƒвижение")]
    [SerializeField, Min(0.1f)] private float forwardSpeed = 12f;
    [SerializeField, Min(0.1f)] private float reverseSpeed = 7f;
    [SerializeField, Min(0.1f)] private float sidewaysSpeed = 7f;
    [SerializeField, Min(0.1f)] private float acceleration = 9f;
    [SerializeField, Min(0.1f)] private float braking = 5f;

    [Header("ѕовороты")]
    [SerializeField, Min(0.1f)] private float yawSpeed = 65f;
    [SerializeField, Min(0.1f)] private float turnAcceleration = 120f;
    [SerializeField, Min(0.1f)] private float pitchSpeed = 35f;
    [SerializeField, Range(0f, 80f)] private float maxPitch = 55f;

    [Header("ћышь")]
    [SerializeField, Range(0f, 0.5f)] private float mouseDeadZone = 0.08f;
    [SerializeField] private bool invertMouseY;

    [Header(" амера")]
    [SerializeField] private Transform cockpitCamera;
    [SerializeField, Min(0.1f)] private float cameraSmoothness = 6f;
    [SerializeField, Min(0f)] private float cameraOffset = 0.035f;
    [SerializeField, Min(0f)] private float cameraTilt = 1.5f;
    [SerializeField, Min(0f)] private float bobAmplitude = 0.008f;
    [SerializeField, Min(0f)] private float bobFrequency = 1.1f;

    [Header("‘онари")]
    [SerializeField] private Light[] headlights;
    [SerializeField] private bool lightsOnAtStart = true;

    [Header("”рон от акулы")]
    [SerializeField, Min(1)] private int hitsToDie = 4;
    private int sharkHits;
    public bool IsDead { get; private set; }
    public int HitsRemaining => Mathf.Max(0, hitsToDie - sharkHits);

    public void TakeSharkHit()
    {
        if (IsDead || !isActiveAndEnabled) return;
        sharkHits++;
        Debug.Log($"јкула протаранила лодку: {sharkHits}/{hitsToDie}", this);
        if (sharkHits < hitsToDie) return;

        IsDead = true;
        SetControlsEnabled(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("SeaDiving_Scene", LoadSceneMode.Single);
    }

    private Rigidbody body;
    private Quaternion referenceOffset;

    private Vector2 moveInput;
    private float turnInput;

    private float yaw;
    private float pitch;
    private float targetPitch;
    private float turnVelocity;

    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;
    private bool cameraInitialized;

    private float bobPhase;
    private bool lightsOn;

    private float MaxSpeed =>
        Mathf.Max(forwardSpeed, reverseSpeed, sidewaysSpeed);

    private Vector3 Velocity
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return body.linearVelocity;
#else
            return body.velocity;
#endif
        }
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        if (movementReference == null ||
            movementReference == transform ||
            !movementReference.IsChildOf(transform))
        {
            Debug.LogError(
                "Ќазначь Movement Reference: дочерний объект Micro_Sub " +
                "с синей осью Z в сторону взгл€да из кабины.",
                this
            );

            enabled = false;
            return;
        }

        referenceOffset =
            Quaternion.Inverse(body.rotation) * movementReference.rotation;

        body.useGravity = false;
        body.isKinematic = false;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        //  онтроллер управл€ет поворотом корпуса.
        body.constraints = RigidbodyConstraints.FreezeRotation;

#if UNITY_6000_0_OR_NEWER
        body.linearDamping = 0f;
#else
        body.drag = 0f;
#endif

        if (cockpitCamera != null)
        {
            cameraStartPosition = cockpitCamera.localPosition;
            cameraStartRotation = cockpitCamera.localRotation;
            cameraInitialized = true;
        }

        SyncRotation();
        SetLights(lightsOnAtStart);
    }

    private void Update()
    {
        moveInput = Vector2.zero;
        turnInput = 0f;

        if (!controlsEnabled || !Application.isFocused ||
            Time.timeScale == 0f)
        {
            targetPitch = pitch;
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null)
        {
            // D Ч вправо, A Ч влево.
            float sideways =
                (keyboard.dKey.isPressed ? 1f : 0f) -
                (keyboard.aKey.isPressed ? 1f : 0f);

            // W Ч вперЄд, S Ч назад.
            float forward =
                (keyboard.wKey.isPressed ? 1f : 0f) -
                (keyboard.sKey.isPressed ? 1f : 0f);

            moveInput = Vector2.ClampMagnitude(
                new Vector2(sideways, forward), 1f
            );

            // E Ч поворот вправо, Q Ч влево.
            turnInput =
                (keyboard.eKey.isPressed ? 1f : 0f) -
                (keyboard.qKey.isPressed ? 1f : 0f);

            if (keyboard.lKey.wasPressedThisFrame)
                SetLights(!lightsOn);
        }

        Mouse mouse = Mouse.current;

        if (mouse != null && Screen.height > 0)
        {
            float vertical = Mathf.Clamp(
                mouse.position.ReadValue().y /
                Screen.height * 2f - 1f,
                -1f,
                1f
            );

            float amount = Mathf.InverseLerp(
                mouseDeadZone, 1f, Mathf.Abs(vertical)
            );

            vertical = Mathf.Sign(vertical) * amount;

            if (invertMouseY)
                vertical = -vertical;

            targetPitch = -vertical * maxPitch;
        }
        else
        {
            targetPitch = pitch;
        }
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled)
            return;

        float dt = Time.fixedDeltaTime;

        turnVelocity = Mathf.MoveTowards(
            turnVelocity,
            turnInput * yawSpeed,
            turnAcceleration * dt
        );

        yaw = Mathf.Repeat(yaw + turnVelocity * dt, 360f);

        pitch = Mathf.MoveTowards(
            pitch,
            targetPitch,
            pitchSpeed * dt
        );

        Quaternion heading = Quaternion.Euler(pitch, yaw, 0f);

        body.MoveRotation(
            heading * Quaternion.Inverse(referenceOffset)
        );

        float longitudinalSpeed =
            moveInput.y >= 0f ? forwardSpeed : reverseSpeed;

        Vector3 targetVelocity = heading * new Vector3(
            moveInput.x * sidewaysSpeed,
            0f,
            moveInput.y * longitudinalSpeed
        );

        Vector3 velocity = Velocity;

        float response =
            moveInput.sqrMagnitude > 0.001f
                ? acceleration
                : braking;

        Vector3 nextVelocity = Vector3.MoveTowards(
            velocity,
            targetVelocity,
            response * dt
        );

        body.AddForce(
            nextVelocity - velocity,
            ForceMode.VelocityChange
        );
    }

    private void LateUpdate()
    {
        if (!controlsEnabled || !cameraInitialized ||
            cockpitCamera == null)
        {
            return;
        }

        Quaternion heading = body.rotation * referenceOffset;

        Vector3 localVelocity =
            Quaternion.Inverse(heading) * Velocity;

        float sideways = Mathf.Clamp(
            localVelocity.x / sidewaysSpeed,
            -1f,
            1f
        );

        float forward = Mathf.Clamp(
            localVelocity.z /
            (localVelocity.z >= 0f ? forwardSpeed : reverseSpeed),
            -1f,
            1f
        );

        float speedAmount = Mathf.Clamp01(
            Velocity.magnitude / MaxSpeed
        );

        bobPhase +=
            Time.deltaTime * bobFrequency * Mathf.PI * 2f;

        Vector3 bob = new Vector3(
            Mathf.Sin(bobPhase * 0.5f) * 0.5f,
            Mathf.Sin(bobPhase),
            0f
        ) * (bobAmplitude * speedAmount);

        Vector3 offset = cameraStartRotation *
            new Vector3(-sideways, 0f, -forward) * cameraOffset;

        Vector3 targetPosition =
            cameraStartPosition +
            offset +
            cameraStartRotation * bob;

        float turnAmount = Mathf.Clamp(
            turnVelocity / yawSpeed,
            -1f,
            1f
        );

        Quaternion targetRotation =
            cameraStartRotation * Quaternion.Euler(
                forward * cameraTilt,
                0f,
                -(sideways * 0.5f + turnAmount) * cameraTilt
            );

        float blend =
            1f - Mathf.Exp(-cameraSmoothness * Time.deltaTime);

        cockpitCamera.localPosition = Vector3.Lerp(
            cockpitCamera.localPosition,
            targetPosition,
            blend
        );

        cockpitCamera.localRotation = Quaternion.Slerp(
            cockpitCamera.localRotation,
            targetRotation,
            blend
        );
    }

    public void SetControlsEnabled(bool value)
    {
        if (body == null || !enabled)
            return;

        moveInput = Vector2.zero;
        turnInput = 0f;
        turnVelocity = 0f;

        if (value)
            SyncRotation();
        else
            ResetCamera();

        controlsEnabled = value;
    }

    private void SyncRotation()
    {
        Quaternion heading = body.rotation * referenceOffset;

        yaw = heading.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, heading.eulerAngles.x);
        targetPitch = pitch;
    }

    private void SetLights(bool value)
    {
        lightsOn = value;

        if (headlights == null)
            return;

        foreach (Light headlight in headlights)
        {
            if (headlight != null)
                headlight.enabled = lightsOn;
        }
    }

    private void ResetCamera()
    {
        if (!cameraInitialized || cockpitCamera == null)
            return;

        cockpitCamera.localPosition = cameraStartPosition;
        cockpitCamera.localRotation = cameraStartRotation;
    }

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        turnInput = 0f;
        turnVelocity = 0f;

        ResetCamera();
    }
}

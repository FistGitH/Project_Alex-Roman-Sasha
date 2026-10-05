using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference move;

    [Header("Movement")]
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float reverseSpeed = 8f;

    [SerializeField] private float turnSpeed = 80f;
    [SerializeField] private float brakeForce = 15f;

    [Header("Grip")]
    [SerializeField] private float sideGrip = 6f;

    [Header("Camera")]
    [SerializeField] private GameObject carCamera;

    [Header("Audio")]
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource engineSound;

    [SerializeField] private float minEnginePitch = 0.8f;
    [SerializeField] private float maxEnginePitch = 1.6f;

    // Используется TrafficLight и carspawn
    public Action onbreakrules;

    private Rigidbody rb;

    private Vector2 input;

    private bool inCar = false;
    private bool waitForERelease = false;

    private GameObject player;
    private Movement playerMovement;
    private Rigidbody playerRb;

    private Renderer[] playerRenderers;
    private Collider[] playerColliders;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (move != null)
        {
            move.action.Enable();
        }

        if (carCamera != null)
        {
            carCamera.SetActive(false);
        }

        if (music != null)
        {
            music.Stop();
        }

        if (engineSound != null)
        {
            engineSound.loop = true;
            engineSound.Stop();
        }
    }

    private void Update()
    {
        if (!inCar)
            return;

        if (move != null)
        {
            input = move.action.ReadValue<Vector2>();
        }

        if (Keyboard.current == null)
            return;

        // После входа ждём отпускания E,
        // чтобы сразу же не выйти
        if (waitForERelease)
        {
            if (!Keyboard.current.eKey.isPressed)
            {
                waitForERelease = false;
            }

            UpdateEngineSound();
            return;
        }

        // E = выйти из машины
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ExitCar();
            return;
        }

        UpdateEngineSound();
    }

    private void FixedUpdate()
    {
        if (!inCar)
            return;

        Drive();
        Turn();
        ApplyGrip();
        LimitSpeed();
    }

    // =====================================
    // ENTER CAR
    // =====================================

    public void EnterCar(GameObject newPlayer)
    {
        if (inCar)
            return;

        player = newPlayer;

        playerMovement =
            player.GetComponent<Movement>();

        playerRb =
            player.GetComponent<Rigidbody>();

        playerRenderers =
            player.GetComponentsInChildren<Renderer>(true);

        playerColliders =
            player.GetComponentsInChildren<Collider>(true);

        inCar = true;
        waitForERelease = true;

        input = Vector2.zero;

        if (playerMovement != null)
        {
            playerMovement.SetInCar(true);
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity =
                Vector3.zero;

            playerRb.angularVelocity =
                Vector3.zero;

            playerRb.isKinematic = true;
        }

        // Скрываем игрока
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null)
                {
                    r.enabled = false;
                }
            }
        }

        // Отключаем Collider игрока
        if (playerColliders != null)
        {
            foreach (Collider c in playerColliders)
            {
                if (c != null)
                {
                    c.enabled = false;
                }
            }
        }

        // Ставим игрока внутрь машины
        player.transform.position =
            transform.position;

        player.transform.rotation =
            transform.rotation;

        // Камера машины
        if (carCamera != null)
        {
            carCamera.SetActive(true);
        }

        // Музыка
        if (music != null)
        {
            if (!music.isPlaying)
            {
                music.Play();
            }
        }

        // Двигатель
        if (engineSound != null)
        {
            if (!engineSound.isPlaying)
            {
                engineSound.Play();
            }
        }
    }

    // =====================================
    // EXIT CAR
    // =====================================

    public void ExitCar()
    {
        if (!inCar)
            return;

        inCar = false;

        input = Vector2.zero;

        if (player == null)
            return;

        // Игрок появляется рядом с машиной
        player.transform.position =
            transform.position +
            transform.right * 2f +
            Vector3.up * 0.2f;

        player.transform.rotation =
            Quaternion.Euler(
                0f,
                transform.eulerAngles.y,
                0f
            );

        // Возвращаем модель игрока
        if (playerRenderers != null)
        {
            foreach (Renderer r in playerRenderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                }
            }
        }

        // Возвращаем Collider
        if (playerColliders != null)
        {
            foreach (Collider c in playerColliders)
            {
                if (c != null)
                {
                    c.enabled = true;
                }
            }
        }

        if (playerRb != null)
        {
            playerRb.isKinematic = false;

            playerRb.linearVelocity =
                Vector3.zero;

            playerRb.angularVelocity =
                Vector3.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.SetInCar(false);
        }

        if (carCamera != null)
        {
            carCamera.SetActive(false);
        }

        if (music != null)
        {
            music.Stop();
        }

        if (engineSound != null)
        {
            engineSound.Stop();
        }

        player = null;
        playerMovement = null;
        playerRb = null;

        playerRenderers = null;
        playerColliders = null;
    }

    // =====================================
    // DRIVE
    // =====================================

    private void Drive()
    {
        /*
         * ВАЖНО:
         *
         * Здесь стоит минус:
         *
         * -input.y
         *
         * потому что твоя модель машины
         * направлена противоположно
         * стандартному Unity forward.
         */

        float gas = -input.y;

        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        // Поскольку модель перевёрнута,
        // направление скорости тоже учитываем наоборот
        float forwardSpeed =
            -localVelocity.z;

        // W
        if (gas < 0f)
        {
            if (forwardSpeed < maxSpeed)
            {
                rb.AddForce(
                    -transform.forward *
                    Mathf.Abs(gas) *
                    acceleration,
                    ForceMode.Acceleration
                );
            }
        }

        // S
        if (gas > 0f)
        {
            // Сначала тормозим
            if (forwardSpeed > 1f)
            {
                rb.AddForce(
                    transform.forward *
                    brakeForce,
                    ForceMode.Acceleration
                );
            }
            else
            {
                // Затем задний ход
                if (forwardSpeed > -reverseSpeed)
                {
                    rb.AddForce(
                        transform.forward *
                        gas *
                        acceleration,
                        ForceMode.Acceleration
                    );
                }
            }
        }

        // Если W/S не нажаты —
        // машина постепенно останавливается
        if (Mathf.Abs(input.y) < 0.01f)
        {
            Vector3 horizontalVelocity =
                new Vector3(
                    rb.linearVelocity.x,
                    0f,
                    rb.linearVelocity.z
                );

            horizontalVelocity =
                Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    4f *
                    Time.fixedDeltaTime
                );

            rb.linearVelocity =
                new Vector3(
                    horizontalVelocity.x,
                    rb.linearVelocity.y,
                    horizontalVelocity.z
                );
        }
    }

    // =====================================
    // TURN
    // =====================================

    private void Turn()
    {
        /*
         * Тоже инвертируем X,
         * поэтому:
         *
         * A = налево
         * D = направо
         */

        float steering = -input.x;

        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        float forwardSpeed =
            -localVelocity.z;

        float speed =
            Mathf.Abs(forwardSpeed);

        // Не поворачиваем машину,
        // если она полностью стоит
        if (speed < 0.3f)
            return;

        // При движении назад
        // руль работает наоборот
        float reverseDirection =
            forwardSpeed >= 0f
                ? 1f
                : -1f;

        float speedPercent =
            Mathf.Clamp01(
                speed / maxSpeed
            );

        // На большой скорости руль
        // становится немного менее резким
        float steeringMultiplier =
            Mathf.Lerp(
                1f,
                0.5f,
                speedPercent
            );

        float rotation =
            steering *
            turnSpeed *
            steeringMultiplier *
            reverseDirection *
            Time.fixedDeltaTime;

        rb.MoveRotation(
            rb.rotation *
            Quaternion.Euler(
                0f,
                rotation,
                0f
            )
        );
    }

    // =====================================
    // GRIP
    // =====================================

    private void ApplyGrip()
    {
        Vector3 localVelocity =
            transform.InverseTransformDirection(
                rb.linearVelocity
            );

        localVelocity.x =
            Mathf.Lerp(
                localVelocity.x,
                0f,
                sideGrip *
                Time.fixedDeltaTime
            );

        Vector3 newVelocity =
            transform.TransformDirection(
                localVelocity
            );

        newVelocity.y =
            rb.linearVelocity.y;

        rb.linearVelocity =
            newVelocity;
    }

    // =====================================
    // SPEED LIMIT
    // =====================================

    private void LimitSpeed()
    {
        Vector3 horizontalVelocity =
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

        float speed =
            horizontalVelocity.magnitude;

        // Определяем,
        // едем вперёд или назад
        Vector3 carForward =
            -transform.forward;

        float direction =
            Vector3.Dot(
                horizontalVelocity,
                carForward
            );

        float limit =
            direction >= 0f
                ? maxSpeed
                : reverseSpeed;

        if (speed > limit)
        {
            horizontalVelocity =
                horizontalVelocity.normalized *
                limit;

            rb.linearVelocity =
                new Vector3(
                    horizontalVelocity.x,
                    rb.linearVelocity.y,
                    horizontalVelocity.z
                );
        }
    }

    // =====================================
    // ENGINE SOUND
    // =====================================

    private void UpdateEngineSound()
    {
        if (engineSound == null)
            return;

        Vector3 velocity =
            rb.linearVelocity;

        velocity.y = 0f;

        float speed =
            velocity.magnitude;

        float percent =
            Mathf.Clamp01(
                speed / maxSpeed
            );

        engineSound.pitch =
            Mathf.Lerp(
                minEnginePitch,
                maxEnginePitch,
                percent
            );
    }

    // =====================================
    // TRAFFIC RULES
    // =====================================

    public void breakroadrules()
    {
        onbreakrules?.Invoke();
    }
}
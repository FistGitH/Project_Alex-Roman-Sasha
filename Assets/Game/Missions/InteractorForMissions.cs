using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractorForMissions : MonoBehaviour
{
    public enum InteractionType
    {
        Collect,
        Interact,
        Reach,
        Kill,
        Custom
    }

    public enum CompletionMode
    {
        HoldEAndStayNear,
        PressEAndWait
    }

    [Header("Mission")]
    [SerializeField] private MissionData mission;

    [Header("Interaction")]
    [SerializeField]
    private InteractionType interactionType = InteractionType.Interact;

    [SerializeField]
    private CompletionMode completionMode =
        CompletionMode.HoldEAndStayNear;

    [Header("Progress")]
    [SerializeField]
    private int progressAmount = 1;

    [Header("Completion Time")]
    [SerializeField]
    private float completionTime = 2f;

    // =========================================================
    // INTERACTION UI
    // =========================================================

    [Header("Interaction UI")]

    [SerializeField]
    private GameObject interactionPanel;

    [SerializeField]
    private TMP_Text interactionText;

    [SerializeField]
    private CanvasGroup interactionCanvasGroup;

    [SerializeField]
    private string interactionMessage = "Press E to interact";

    [SerializeField]
    private float fadeDuration = 0.3f;

    // =========================================================
    // PROGRESS UI
    // =========================================================

    [Header("Progress UI")]

    [SerializeField]
    private GameObject progressPanel;

    [SerializeField]
    private Slider progressSlider;

    // =========================================================
    // AFTER COMPLETION
    // =========================================================

    [Header("After Completion")]

    [SerializeField]
    private float hideUIDelay = 0.5f;

    // =========================================================
    // INTERNAL
    // =========================================================

    private readonly HashSet<Collider> playerColliders =
        new HashSet<Collider>();

    private bool interacting;
    private bool interacted;

    private Coroutine interactionCoroutine;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        Debug.Log(
            "[Interactor] Awake: " +
            gameObject.name
        );

        CheckReferences();
    }

    private void OnEnable()
    {
        Debug.Log(
            "[Interactor] OnEnable: " +
            gameObject.name
        );

        /*
         * MissionManager может ещё не существовать
         * в момент OnEnable.
         *
         * Поэтому подписываемся здесь,
         * а также в Start().
         */
        SubscribeToMissionManager();

        interacting = false;
        interacted = false;

        playerColliders.Clear();

        if (interactionCanvasGroup != null)
        {
            interactionCanvasGroup.alpha = 0f;
        }

        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }

        if (progressPanel != null)
        {
            progressPanel.SetActive(false);
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    private void Start()
    {
        /*
         * MissionManager уже должен существовать
         * к этому моменту.
         */
        SubscribeToMissionManager();
    }

    private void OnDisable()
    {
        UnsubscribeFromMissionManager();

        StopAllCoroutines();

        interactionCoroutine = null;
        fadeCoroutine = null;

        interacting = false;

        playerColliders.Clear();
    }

    // =========================================================
    // MISSION MANAGER EVENTS
    // =========================================================

    private void SubscribeToMissionManager()
    {
        if (MissionManager.Instance == null)
            return;

        MissionManager.Instance.OnMissionCompleted -=
            OnMissionCompleted;

        MissionManager.Instance.OnMissionCompleted +=
            OnMissionCompleted;
    }

    private void UnsubscribeFromMissionManager()
    {
        if (MissionManager.Instance == null)
            return;

        MissionManager.Instance.OnMissionCompleted -=
            OnMissionCompleted;
    }

    // =========================================================
    // CHECK REFERENCES
    // =========================================================

    private void CheckReferences()
    {
        Debug.Log(
            "========== INTERACTOR CHECK =========="
        );

        if (mission != null)
        {
            Debug.Log(
                "[Interactor] Mission: " +
                mission.title +
                " | ID: " +
                mission.missionID +
                " | Target: " +
                mission.targetID
            );
        }
        else
        {
            Debug.LogError(
                "[Interactor] ERROR: Mission is NULL!"
            );
        }

        if (interactionPanel != null)
            Debug.Log(
                "[Interactor] InteractionPanel OK"
            );
        else
            Debug.LogError(
                "[Interactor] ERROR: InteractionPanel NULL"
            );

        if (interactionText != null)
            Debug.Log(
                "[Interactor] InteractionText OK"
            );
        else
            Debug.LogError(
                "[Interactor] ERROR: InteractionText NULL"
            );

        if (interactionCanvasGroup != null)
            Debug.Log(
                "[Interactor] CanvasGroup OK"
            );
        else
            Debug.LogError(
                "[Interactor] ERROR: CanvasGroup NULL"
            );

        if (progressPanel != null)
            Debug.Log(
                "[Interactor] ProgressPanel OK"
            );
        else
            Debug.LogError(
                "[Interactor] ERROR: ProgressPanel NULL"
            );

        if (progressSlider != null)
            Debug.Log(
                "[Interactor] ProgressSlider OK"
            );
        else
            Debug.LogError(
                "[Interactor] ERROR: ProgressSlider NULL"
            );

        Debug.Log(
            "======================================"
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (MissionManager.Instance == null)
            return;

        if (mission == null)
            return;

        if (interacted)
            return;

        /*
         * Проверяем реальное количество
         * Collider'ов игрока внутри Trigger.
         */
        bool playerInRange =
            playerColliders.Count > 0;

        if (!playerInRange)
            return;

        MissionData currentMission =
            MissionManager.Instance.GetCurrentMission();

        if (currentMission == null)
            return;

        /*
         * Это НЕ текущая миссия.
         */
        if (currentMission != mission)
        {
            HideInteractionUI();
            return;
        }

        /*
         * Если взаимодействие уже идёт,
         * ничего нового не запускаем.
         */
        if (interacting)
        {
            /*
             * Для Hold E проверяем,
             * что E всё ещё зажата.
             */
            if (completionMode ==
                CompletionMode.HoldEAndStayNear)
            {
                if (Keyboard.current == null)
                    return;

                if (!Keyboard.current.eKey.isPressed)
                {
                    Debug.Log(
                        "[Interactor] E released"
                    );

                    StopInteraction();
                }
            }

            return;
        }

        /*
         * Игрок находится рядом
         * с объектом текущей миссии.
         */
        ShowInteractionUI();

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log(
                "[Interactor] E PRESSED! Mission: " +
                mission.title
            );

            StartInteraction();
        }
    }

    // =========================================================
    // START INTERACTION
    // =========================================================

    private void StartInteraction()
    {
        if (interacting)
            return;

        interacting = true;

        Debug.Log(
            "[Interactor] Starting interaction. Mode: " +
            completionMode
        );

        /*
         * Показываем Slider.
         */
        ShowProgressUI();

        /*
         * Убираем подсказку.
         */
        HideInteractionUI();

        if (interactionCoroutine != null)
        {
            StopCoroutine(
                interactionCoroutine
            );
        }

        if (completionMode ==
            CompletionMode.HoldEAndStayNear)
        {
            interactionCoroutine =
                StartCoroutine(
                    HoldInteraction()
                );
        }
        else
        {
            interactionCoroutine =
                StartCoroutine(
                    PressInteraction()
                );
        }
    }

    // =========================================================
    // HOLD E
    // =========================================================

    private IEnumerator HoldInteraction()
    {
        float timer = 0f;

        Debug.Log(
            "[Interactor] HOLD interaction started"
        );

        while (timer < completionTime)
        {
            /*
             * Проверяем, находится ли
             * хотя бы один Collider игрока
             * внутри Trigger.
             */
            if (playerColliders.Count == 0)
            {
                Debug.Log(
                    "[Interactor] Player left trigger!"
                );

                StopInteraction();

                yield break;
            }

            /*
             * E должна быть зажата.
             */
            if (Keyboard.current == null ||
                !Keyboard.current.eKey.isPressed)
            {
                Debug.Log(
                    "[Interactor] E released!"
                );

                StopInteraction();

                yield break;
            }

            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / completionTime
                );

            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        Debug.Log(
            "[Interactor] Interaction reached 100%"
        );

        FinishInteraction();
    }

    // =========================================================
    // PRESS E
    // =========================================================

    private IEnumerator PressInteraction()
    {
        float timer = 0f;

        Debug.Log(
            "[Interactor] PRESS interaction started"
        );

        while (timer < completionTime)
        {
            if (playerColliders.Count == 0)
            {
                StopInteraction();

                yield break;
            }

            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / completionTime
                );

            SetProgress(progress);

            yield return null;
        }

        SetProgress(1f);

        FinishInteraction();
    }

    // =========================================================
    // FINISH
    // =========================================================

    private void FinishInteraction()
    {
        Debug.Log(
            "[Interactor] FINISH INTERACTION"
        );

        interacting = false;
        interactionCoroutine = null;

        if (MissionManager.Instance == null)
        {
            Debug.LogError(
                "[Interactor] MissionManager.Instance NULL!"
            );

            return;
        }

        if (mission == null)
        {
            Debug.LogError(
                "[Interactor] MissionData NULL!"
            );

            return;
        }

        Debug.Log(
            "[Interactor] Sending progress:"
        );

        Debug.Log(
            "Mission = " +
            mission.title
        );

        Debug.Log(
            "Mission ID = " +
            mission.missionID
        );

        Debug.Log(
            "Target ID = " +
            mission.targetID
        );

        Debug.Log(
            "Amount = " +
            progressAmount
        );

        if (interactionType ==
            InteractionType.Custom)
        {
            MissionManager.Instance.CompleteMission();
        }
        else
        {
            MissionManager.Instance.AddProgress(
                mission.targetID,
                progressAmount
            );
        }
    }

    // =========================================================
    // STOP
    // =========================================================

    private void StopInteraction()
    {
        if (interactionCoroutine != null)
        {
            StopCoroutine(
                interactionCoroutine
            );

            interactionCoroutine = null;
        }

        interacting = false;

        HideProgressUI();

        if (playerColliders.Count > 0 &&
            !interacted)
        {
            ShowInteractionUI();
        }
    }

    // =========================================================
    // INTERACTION UI
    // =========================================================

    private void ShowInteractionUI()
    {
        if (interactionPanel == null)
            return;

        if (interactionText != null)
        {
            interactionText.text =
                interactionMessage;
        }

        interactionPanel.SetActive(true);

        if (interactionCanvasGroup == null)
            return;

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );
        }

        fadeCoroutine =
            StartCoroutine(
                FadeInteraction(1f)
            );
    }

    private void HideInteractionUI()
    {
        if (interactionPanel == null)
            return;

        if (interactionCanvasGroup == null)
        {
            interactionPanel.SetActive(false);
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(
                fadeCoroutine
            );
        }

        fadeCoroutine =
            StartCoroutine(
                FadeInteraction(0f)
            );
    }

    private IEnumerator FadeInteraction(
        float targetAlpha)
    {
        interactionPanel.SetActive(true);

        float startAlpha =
            interactionCanvasGroup.alpha;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            interactionCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    t
                );

            yield return null;
        }

        /*
         * Обязательно устанавливаем
         * точное значение.
         */
        interactionCanvasGroup.alpha =
            targetAlpha;

        if (targetAlpha <= 0f)
        {
            interactionPanel.SetActive(false);
        }

        fadeCoroutine = null;
    }

    // =========================================================
    // PROGRESS UI
    // =========================================================

    private void ShowProgressUI()
    {
        Debug.Log(
            "[Interactor] SHOW PROGRESS UI"
        );

        if (progressPanel == null)
        {
            Debug.LogError(
                "[Interactor] ProgressPanel NULL!"
            );

            return;
        }

        if (progressSlider == null)
        {
            Debug.LogError(
                "[Interactor] ProgressSlider NULL!"
            );

            return;
        }

        progressPanel.SetActive(true);

        progressSlider.gameObject.SetActive(true);

        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.value = 0f;

        Debug.Log(
            "[Interactor] ProgressPanel active = " +
            progressPanel.activeSelf
        );

        Debug.Log(
            "[Interactor] Slider active = " +
            progressSlider.gameObject.activeSelf
        );
    }

    private void HideProgressUI()
    {
        if (progressSlider != null)
        {
            progressSlider.value = 0f;
        }

        if (progressPanel != null)
        {
            progressPanel.SetActive(false);
        }
    }

    private void SetProgress(float value)
    {
        if (progressSlider == null)
        {
            Debug.LogError(
                "[Interactor] Slider NULL!"
            );

            return;
        }

        if (progressPanel != null &&
            !progressPanel.activeSelf)
        {
            progressPanel.SetActive(true);
        }

        progressSlider.value =
            Mathf.Clamp01(value);
    }

    // =========================================================
    // MISSION COMPLETED
    // =========================================================

    private void OnMissionCompleted(
        MissionData completedMission)
    {
        if (completedMission == null)
            return;

        if (mission == null)
            return;

        if (completedMission != mission)
            return;

        Debug.Log(
            "[Interactor] MY MISSION COMPLETED!"
        );

        interacted = true;
        interacting = false;

        if (interactionCoroutine != null)
        {
            StopCoroutine(
                interactionCoroutine
            );

            interactionCoroutine = null;
        }

        StartCoroutine(
            DisableAfterCompletion()
        );
    }

    private IEnumerator DisableAfterCompletion()
    {
        yield return new WaitForSeconds(
            hideUIDelay
        );

        HideInteractionUI();
        HideProgressUI();

        gameObject.SetActive(false);
    }

    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        /*
         * Добавляем конкретный Collider.
         */
        playerColliders.Add(other);

        Debug.Log(
            "[Interactor] PLAYER ENTERED: " +
            gameObject.name +
            " | Colliders inside: " +
            playerColliders.Count
        );

        if (MissionManager.Instance == null)
        {
            Debug.LogWarning(
                "[Interactor] MissionManager.Instance is NULL!"
            );

            return;
        }

        MissionData current =
            MissionManager.Instance.GetCurrentMission();

        if (current == mission)
        {
            Debug.Log(
                "[Interactor] Correct mission detected!"
            );

            ShowInteractionUI();
        }
    }

    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        /*
         * Удаляем только этот Collider.
         */
        playerColliders.Remove(other);

        Debug.Log(
            "[Interactor] PLAYER EXITED: " +
            gameObject.name +
            " | Colliders inside: " +
            playerColliders.Count
        );

        /*
         * Только когда ВСЕ Collider'ы игрока
         * вышли из Trigger, считаем,
         * что игрок действительно вышел.
         */
        if (playerColliders.Count > 0)
            return;

        if (interacting)
        {
            StopInteraction();
        }

        HideInteractionUI();
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoInSubmarine : MonoBehaviour
{
    [Header("Объекты")]
    [SerializeField] private GameObject currentCamera;
    [SerializeField] private GameObject animCamera;
    [SerializeField] private GameObject submarine;
    [SerializeField] private GameObject finalpoint;

    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Подводный туман")]
    [SerializeField]
    private Color underwaterFogColor =
        new Color(0.03f, 0.22f, 0.28f);

    [SerializeField, Range(0f, 0.2f)]
    private float underwaterFogDensity = 0.045f;

    private bool isStarted;
    private int playerCollidersInside;

    private void Awake()
    {
        if (submarine != null)
            submarine.SetActive(false);

        if (animCamera != null)
            animCamera.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerCollidersInside++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerCollidersInside = Mathf.Max(0, playerCollidersInside - 1);
    }

    private void Update()
    {
        if (isStarted || playerCollidersInside == 0)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartSubmarineAnimation();
        }
    }

    private void StartSubmarineAnimation()
    {
        if (currentCamera == null || animCamera == null ||
            submarine == null || finalpoint == null)
        {
            Debug.LogError(
                "Назначь Current Camera, Anim Camera, Submarine и Finalpoint в Inspector.",
                this
            );
            return;
        }

        isStarted = true;

        currentCamera.SetActive(false);
        animCamera.SetActive(true);
        submarine.SetActive(true);

        EnableUnderwaterFog();

        StartCoroutine(Animation());
    }

    private void EnableUnderwaterFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = underwaterFogColor;
        RenderSettings.fogDensity = underwaterFogDensity;

        // Фон камеры тоже окрашиваем в цвет воды.
        Camera camera = animCamera.GetComponentInChildren<Camera>();

        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = underwaterFogColor;
        }
    }

    private IEnumerator Animation()
    {
        Transform sub = submarine.transform;
        Vector3 targetPosition = finalpoint.transform.position;
        Vector3 direction = targetPosition - sub.position;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction.normalized, Vector3.up);

            // Поворачиваем субмарину к цели.
            while (Quaternion.Angle(sub.rotation, targetRotation) > 0.1f)
            {
                sub.rotation = Quaternion.RotateTowards(
                    sub.rotation,
                    targetRotation,
                    Mathf.Max(rotationSpeed, 0.01f) * Time.deltaTime
                );

                yield return null;
            }

            sub.rotation = targetRotation;

            // Плывём к цели.
            while ((sub.position - targetPosition).sqrMagnitude > 0.0001f)
            {
                sub.position = Vector3.MoveTowards(
                    sub.position,
                    targetPosition,
                    Mathf.Max(moveSpeed, 0.01f) * Time.deltaTime
                );

                yield return null;
            }
        }

        sub.position = targetPosition;
        LoadScene();
    }

    public void LoadScene()
    {
        const string sceneName = "SeaDiving_Scene";
        const float fadeDuration = 1f;

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Добавь сцену в список сцен сборки: " + sceneName);
            return;
        }

        if (GameObject.Find("__SceneFadeTransition") != null)
            return;

        GameObject overlay = new GameObject(
            "__SceneFadeTransition",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(Image)
        );

        DontDestroyOnLoad(overlay);

        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        Image image = overlay.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        CanvasGroup group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        // Постоянный MonoBehaviour нужен, чтобы продолжить
        // корутину после уничтожения объектов старой сцены.
        // Используем копию текущего компонента без запуска его логики.
        GameObject runnerObject = Instantiate(gameObject);
        runnerObject.SetActive(false);

        foreach (Transform child in runnerObject.transform)
            Destroy(child.gameObject);

        foreach (Component component in runnerObject.GetComponents<Component>())
        {
            if (component is Transform)
                continue;

            if (component != runnerObject.GetComponent(GetType()))
                Destroy(component);
        }

        MonoBehaviour runner =
            (MonoBehaviour)runnerObject.GetComponent(GetType());

        runner.enabled = false;
        runnerObject.transform.SetParent(overlay.transform, false);
        runnerObject.SetActive(true);

        runner.StartCoroutine(Transition());

        IEnumerator Transition()
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.SmoothStep(
                    0f, 1f, Mathf.Clamp01(elapsed / fadeDuration)
                );
                yield return null;
            }

            group.alpha = 1f;

            AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);

            while (!loading.isDone)
                yield return null;

            yield return null;

            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.SmoothStep(
                    1f, 0f, Mathf.Clamp01(elapsed / fadeDuration)
                );
                yield return null;
            }

            Destroy(overlay);
        }
    }
}
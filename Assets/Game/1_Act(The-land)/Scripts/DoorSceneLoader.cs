using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DoorSceneLoader : MonoBehaviour
{
    [Header("Следующая сцена")]
    [SerializeField] private string sceneName = "НазваниеСцены";

    [Header("Задержка после начала открытия двери")]
    [SerializeField, Min(0f)] private float openingDelay = 1f;

    [Header("Затемнение")]
    [SerializeField, Min(0.01f)] private float fadeOutDuration = 1f;
    [SerializeField, Min(0.01f)] private float fadeInDuration = 1f;

    private OpenDoor door;
    private bool triggered;

    private static DoorSceneLoader activeTransition;

    private void Awake()
    {
        door = GetComponent<OpenDoor>();
    }

    private void Update()
    {
        if (triggered || door == null)
            return;

        if (door.IsOpen)
        {
            triggered = true;
            LoadScene();
        }
    }

    public void LoadScene()
    {
        if (activeTransition != null)
            return;

        if (string.IsNullOrWhiteSpace(sceneName) ||
            !Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "Проверь название сцены и добавь её в список сцен сборки.",
                this
            );
            return;
        }

        triggered = true;

        // Отдельный объект продолжит переход после загрузки сцены.
        GameObject transitionObject =
            new GameObject("Door Scene Transition");

        DoorSceneLoader runner =
            transitionObject.AddComponent<DoorSceneLoader>();

        runner.triggered = true;
        activeTransition = runner;

        DontDestroyOnLoad(transitionObject);

        runner.StartCoroutine(runner.Transition(
            sceneName,
            openingDelay,
            fadeOutDuration,
            fadeInDuration
        ));
    }

    private IEnumerator Transition(
        string targetScene,
        float delay,
        float fadeOut,
        float fadeIn
    )
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delay));

        GameObject canvasObject = new GameObject(
            "Fade Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        CanvasGroup group = canvasObject.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;

        GameObject imageObject = new GameObject(
            "Black Screen",
            typeof(RectTransform),
            typeof(Image)
        );

        imageObject.transform.SetParent(canvasObject.transform, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Затемняем экран.
        yield return Fade(group, 0f, 1f, fadeOut);

        // Загружаем сцену под чёрным экраном.
        AsyncOperation loading =
            SceneManager.LoadSceneAsync(targetScene);

        if (loading != null)
        {
            while (!loading.isDone)
                yield return null;
        }

        // Даём новой сцене выполнить Start.
        yield return null;

        // Плавно показываем новую сцену.
        yield return Fade(group, 1f, 0f, fadeIn);

        Destroy(gameObject);
    }

    private IEnumerator Fade(
        CanvasGroup group,
        float from,
        float to,
        float duration
    )
    {
        duration = Mathf.Max(0.01f, duration);
        float elapsed = 0f;

        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            group.alpha = Mathf.SmoothStep(from, to, t);

            yield return null;
        }

        group.alpha = to;
    }

    private void OnDestroy()
    {
        if (activeTransition == this)
            activeTransition = null;
    }
}
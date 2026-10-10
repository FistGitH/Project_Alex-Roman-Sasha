using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoInBase : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Submarine"))
        {
            LoadScene();
        }
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
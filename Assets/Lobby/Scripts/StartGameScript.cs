using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlay;
    private bool isLoading;

    public void LoadGame()
    {
        if (isLoading)
            return;

        isLoading = true;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        const float duration = 0.5f;
        fadeOverlay.alpha = 0f;
        fadeOverlay.gameObject.SetActive(true);
        const float startAlpha = 0f;
        float elapsed = 0f;

        fadeOverlay.blocksRaycasts = true;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }

        fadeOverlay.alpha = 1f;
        yield return SceneManager.LoadSceneAsync("OpeningCutScene");
    }
}
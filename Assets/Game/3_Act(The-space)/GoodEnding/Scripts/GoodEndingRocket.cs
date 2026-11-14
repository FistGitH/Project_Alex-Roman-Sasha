using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GoodEndingRocket : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    [SerializeField] private Camera shotCamera;
    [SerializeField] private Material particleMaterial;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField, Min(0.1f)] private float flightDuration = 6f;
    [SerializeField, Min(0f)] private float flightDistance = 8f;

    private Texture2D particleTexture;
    private Material burstMaterial;

    private IEnumerator Start()
    {
        Renderer[] renderers = rocket.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(rocket.position, Vector3.zero);
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            foreach (Renderer rocketRenderer in renderers)
                bounds.Encapsulate(rocketRenderer.bounds);
        }

        Vector3 travel = rocket.up * flightDistance;
        Vector3 start = rocket.position;
        float radius = Mathf.Max(1f, bounds.extents.magnitude);
        FrameShot(bounds.center + travel * 0.5f, radius, travel.magnitude);

        for (float elapsed = 0f; elapsed < flightDuration; elapsed += Time.deltaTime)
        {
            rocket.position = start + travel * (elapsed / flightDuration);
            yield return null;
        }

        rocket.position = start + travel;
        rocket.gameObject.SetActive(false);

        Vector3 burstPosition = bounds.center + travel;
        CreateParticleMaterial();
        var explosionAudio = gameObject.AddComponent<AudioSource>();
        if (explosionSound != null)
            explosionAudio.PlayOneShot(explosionSound, 0.85f);

        PlayBurst("Flash", burstPosition, radius, 52, 0.22f, 1.3f,
            new Color(1f, 0.93f, 0.68f), ParticleSystemRenderMode.Billboard);
        PlayBurst("Fireball", burstPosition, radius, 140, 1.15f, 2.2f,
            new Color(1f, 0.32f, 0.04f), ParticleSystemRenderMode.Billboard);
        PlayBurst("Sparks", burstPosition, radius, 185, 1.55f, 4f,
            new Color(1f, 0.78f, 0.24f), ParticleSystemRenderMode.Stretch);
        PlayBurst("Smoke", burstPosition, radius, 110, 3.7f, 0.9f,
            new Color(0.32f, 0.29f, 0.28f), ParticleSystemRenderMode.Billboard);

        yield return new WaitForSecondsRealtime(2f);
        yield return FadeAndLoadLobby();
    }

    private IEnumerator FadeAndLoadLobby()
    {
        var overlay = new GameObject("Lobby Fade", typeof(Canvas), typeof(CanvasGroup));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;

        var panel = new GameObject("Black Screen", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);
        panel.GetComponent<Image>().color = Color.black;

        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        const float fadeDuration = 1f;
        for (float elapsed = 0f; elapsed < fadeDuration; elapsed += Time.unscaledDeltaTime)
        {
            group.alpha = elapsed / fadeDuration;
            yield return null;
        }

        group.alpha = 1f;
        yield return null;
        yield return SceneManager.LoadSceneAsync("Lobby_Scene", LoadSceneMode.Single);
    }

    private void FrameShot(Vector3 center, float radius, float travelDistance)
    {
        float halfVerticalFov = shotCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float halfHorizontalFov = Mathf.Atan(Mathf.Tan(halfVerticalFov) * shotCamera.aspect);
        float halfFov = Mathf.Min(halfVerticalFov, halfHorizontalFov);
        float distance = (radius * 2.3f + travelDistance * 0.5f) / Mathf.Sin(halfFov);
        Vector3 offset = new Vector3(0.65f, 0.15f, -1f).normalized * distance;
        shotCamera.transform.SetPositionAndRotation(center + offset, Quaternion.LookRotation(-offset));
    }

    private void CreateParticleMaterial()
    {
        const int size = 32;
        particleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        particleTexture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color32[size * size];
        Vector2 center = Vector2.one * ((size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / center.x;
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, distance);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255f * alpha));
            }
        }

        particleTexture.SetPixels32(pixels);
        particleTexture.Apply(false, true);
        burstMaterial = new Material(particleMaterial) { mainTexture = particleTexture };
    }

    private void PlayBurst(string effectName, Vector3 position, float radius,
        int count, float lifetime, float speed, Color color, ParticleSystemRenderMode renderMode)
    {
        var effect = new GameObject(effectName);
        effect.SetActive(false);
        effect.transform.SetParent(transform, false);
        effect.transform.position = position;

        var particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = lifetime;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(radius * speed * 0.4f, radius * speed);
        float size = renderMode == ParticleSystemRenderMode.Stretch ? radius * 0.10f : radius * 0.72f;
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.45f, size);
        main.startColor = color;
        main.maxParticles = count;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radius * 0.18f;

        var emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = fade;

        var particleRenderer = effect.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sharedMaterial = burstMaterial;
        particleRenderer.renderMode = renderMode;
        effect.SetActive(true);
        particles.Play();
    }

    private void OnDestroy()
    {
        Destroy(burstMaterial);
        Destroy(particleTexture);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RocketLaunchingAnim : MonoBehaviour
{
    [Header("Ракета")]
    [SerializeField] private Transform rocket;
    [SerializeField] private Renderer rocketRenderer;

    [Header("Один материал — Standard или URP/Lit")]
    [SerializeField] private Material rocketMaterial;
    [SerializeField] private Color emissionColor = Color.white;
    [SerializeField, Min(0f)] private float emissionIntensity = 10f;

    [Header("Камеры")]
    [SerializeField] private Camera approachCamera;
    [SerializeField] private Camera engineCamera;
    [SerializeField] private Camera overviewCamera;
    [SerializeField] private Camera launchCamera;

    [Header("Первая камера")]
    [SerializeField] private Transform approachEnd;
    [SerializeField, Min(0.1f)] private float approachDuration = 5f;

    [Header("Двигатели")]
    [SerializeField, Min(0f)] private float ignitionDelay = 1f;
    [SerializeField, Min(0f)] private float engineHoldDuration = 2f;
    [SerializeField] private ParticleSystem[] engineParticles;

    [Header("Третья камера")]
    [SerializeField] private Transform overviewEnd;
    [SerializeField, Min(0.1f)] private float overviewDuration = 5f;

    [Header("Взлёт")]
    [SerializeField] private Vector3 launchDirection = Vector3.up;
    [SerializeField, Min(0f)] private float launchDistance = 150f;
    [SerializeField, Min(0.1f)] private float launchDuration = 10f;
    [SerializeField, Min(0f)] private float launchDelay = 1f;

    [Header("Звуки")]
    [SerializeField] private AudioClip ignitionSound;
    [SerializeField] private AudioClip engineStartSound;
    [SerializeField] private AudioClip engineLoopSound;
    [SerializeField] private AudioClip launchRumbleSound;

    [SerializeField, Range(0f, 1f)] private float ignitionVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float engineStartVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float engineLoopVolume = 0.7f;
    [SerializeField, Range(0f, 1f)] private float launchRumbleVolume = 0.8f;
    [SerializeField, Min(0f)] private float engineStartDelay = 0.3f;

    [Header("Музыка")]
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Тряска последней камеры")]
    [SerializeField, Min(0f)] private float shakePositionStrength = 0.08f;
    [SerializeField, Min(0f)] private float shakeRotationStrength = 0.6f;
    [SerializeField, Min(0.1f)] private float shakeFrequency = 25f;
    [SerializeField, Min(0.01f)] private float shakeFadeIn = 1f;
    [SerializeField, Min(0.01f)] private float shakeFadeOut = 1f;

    [Header("Переход между сценами")]
    [SerializeField] private string nextSceneName;
    [SerializeField, Min(0f)] private float endDelay = 1f;
    [SerializeField, Min(0.01f)] private float screenFadeOutDuration = 2f;
    [SerializeField, Min(0.01f)] private float musicFadeOutDuration = 2f;
    [SerializeField, Min(0.01f)] private float screenFadeInDuration = 2f;

    private Camera[] cameras;
    private Renderer[] rocketRenderers;

    private Material runtimeMaterial;
    private Material[] originalMaterials;

    private AudioSource ignitionSource;
    private AudioSource engineStartSource;
    private AudioSource engineLoopSource;
    private AudioSource rumbleSource;

    private Coroutine engineAudioCoroutine;
    private RocketSceneTransition transition;

    private bool ready;
    private bool trackRocket;
    private bool isLaunching;

    private Vector3 launchCameraPosition;
    private float launchElapsed;
    private float shakeAmount;

    private static readonly int EmissionColorId =
        Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        cameras = new[]
        {
            approachCamera, engineCamera, overviewCamera, launchCamera
        };

        if (rocket == null || rocketRenderer == null ||
            rocketMaterial == null ||
            approachEnd == null || overviewEnd == null)
        {
            Debug.LogError("Назначь ракету, материал и точки камер.", this);
            return;
        }

        if (rocketRenderer.transform != rocket &&
            !rocketRenderer.transform.IsChildOf(rocket))
        {
            Debug.LogError("Rocket Renderer должен быть внутри Rocket.", this);
            return;
        }

        foreach (Camera cam in cameras)
        {
            if (cam == null || !cam.gameObject.activeInHierarchy ||
                cam.transform == rocket)
            {
                Debug.LogError("Проверь четыре камеры.", this);
                return;
            }
        }

        if (!rocketMaterial.HasProperty(EmissionColorId))
        {
            Debug.LogError("Материал не поддерживает _EmissionColor.", this);
            return;
        }

        originalMaterials = rocketRenderer.sharedMaterials;

        if (System.Array.IndexOf(originalMaterials, rocketMaterial) < 0)
        {
            Debug.LogError("Материал не назначен Rocket Renderer.", this);
            return;
        }

        runtimeMaterial = new Material(rocketMaterial);
        Material[] materials = (Material[])originalMaterials.Clone();

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] == rocketMaterial)
                materials[i] = runtimeMaterial;
        }

        rocketRenderer.sharedMaterials = materials;

        foreach (Camera cam in cameras)
        {
            if (cam.transform.IsChildOf(rocket))
                cam.transform.SetParent(null, true);
        }

        rocketRenderers = rocket.GetComponentsInChildren<Renderer>();
        SetEmission(0f);

        if (engineParticles != null)
        {
            foreach (ParticleSystem effect in engineParticles)
            {
                if (effect != null)
                    effect.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear
                    );
            }
        }

        ignitionSource = CreateAudioSource(false);
        engineStartSource = CreateAudioSource(false);
        engineLoopSource = CreateAudioSource(true);
        rumbleSource = CreateAudioSource(true);

        // Отдельный объект переживёт загрузку сцены.
        GameObject transitionObject = new GameObject("Rocket Scene Transition");
        transition = transitionObject.AddComponent<RocketSceneTransition>();
        transition.Initialize(music, musicVolume);

        ready = true;
    }

    private AudioSource CreateAudioSource(bool loop)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    private IEnumerator Start()
    {
        if (!ready)
            yield break;

        transition.PlayMusic();

        // 1. Подлёт.
        SwitchCamera(approachCamera);

        Vector3 startPosition = approachCamera.transform.position;
        Vector3 finishPosition = approachEnd.position;

        float duration = Mathf.Max(0.1f, approachDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            approachCamera.transform.position = Vector3.Lerp(
                startPosition, finishPosition, Mathf.SmoothStep(0f, 1f, t)
            );

            LookAtRocket(approachCamera);
            yield return null;
        }

        // 2. Двигатели.
        SwitchCamera(engineCamera);
        yield return new WaitForSeconds(ignitionDelay);

        SetEmission(emissionIntensity);
        PlaySound(ignitionSource, ignitionSound, ignitionVolume);
        engineAudioCoroutine = StartCoroutine(PlayEngineAudio());

        if (engineParticles != null)
        {
            foreach (ParticleSystem effect in engineParticles)
            {
                if (effect != null)
                    effect.Play(true);
            }
        }

        yield return new WaitForSeconds(engineHoldDuration);

        // 3. Поворот камеры снизу вверх.
        SwitchCamera(overviewCamera);

        Quaternion startRotation = overviewCamera.transform.rotation;
        Quaternion finishRotation = overviewEnd.rotation;

        duration = Mathf.Max(0.1f, overviewDuration);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            overviewCamera.transform.rotation = Quaternion.Slerp(
                startRotation, finishRotation, Mathf.SmoothStep(0f, 1f, t)
            );

            yield return null;
        }

        // 4. Неподвижная камера следит за ракетой.
        launchCamera.transform.SetParent(null, true);
        launchCameraPosition = launchCamera.transform.position;

        SwitchCamera(launchCamera);
        trackRocket = true;
        LookAtRocket(launchCamera);

        yield return new WaitForSeconds(launchDelay);

        isLaunching = true;
        launchElapsed = 0f;
        PlaySound(rumbleSource, launchRumbleSound, 0f);

        Vector3 rocketStart = rocket.position;
        Vector3 direction = launchDirection.sqrMagnitude > 0.0001f
            ? launchDirection.normalized
            : Vector3.up;

        duration = Mathf.Max(0.1f, launchDuration);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            launchElapsed = elapsed;

            float t = Mathf.Clamp01(elapsed / duration);
            rocket.position =
                rocketStart + direction * launchDistance * t * t;

            yield return null;
        }

        isLaunching = false;

        // Плавно убираем тряску и звук взлёта.
        float initialShake = shakeAmount;
        float initialVolume = rumbleSource.volume;

        duration = Mathf.Max(0.01f, shakeFadeOut);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            shakeAmount = Mathf.Lerp(initialShake, 0f, t);
            rumbleSource.volume = Mathf.Lerp(initialVolume, 0f, t);

            yield return null;
        }

        shakeAmount = 0f;
        rumbleSource.Stop();

        yield return new WaitForSeconds(endDelay);
        LoadScene();
    }

    private IEnumerator PlayEngineAudio()
    {
        yield return new WaitForSeconds(engineStartDelay);

        if (engineStartSound != null)
        {
            PlaySound(engineStartSource, engineStartSound, engineStartVolume);

            while (engineStartSource.isPlaying)
                yield return null;
        }

        PlaySound(engineLoopSource, engineLoopSound, engineLoopVolume);
        engineAudioCoroutine = null;
    }

    private void PlaySound(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null)
            return;

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    private void LateUpdate()
    {
        if (!trackRocket || launchCamera == null || rocket == null)
            return;

        if (isLaunching)
        {
            shakeAmount = Mathf.Clamp01(
                launchElapsed / Mathf.Max(0.01f, shakeFadeIn)
            );

            rumbleSource.volume = launchRumbleVolume * shakeAmount;
        }

        float noiseTime = Time.time * shakeFrequency;

        Vector3 positionNoise = new Vector3(
            Noise(10f, noiseTime),
            Noise(20f, noiseTime),
            Noise(30f, noiseTime)
        );

        launchCamera.transform.position =
            launchCameraPosition +
            positionNoise * shakePositionStrength * shakeAmount;

        LookAtRocket(launchCamera);

        Vector3 rotationNoise = new Vector3(
            Noise(40f, noiseTime),
            Noise(50f, noiseTime),
            Noise(60f, noiseTime)
        );

        launchCamera.transform.rotation *= Quaternion.Euler(
            rotationNoise * shakeRotationStrength * shakeAmount
        );
    }

    private float Noise(float seed, float time)
    {
        return Mathf.PerlinNoise(seed, time) * 2f - 1f;
    }

    private Vector3 GetRocketCenter()
    {
        Bounds bounds = new Bounds();
        bool found = false;

        foreach (Renderer modelRenderer in rocketRenderers)
        {
            if (modelRenderer == null ||
                !modelRenderer.enabled ||
                !modelRenderer.gameObject.activeInHierarchy ||
                modelRenderer is ParticleSystemRenderer ||
                modelRenderer is TrailRenderer ||
                modelRenderer is LineRenderer)
                continue;

            if (!found)
            {
                bounds = modelRenderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(modelRenderer.bounds);
            }
        }

        return found ? bounds.center : rocket.position;
    }

    private void LookAtRocket(Camera cam)
    {
        Vector3 direction = GetRocketCenter() - cam.transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        Vector3 up = Mathf.Abs(Vector3.Dot(direction, Vector3.up)) > 0.999f
            ? Vector3.forward
            : Vector3.up;

        cam.transform.rotation = Quaternion.LookRotation(direction, up);
    }

    private void SetEmission(float intensity)
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetColor(
            EmissionColorId, emissionColor * Mathf.Max(0f, intensity)
        );

        if (intensity > 0f)
            runtimeMaterial.EnableKeyword("_EMISSION");
        else
            runtimeMaterial.DisableKeyword("_EMISSION");
    }

    private void SwitchCamera(Camera activeCamera)
    {
        foreach (Camera cam in cameras)
        {
            bool active = cam == activeCamera;
            cam.enabled = active;

            if (cam.TryGetComponent(out AudioListener listener))
                listener.enabled = active;
        }
    }

    public void LoadScene()
    {
        if (!ready || transition == null || transition.IsTransitioning)
            return;

        if (string.IsNullOrWhiteSpace(nextSceneName) ||
            !Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError(
                "Укажи следующую сцену и добавь её в список сцен сборки.",
                this
            );
            return;
        }

        transition.Begin(
            nextSceneName,
            screenFadeOutDuration,
            musicFadeOutDuration,
            screenFadeInDuration,
            StopAllAudio
        );
    }

    private void StopAllAudio()
    {
        if (engineAudioCoroutine != null)
        {
            StopCoroutine(engineAudioCoroutine);
            engineAudioCoroutine = null;
        }

        if (ignitionSource != null) ignitionSource.Stop();
        if (engineStartSource != null) engineStartSource.Stop();
        if (engineLoopSource != null) engineLoopSource.Stop();
        if (rumbleSource != null) rumbleSource.Stop();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        StopAllAudio();

        isLaunching = false;
        shakeAmount = 0f;

        if (trackRocket && launchCamera != null)
        {
            launchCamera.transform.position = launchCameraPosition;

            if (rocket != null)
                LookAtRocket(launchCamera);
        }

        trackRocket = false;

        if (transition != null && !transition.IsTransitioning)
            Destroy(transition.gameObject);
    }

    private void OnDestroy()
    {
        if (runtimeMaterial == null)
            return;

        if (rocketRenderer != null && originalMaterials != null)
            rocketRenderer.sharedMaterials = originalMaterials;

        Destroy(runtimeMaterial);
    }
}

// Этот компонент создаётся автоматически.
// Он сохраняет чёрный экран и музыку во время загрузки сцены.
public class RocketSceneTransition : MonoBehaviour
{
    public bool IsTransitioning { get; private set; }

    private CanvasGroup fadeGroup;
    private AudioSource musicSource;

    public void Initialize(AudioClip music, float volume)
    {
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.spatialBlend = 0f;
        musicSource.clip = music;
        musicSource.volume = volume;

        GameObject canvasObject = new GameObject(
            "Fade Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasGroup)
        );

        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        fadeGroup = canvasObject.GetComponent<CanvasGroup>();
        fadeGroup.alpha = 0f;
        fadeGroup.interactable = false;
        fadeGroup.blocksRaycasts = false;

        GameObject imageObject = new GameObject(
            "Black Screen",
            typeof(RectTransform),
            typeof(Image)
        );

        imageObject.transform.SetParent(canvasObject.transform, false);

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    public void PlayMusic()
    {
        if (musicSource.clip != null)
            musicSource.Play();
    }

    public void Begin(
        string sceneName,
        float screenFadeOut,
        float musicFadeOut,
        float screenFadeIn,
        System.Action stopEffects
    )
    {
        if (IsTransitioning)
            return;

        IsTransitioning = true;

        StartCoroutine(Transition(
            sceneName,
            screenFadeOut,
            musicFadeOut,
            screenFadeIn,
            stopEffects
        ));
    }

    private IEnumerator Transition(
        string sceneName,
        float screenFadeOut,
        float musicFadeOut,
        float screenFadeIn,
        System.Action stopEffects
    )
    {
        // Сначала затемняем экран.
        float duration = Mathf.Max(0.01f, screenFadeOut);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.SmoothStep(
                0f, 1f, Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        fadeGroup.alpha = 1f;
        stopEffects?.Invoke();

        // Затем плавно уменьшаем громкость музыки.
        float startVolume = musicSource.volume;
        duration = Mathf.Max(0.01f, musicFadeOut);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            musicSource.volume = Mathf.Lerp(
                startVolume, 0f, Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Чёрный экран остаётся поверх новой сцены при загрузке.
        AsyncOperation loading = SceneManager.LoadSceneAsync(sceneName);

        if (loading != null)
        {
            while (!loading.isDone)
                yield return null;
        }

        // Даём новой сцене выполнить Start перед появлением.
        yield return null;

        duration = Mathf.Max(0.01f, screenFadeIn);
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.SmoothStep(
                1f, 0f, Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        fadeGroup.alpha = 0f;
        Destroy(gameObject);
    }
}
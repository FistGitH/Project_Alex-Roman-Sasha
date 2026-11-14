using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BadEndingRocket : MonoBehaviour
{
    [SerializeField] private Transform rocket;
    [SerializeField] private Camera shotCamera;
    [SerializeField] private Material particleMaterial;
    [SerializeField] private GameObject forestTree;
    private Material forestBark;
    private Mesh grassMesh;
    private Material grassMaterial;
    private Vector3[] grassVertices;
    private float[] grassTipX;
    [SerializeField] private Texture2D forestFloorTexture;
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
        var explosionSound = Resources.Load<AudioClip>("BadEndingExplosion");
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

        yield return EscapeToForest(burstPosition, radius);
        yield return FadeAndLoadLobby();
    }

    private IEnumerator EscapeToForest(Vector3 origin, float radius)
    {
        var capsule = Primitive(PrimitiveType.Capsule, "Escape Pod", origin,
            Vector3.one * radius * 0.13f, new Color(0.32f, 0.34f, 0.37f));
        AddPodLight(capsule.transform);
        Vector3 direction = (rocket.right - rocket.up * 0.65f).normalized;
        capsule.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

        const float escapeDuration = 1.3f;
        for (float t = 0f; t < escapeDuration; t += Time.deltaTime)
        {
            capsule.transform.position = origin + direction * (radius * 3f * t / escapeDuration);
            yield return null;
        }

        Vector3 forest = new Vector3(0f, -100f, 0f);
        var floor = Primitive(PrimitiveType.Plane, "Forest Floor", forest,
            new Vector3(9f, 1f, 9f), new Color(0.53f, 0.68f, 0.39f));
        var floorRenderer = floor.GetComponent<Renderer>();
        var ground = new MaterialPropertyBlock();
        floorRenderer.GetPropertyBlock(ground);
        ground.SetTexture("_BaseMap", forestFloorTexture);
        ground.SetVector("_BaseMap_ST", new Vector4(34f, 34f, 0f, 0f));
        floorRenderer.SetPropertyBlock(ground);

        forestBark = Resources.Load<Material>("ForestBark");
        var random = new System.Random(37);
        for (int i = 0; i < 76; i++)
        {
            float angle = i * 2.4f;
            float distance = 6f + (float)random.NextDouble() * 24f;
            if (Mathf.Sin(angle) < -0.55f && distance < 11f)
                continue;

            Vector3 root = forest + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            PlaceTree(root, angle * Mathf.Rad2Deg, 3.8f + (float)random.NextDouble() * 3.8f);
        }

        // Stagger the back rows so the clearing does not open onto the horizon.
        for (int row = 0; row < 5; row++)
        {
            int count = 13 + row * 3;
            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * 2.35f;
                x += (float)random.NextDouble() * 1.5f - 0.75f;
                float z = 5f + row * 4.1f + (float)random.NextDouble() * 2.6f;
                float height = 3.4f + (float)random.NextDouble() * 4.9f;
                PlaceTree(forest + new Vector3(x, 0f, z), (float)random.NextDouble() * 360f, height);
            }
        }

        CreateGrass(forest);

        Vector3 cameraOffset = new Vector3(1.8f, 1.05f, -4.4f);
        PlaceTree(forest + new Vector3(2.58f, 0f, -4.0f), 20f, 4.8f);
        shotCamera.nearClipPlane = 0.06f;
        shotCamera.fieldOfView = 55f;
        shotCamera.transform.position = forest + cameraOffset;
        shotCamera.transform.LookAt(forest + Vector3.up * 0.42f);
        Color horizon = new Color(0.26f, 0.38f, 0.34f);
        shotCamera.backgroundColor = horizon;
        RenderSettings.fog = true;
        RenderSettings.fogColor = horizon;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.067f;
        shotCamera.clearFlags = CameraClearFlags.SolidColor;

        var forestAmbience = Resources.Load<AudioClip>("BadEndingForest");
        if (forestAmbience != null)
        {
            var ambience = gameObject.AddComponent<AudioSource>();
            ambience.clip = forestAmbience;
            ambience.loop = true;
            ambience.volume = 0.32f;
            ambience.Play();
        }

        Vector3 landing = forest + Vector3.up * 0.15f;
        Vector3 approach = landing + new Vector3(-5f, 7f, 2f);
        capsule.transform.localScale = Vector3.one * 0.30f;
        Vector3 landingDirection = (landing - approach).normalized;
        capsule.transform.rotation = Quaternion.FromToRotation(Vector3.up, landingDirection);
        for (float t = 0f; t < 0.85f; t += Time.deltaTime)
        {
            capsule.transform.position = Vector3.Lerp(approach, landing, t / 0.85f);
            yield return null;
        }

        capsule.transform.position = landing;
        PlayBurst("Impact Dust", landing, 0.9f, 30, 0.9f, 1.3f,
            new Color(0.39f, 0.31f, 0.22f), ParticleSystemRenderMode.Billboard);
        Vector3 cameraStart = shotCamera.transform.position;
        const float shakeDuration = 0.35f;
        for (float t = 0f; t < shakeDuration; t += Time.deltaTime)
        {
            float strength = (1f - t / shakeDuration) * 0.055f;
            shotCamera.transform.position = cameraStart
                + shotCamera.transform.right * (Mathf.Sin(t * 91f) * strength)
                + Vector3.up * (Mathf.Sin(t * 73f) * strength);
            yield return null;
        }
        shotCamera.transform.position = cameraStart;

        var puddles = new Transform[8];
        for (int i = 0; i < puddles.Length; i++)
        {
            float angle = i * 2.4f;
            puddles[i] = Primitive(PrimitiveType.Sphere, "Black Ooze",
                forest + Vector3.up * (0.04f + i * 0.007f), Vector3.one * 0.05f,
                new Color(0.018f, 0.014f, 0.025f)).transform;
            puddles[i].rotation = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f);
        }

        const float spreadDuration = 2.8f;
        for (float t = 0f; t < spreadDuration; t += Time.deltaTime)
        {
            float shotProgress = Mathf.SmoothStep(0f, 1f, t / spreadDuration);
            shotCamera.transform.position = Vector3.Lerp(cameraStart,
                cameraStart + shotCamera.transform.forward * 0.55f, shotProgress);
            SwayGrass(t);
            for (int i = 0; i < puddles.Length; i++)
            {
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / spreadDuration * 1.5f - i * 0.06f));
                float angle = i * 2.4f;
                Vector3 spread = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (0.28f * i * progress);
                puddles[i].position = forest + spread + Vector3.up * (0.04f + i * 0.007f);
                puddles[i].localScale = new Vector3(0.05f + progress * 1.75f, 0.13f, 0.05f + progress);
            }
            yield return null;
        }
    }

    private void CreateGrass(Vector3 origin)
    {
        const int bladeCount = 2800;
        var vertices = new Vector3[bladeCount * 3];
        grassVertices = vertices;
        grassTipX = new float[bladeCount];
        var indices = new int[bladeCount * 6];
        var colors = new Color[vertices.Length];
        var random = new System.Random(52);
        for (int i = 0; i < bladeCount; i++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = Mathf.Lerp(1.25f, 14f, Mathf.Pow((float)random.NextDouble(), 1.5f));
            Vector3 position = new Vector3(Mathf.Cos(angle), 0.016f, Mathf.Sin(angle)) * distance;
            position.y = 0.016f;
            float bladeAngle = (float)random.NextDouble() * Mathf.PI * 2f;
            Vector3 side = new Vector3(Mathf.Cos(bladeAngle), 0f, Mathf.Sin(bladeAngle));
            float height = Mathf.Lerp(0.09f, 0.41f, (float)random.NextDouble());
            int index = i * 3;
            vertices[index] = position - side * 0.024f;
            vertices[index + 1] = position + side * 0.024f;
            vertices[index + 2] = position + Vector3.up * height + side * height * 0.31f;

            grassTipX[i] = vertices[index + 2].x;

            float shade = Mathf.Lerp(0.65f, 1.25f, (float)random.NextDouble());
            colors[index] = colors[index + 1] = new Color(0.08f, 0.15f, 0.034f) * shade;
            colors[index + 2] = new Color(0.26f, 0.42f, 0.09f) * shade;
            int triangle = i * 6;
            indices[triangle] = indices[triangle + 3] = index;
            indices[triangle + 1] = indices[triangle + 5] = index + 1;
            indices[triangle + 2] = indices[triangle + 4] = index + 2;
        }

        grassMesh = new Mesh { name = "Forest grass", vertices = vertices, colors = colors, triangles = indices };
        grassMesh.RecalculateNormals();
        var grass = new GameObject("Forest Grass", typeof(MeshFilter), typeof(MeshRenderer));
        grass.transform.SetParent(transform, false);
        grass.transform.position = origin;
        grass.GetComponent<MeshFilter>().sharedMesh = grassMesh;
        grassMaterial = new Material(particleMaterial);
        grassMaterial.color = Color.white;
        grass.GetComponent<MeshRenderer>().sharedMaterial = grassMaterial;
    }

    private void SwayGrass(float time)
    {
        for (int i = 0; i < grassTipX.Length; i++)
        {
            int tip = i * 3 + 2;
            float phase = grassVertices[tip].z * 0.8f + time * 3f;
            grassVertices[tip].x = grassTipX[i] + Mathf.Sin(phase) * grassVertices[tip].y * 0.23f;
        }
        grassMesh.vertices = grassVertices;
    }

    private void PlaceTree(Vector3 root, float yaw, float height)
    {
        if (forestTree == null)
            return;

        GameObject tree = Instantiate(forestTree, root, Quaternion.Euler(0f, yaw, 0f), transform);
        Renderer[] renderers = tree.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer treeRenderer in renderers)
        {
            bounds.Encapsulate(treeRenderer.bounds);
            treeRenderer.sharedMaterial = forestBark;
        }

        float scale = height / Mathf.Max(0.01f, bounds.size.y);
        tree.transform.localScale *= scale;
        tree.transform.position += Vector3.up * ((root.y - bounds.min.y) * scale);

        var random = new System.Random(Mathf.RoundToInt(yaw * 31f));
        for (int i = 0; i < 7; i++)
        {
            float angle = i * 2.4f + yaw * Mathf.Deg2Rad;
            float spread = height * (0.05f + (float)random.NextDouble() * 0.15f);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * spread,
                height * (0.69f + (float)random.NextDouble() * 0.23f), Mathf.Sin(angle) * spread);
            float shade = 0.72f + (float)random.NextDouble() * 0.42f;
            var canopy = Primitive(PrimitiveType.Sphere, "Foliage", root + offset,
                new Vector3(height * 0.28f, height * 0.19f, height * 0.24f),
                new Color(0.09f * shade, 0.17f * shade, 0.07f * shade));
            canopy.transform.rotation = Quaternion.Euler(12f, angle * Mathf.Rad2Deg, 15f);
        }
    }

    private void AddPodLight(Transform capsule)
    {
        var diode = Primitive(PrimitiveType.Sphere, "Red Status Light", capsule.position,
            Vector3.one, new Color(0.63f, 0.055f, 0.045f));
        diode.transform.SetParent(capsule, false);
        diode.transform.localPosition = new Vector3(0.39f, 0.23f, -0.24f);
        diode.transform.localScale = Vector3.one * 0.13f;

        var light = diode.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.10f, 0.06f);
        light.intensity = 0.45f;
        light.range = 0.45f;
    }

    private GameObject Primitive(PrimitiveType type, string objectName, Vector3 position, Vector3 scale, Color color)
    {
        var obj = GameObject.CreatePrimitive(type);
        obj.name = objectName;
        obj.transform.SetParent(transform, false);
        obj.transform.position = position;
        obj.transform.localScale = scale;
        Destroy(obj.GetComponent<Collider>());

        var renderer = obj.GetComponent<Renderer>();
        var properties = new MaterialPropertyBlock();
        properties.SetColor("_BaseColor", color);
        properties.SetColor("_Color", color);
        renderer.SetPropertyBlock(properties);
        return obj;
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
        Destroy(grassMesh);
        Destroy(grassMaterial);
        Destroy(burstMaterial);
        Destroy(particleTexture);
    }
}

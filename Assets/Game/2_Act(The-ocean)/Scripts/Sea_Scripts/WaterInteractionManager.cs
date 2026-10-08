using UnityEngine;

namespace OriginalWater
{
    /// <summary>
    /// Central manager for water interaction events: spawns splash VFX,
    /// ripple rings, and plays water impact audio.
    /// </summary>
    [AddComponentMenu("Original Water/Water Interaction Manager")]
    public class WaterInteractionManager : MonoBehaviour
    {
        public static WaterInteractionManager Instance { get; private set; }

        [Header("VFX Prefabs")]
        [Tooltip("Prefab spawned when objects hit the water at speed.")]
        [SerializeField] private GameObject splashEffectPrefab;

        [Header("Audio")]
        [Tooltip("Audio clip played when an object splashes into the water.")]
        [SerializeField] private AudioClip splashSound;
        [SerializeField] [Range(0f, 1f)] private float splashVolume = 0.8f;

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1.0f; // 3D sound
                _audioSource.minDistance = 2f;
                _audioSource.maxDistance = 60f;
            }
        }

        public static void TriggerSplash(Vector3 position, float impactSpeed = 3f)
        {
            if (Instance != null)
            {
                Instance.SpawnSplashInternal(position, impactSpeed);
            }
        }

        private void SpawnSplashInternal(Vector3 position, float impactSpeed)
        {
            // Spawn VFX
            if (splashEffectPrefab != null)
            {
                GameObject fx = Instantiate(splashEffectPrefab, position, Quaternion.identity);
                // Scale effect based on impact speed
                float scaleFactor = Mathf.Clamp(impactSpeed / 4f, 0.6f, 2.5f);
                fx.transform.localScale = Vector3.one * scaleFactor;
                Destroy(fx, 3.5f);
            }

            // Play SFX
            if (splashSound != null && _audioSource != null)
            {
                float pitch = Random.Range(0.85f, 1.15f);
                _audioSource.pitch = pitch;
                _audioSource.PlayOneShot(splashSound, splashVolume * Mathf.Clamp01(impactSpeed / 5f));
            }
        }
    }
}

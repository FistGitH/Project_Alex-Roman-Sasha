using UnityEngine;

namespace OriginalWater
{
    /// <summary>
    /// Immersive water audio controller that manages underwater audio muffling
    /// (via AudioLowPassFilter), surface/underwater ambient loop crossfading,
    /// and transition splashes when camera submerges or surfaces.
    /// </summary>
    [AddComponentMenu("Original Water/Water Audio Controller")]
    public class WaterAudioController : MonoBehaviour
    {
        [Header("Audio Clips")]
        [Tooltip("Ambient water laps playing while above water.")]
        [SerializeField] private AudioClip surfaceAmbientClip;

        [Tooltip("Deep muffled ambiance playing while underwater.")]
        [SerializeField] private AudioClip underwaterAmbientClip;

        [Tooltip("Sound played when the camera plunges into the water.")]
        [SerializeField] private AudioClip enterWaterSplashClip;

        [Tooltip("Sound played when the camera surfaces out of the water.")]
        [SerializeField] private AudioClip exitWaterSplashClip;

        [Header("Volumes & Crossfade")]
        [SerializeField] [Range(0f, 1f)] private float surfaceVolume = 0.6f;
        [SerializeField] [Range(0f, 1f)] private float underwaterVolume = 0.8f;
        [SerializeField] private float fadeSpeed = 3.5f;

        [Header("Low-Pass Filter Settings")]
        [Tooltip("Frequency when fully above water (all sounds crisp).")]
        [SerializeField] private float airCutoffFrequency = 22000f;

        [Tooltip("Frequency when submerged (muffled underwater sound).")]
        [SerializeField] private float underwaterCutoffFrequency = 650f;

        [Tooltip("Speed of low-pass transition.")]
        [SerializeField] private float filterLerpSpeed = 6.0f;

        [Header("Water Reference")]
        [SerializeField] private float waterLevel = 70.0f;
        [SerializeField] private Transform targetCamera;

        private AudioSource _surfaceSource;
        private AudioSource _underwaterSource;
        private AudioSource _sfxSource;
        private AudioLowPassFilter _filter;
        private bool _isCameraUnderwater;

        private void Start()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            SetupAudioSources();
            SetupLowPassFilter();
        }

        private void SetupAudioSources()
        {
            _surfaceSource = gameObject.AddComponent<AudioSource>();
            _surfaceSource.loop = true;
            _surfaceSource.clip = surfaceAmbientClip;
            _surfaceSource.volume = surfaceVolume;
            _surfaceSource.spatialBlend = 0f; // 2D ambient
            if (surfaceAmbientClip != null) _surfaceSource.Play();

            _underwaterSource = gameObject.AddComponent<AudioSource>();
            _underwaterSource.loop = true;
            _underwaterSource.clip = underwaterAmbientClip;
            _underwaterSource.volume = 0f;
            _underwaterSource.spatialBlend = 0f;
            if (underwaterAmbientClip != null) _underwaterSource.Play();

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.spatialBlend = 0f;
        }

        private void SetupLowPassFilter()
        {
            if (targetCamera == null) return;

            _filter = targetCamera.GetComponent<AudioLowPassFilter>();
            if (_filter == null)
            {
                _filter = targetCamera.gameObject.AddComponent<AudioLowPassFilter>();
            }
            _filter.cutoffFrequency = airCutoffFrequency;
        }

        private void Update()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
                if (targetCamera == null) return;
            }

            float currentWaterY = (SurfaceWaterController.Instance != null)
                ? SurfaceWaterController.Instance.GetWaterHeightAt(targetCamera.position)
                : waterLevel;

            bool nowUnderwater = targetCamera.position.y < currentWaterY;

            // Transition detection
            if (nowUnderwater != _isCameraUnderwater)
            {
                _isCameraUnderwater = nowUnderwater;
                if (_isCameraUnderwater)
                {
                    PlaySFX(enterWaterSplashClip);
                }
                else
                {
                    PlaySFX(exitWaterSplashClip);
                }
            }

            // Smooth crossfade between surface & underwater ambient
            float targetSurfaceVol = _isCameraUnderwater ? 0f : surfaceVolume;
            float targetUnderwaterVol = _isCameraUnderwater ? underwaterVolume : 0f;

            if (_surfaceSource != null)
                _surfaceSource.volume = Mathf.MoveTowards(_surfaceSource.volume, targetSurfaceVol, fadeSpeed * Time.deltaTime);

            if (_underwaterSource != null)
                _underwaterSource.volume = Mathf.MoveTowards(_underwaterSource.volume, targetUnderwaterVol, fadeSpeed * Time.deltaTime);

            // Filter interpolation
            if (_filter != null)
            {
                float targetFreq = _isCameraUnderwater ? underwaterCutoffFrequency : airCutoffFrequency;
                _filter.cutoffFrequency = Mathf.Lerp(_filter.cutoffFrequency, targetFreq, Time.deltaTime * filterLerpSpeed);
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            if (clip != null && _sfxSource != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }
    }
}

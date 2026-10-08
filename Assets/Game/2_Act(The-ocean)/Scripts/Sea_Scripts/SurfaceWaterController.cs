using UnityEngine;

namespace OriginalWater
{
    /// <summary>
    /// Drives a simple stylized water surface: animates the material's
    /// scrolling normal map offsets and (optionally) applies a gentle
    /// vertical bob to simulate swell. Designed to be lightweight and
    /// self-contained so it can run on a flat plane mesh with the
    /// companion "OriginalWater/SurfaceFX" shader.
    /// </summary>
    [AddComponentMenu("Original Water/Surface Water Controller")]
    [DisallowMultipleComponent]
    public class SurfaceWaterController : MonoBehaviour
    {
        [Header("Material")]
        [Tooltip("Renderer whose material will be animated. Defaults to the Renderer on this GameObject.")]
        [SerializeField] private Renderer targetRenderer;

        [Header("Flow")]
        [Tooltip("Direction and speed the primary normal layer scrolls, in UV units per second.")]
        [SerializeField] private Vector2 primaryFlow = new Vector2(0.05f, 0.03f);

        [Tooltip("Direction and speed the secondary normal layer scrolls, in UV units per second.")]
        [SerializeField] private Vector2 secondaryFlow = new Vector2(-0.04f, 0.02f);

        [Header("Swell")]
        [Tooltip("Enable a small sinusoidal vertical bob on the transform (for non-baked, simple setups).")]
        [SerializeField] private bool applySwell = false;
        [SerializeField] private float swellAmplitude = 0.05f;
        [SerializeField] private float swellFrequency = 0.5f;

        private static readonly int PrimaryOffsetId = Shader.PropertyToID("_PrimaryFlowOffset");
        private static readonly int SecondaryOffsetId = Shader.PropertyToID("_SecondaryFlowOffset");

        private MaterialPropertyBlock _block;
        private Vector3 _startLocalPos;
        private float _clock;

        public static SurfaceWaterController Instance { get; private set; }
        public float CurrentWaterLevel => transform.position.y;

        public float GetWaterHeightAt(Vector3 worldPos)
        {
            return transform.position.y;
        }

        private void EnsureInitialized()
        {
            if (Instance == null) Instance = this;
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
            if (_block == null)
            {
                _block = new MaterialPropertyBlock();
            }
        }

        private void Awake()
        {
            Instance = this;
            EnsureInitialized();
            _startLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            EnsureInitialized();
        }

        private void Update()
        {
            EnsureInitialized();
            _clock += Time.deltaTime;
            if (_clock > 10000f)
            {
                _clock %= 1000f;
            }

            if (targetRenderer != null)
            {
                Vector2 primaryOffset = primaryFlow * _clock;
                Vector2 secondaryOffset = secondaryFlow * _clock;

                targetRenderer.GetPropertyBlock(_block);
                _block.SetVector(PrimaryOffsetId, new Vector4(primaryOffset.x, primaryOffset.y, 0f, 0f));
                _block.SetVector(SecondaryOffsetId, new Vector4(secondaryOffset.x, secondaryOffset.y, 0f, 0f));
                targetRenderer.SetPropertyBlock(_block);
            }

            if (applySwell)
            {
                float bob = Mathf.Sin(_clock * swellFrequency * Mathf.PI * 2f) * swellAmplitude;
                Vector3 pos = _startLocalPos;
                pos.y += bob;
                transform.localPosition = pos;
            }
        }

        private void OnValidate()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponent<Renderer>();
            }
        }
    }
}

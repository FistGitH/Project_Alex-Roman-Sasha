using UnityEngine;
using System.Collections.Generic;

namespace OriginalWater
{
    /// <summary>
    /// Realistic multi-point buoyancy and hydrodynamic drag simulation.
    /// Supports dynamic water height, Archimedes upward force, torque stabilization,
    /// and entry splash notifications.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Original Water/Water Buoyancy")]
    public class WaterBuoyancy : MonoBehaviour
    {
        [Header("Buoyancy Settings")]
        [Tooltip("Multiplies upward buoyant force relative to gravity. 1.0 = neutral buoyancy, >1.0 = floats high.")]
        [SerializeField] private float buoyancyMultiplier = 1.25f;

        [Tooltip("Submersion depth (in meters) at which full buoyant force is achieved.")]
        [SerializeField] private float immersionDepth = 1.0f;

        [Tooltip("Manual water surface Y fallback if no SurfaceWaterController is found.")]
        [SerializeField] private float defaultWaterLevel = 70.0f;

        [Header("Hydrodynamics")]
        [Tooltip("Linear damping applied when submerged to simulate water resistance.")]
        [SerializeField] private float waterLinearDrag = 2.5f;

        [Tooltip("Angular damping applied when submerged to prevent wild spinning.")]
        [SerializeField] private float waterAngularDrag = 2.0f;

        [Header("Float Points")]
        [Tooltip("Optional custom sample points. If empty, 4 balanced corner points are generated from Collider bounds.")]
        [SerializeField] private Transform[] customFloatPoints;

        [Header("Splash & Audio")]
        [Tooltip("Minimum downward speed required to trigger a water splash on impact.")]
        [SerializeField] private float minSplashVelocity = 2.0f;

        private Rigidbody _rb;
        private Collider _collider;
        private List<Vector3> _autoFloatOffsets = new List<Vector3>();
        private bool _wasUnderwater;

        public bool IsUnderwater { get; private set; }
        public float SubmersionFraction { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<Collider>();
            InitializeFloatPoints();
        }

        private void InitializeFloatPoints()
        {
            if (customFloatPoints != null && customFloatPoints.Length > 0)
            {
                return;
            }

            _autoFloatOffsets.Clear();
            if (_collider != null)
            {
                Bounds b = _collider.bounds;
                Vector3 ext = b.extents * 0.75f;
                // 4 corners on local horizontal plane at the bottom-half
                _autoFloatOffsets.Add(new Vector3(ext.x, -ext.y * 0.4f, ext.z));
                _autoFloatOffsets.Add(new Vector3(-ext.x, -ext.y * 0.4f, ext.z));
                _autoFloatOffsets.Add(new Vector3(ext.x, -ext.y * 0.4f, -ext.z));
                _autoFloatOffsets.Add(new Vector3(-ext.x, -ext.y * 0.4f, -ext.z));
            }
            else
            {
                // Fallback to center point
                _autoFloatOffsets.Add(Vector3.zero);
            }
        }

        private void FixedUpdate()
        {
            int pointCount = (customFloatPoints != null && customFloatPoints.Length > 0)
                ? customFloatPoints.Length
                : _autoFloatOffsets.Count;

            if (pointCount == 0) return;

            int submergedCount = 0;
            float totalSubmersion = 0f;

            for (int i = 0; i < pointCount; i++)
            {
                Vector3 worldPoint = (customFloatPoints != null && customFloatPoints.Length > 0)
                    ? customFloatPoints[i].position
                    : transform.TransformPoint(_autoFloatOffsets[i]);

                float waterY = GetWaterHeight(worldPoint);
                float depth = waterY - worldPoint.y;

                if (depth > 0f)
                {
                    submergedCount++;
                    float factor = Mathf.Clamp01(depth / immersionDepth);
                    totalSubmersion += factor;

                    // Archimedes buoyant force distributed across sample points
                    Vector3 buoyantForce = -Physics.gravity * (buoyancyMultiplier * factor / pointCount);
                    _rb.AddForceAtPosition(buoyantForce, worldPoint, ForceMode.Acceleration);
                }
            }

            SubmersionFraction = totalSubmersion / pointCount;
            IsUnderwater = submergedCount > 0;

            // Apply hydrodynamic drag when submerged
            if (IsUnderwater)
            {
                float dragFactor = Mathf.Clamp01(SubmersionFraction);
#if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity *= Mathf.Clamp01(1f - (waterLinearDrag * dragFactor * Time.fixedDeltaTime));
                _rb.angularVelocity *= Mathf.Clamp01(1f - (waterAngularDrag * dragFactor * Time.fixedDeltaTime));
#else
                _rb.velocity *= Mathf.Clamp01(1f - (waterLinearDrag * dragFactor * Time.fixedDeltaTime));
                _rb.angularVelocity *= Mathf.Clamp01(1f - (waterAngularDrag * dragFactor * Time.fixedDeltaTime));
#endif
            }

            // Detect splash entry
            HandleSplashTransition();
        }

        private void HandleSplashTransition()
        {
            if (!_wasUnderwater && IsUnderwater)
            {
                // Transitioning from air to water
#if UNITY_6000_0_OR_NEWER
                float speed = Mathf.Abs(_rb.linearVelocity.y);
#else
                float speed = Mathf.Abs(_rb.velocity.y);
#endif
                if (speed >= minSplashVelocity)
                {
                    Vector3 splashPos = transform.position;
                    splashPos.y = GetWaterHeight(splashPos);
                    WaterInteractionManager.TriggerSplash(splashPos, speed);
                }
            }
            _wasUnderwater = IsUnderwater;
        }

        public float GetWaterHeight(Vector3 position)
        {
            if (SurfaceWaterController.Instance != null)
            {
                return SurfaceWaterController.Instance.GetWaterHeightAt(position);
            }
            return defaultWaterLevel;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (customFloatPoints != null && customFloatPoints.Length > 0)
            {
                foreach (var pt in customFloatPoints)
                {
                    if (pt != null) Gizmos.DrawWireSphere(pt.position, 0.15f);
                }
            }
            else if (_autoFloatOffsets != null)
            {
                foreach (var offset in _autoFloatOffsets)
                {
                    Gizmos.DrawWireSphere(transform.TransformPoint(offset), 0.15f);
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OriginalWater
{
    /// <summary>Configures the main camera for the water prefab's URP volumes and depth effects.</summary>
    [AddComponentMenu("Original Water/Water Camera Setup")]
    public class WaterCameraSetup : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        private Camera _configuredCamera;

        private void Update()
        {
            Camera camera = targetCamera != null ? targetCamera : Camera.main;
            if (camera == null || camera == _configuredCamera) return;
            UniversalAdditionalCameraData data = camera.GetUniversalAdditionalCameraData();
            data.requiresDepthTexture = true;
            data.renderPostProcessing = true;
            int mask = data.volumeLayerMask.value;
            foreach (Volume volume in GetComponentsInChildren<Volume>(true))
                mask |= 1 << volume.gameObject.layer;
            data.volumeLayerMask = mask;
            _configuredCamera = camera;
        }
    }
}

using UnityEngine;
using UnityEngine.Rendering;

namespace OriginalWater
{
    /// <summary>Depth-projected caustics for URP, replacing the Built-in Projector.</summary>
    [AddComponentMenu("Original Water/Water Caustics Volume")]
    public class WaterCausticsVolume : MonoBehaviour
    {
        [SerializeField] private Material causticsMaterial;
        [SerializeField] private Vector3 volumeSize = new Vector3(320f, 85f, 320f);
        private GameObject _volume;
        private Renderer _renderer;
        private MaterialPropertyBlock _properties;

        private void OnEnable()
        {
            if (causticsMaterial == null) return;
            _volume = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _volume.name = "URP Caustics Volume";
            _volume.layer = gameObject.layer;
            _volume.transform.SetParent(transform.parent, false);
            _volume.GetComponent<Collider>().enabled = false;
            Destroy(_volume.GetComponent<Collider>());
            _renderer = _volume.GetComponent<Renderer>();
            _renderer.sharedMaterial = causticsMaterial;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _properties = new MaterialPropertyBlock();
            LateUpdate();
        }

        private void LateUpdate()
        {
            if (_volume == null) return;
            float waterY = transform.parent != null ? transform.parent.position.y : transform.position.y;
            Vector3 scale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
            Vector3 size = Vector3.Scale(volumeSize, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            _volume.transform.SetPositionAndRotation(new Vector3(transform.position.x,
                waterY - size.y * 0.5f, transform.position.z), Quaternion.identity);
            _volume.transform.localScale = Vector3.one;
            Vector3 inherited = _volume.transform.lossyScale;
            _volume.transform.localScale = new Vector3(size.x / Mathf.Max(Mathf.Abs(inherited.x), 0.001f),
                size.y / Mathf.Max(Mathf.Abs(inherited.y), 0.001f),
                size.z / Mathf.Max(Mathf.Abs(inherited.z), 0.001f));
            _properties.SetFloat("_WaterHeight", waterY);
            _renderer.SetPropertyBlock(_properties);
        }

        private void OnDisable()
        {
            if (_volume != null) { _volume.SetActive(false); Destroy(_volume); }
            _volume = null;
        }
    }
}

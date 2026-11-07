using System;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public sealed class CameraInterferenceEffect : MonoBehaviour
{
    [SerializeField] private Shader interferenceShader;

    private static readonly int StrengthId = Shader.PropertyToID("_Strength");

    private Camera targetCamera;
    private GameObject overlay;
    private Mesh overlayMesh;
    private MeshRenderer overlayRenderer;
    private Material overlayMaterial;
    private float strength;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
        {
            throw new InvalidOperationException("CameraInterferenceEffect must be attached to a GameObject with a Camera component.");
        }

        if (interferenceShader == null)
        {
            throw new InvalidOperationException("CameraInterferenceEffect requires an interference shader.");
        }

        CreateOverlay();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

        if (overlayRenderer != null)
        {
            overlayRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(overlayMaterial);
        Destroy(overlayMesh);
        Destroy(overlay);
    }

    public void EnableInterference()
    {
        SetInterferenceStrength(1f);
    }

    public void DisableInterference()
    {
        SetInterferenceStrength(0f);
    }

    public void SetInterferenceStrength(float value)
    {
        strength = Mathf.Clamp01(value);
    }

    private void CreateOverlay()
    {
        overlay = new GameObject("Camera Interference Overlay", typeof(MeshFilter), typeof(MeshRenderer));
        overlay.layer = gameObject.layer;
        overlay.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSave;
        overlay.transform.SetParent(targetCamera.transform, false);

        overlayMesh = CreateQuadMesh();
        overlay.GetComponent<MeshFilter>().sharedMesh = overlayMesh;

        overlayMaterial = new Material(interferenceShader)
        {
            name = "Camera Interference Material",
            hideFlags = HideFlags.DontSave
        };

        overlayRenderer = overlay.GetComponent<MeshRenderer>();
        overlayRenderer.sharedMaterial = overlayMaterial;
        overlayRenderer.enabled = false;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != targetCamera)
        {
            return;
        }

        UpdateOverlayTransform();
        overlayMaterial.SetFloat(StrengthId, strength);
        overlayRenderer.enabled = strength > 0f;
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera == targetCamera)
        {
            overlayRenderer.enabled = false;
        }
    }

    private void UpdateOverlayTransform()
    {
        float distance = targetCamera.nearClipPlane + 0.01f;
        float height = 2f * distance * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float width = height * targetCamera.aspect;

        overlay.transform.localPosition = Vector3.forward * distance;
        overlay.transform.localRotation = Quaternion.identity;
        overlay.transform.localScale = new Vector3(width, height, 1f) * 1.01f;
    }

    private static Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh
        {
            name = "Camera Interference Quad",
            hideFlags = HideFlags.DontSave,
            vertices = new[]
            {
                new Vector3(-0.5f, -0.5f),
                new Vector3(0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f),
                new Vector3(0.5f, 0.5f)
            },
            uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            },
            triangles = new[] { 0, 2, 1, 2, 3, 1 }
        };

        mesh.RecalculateBounds();
        mesh.UploadMeshData(true);

        return mesh;
    }
}
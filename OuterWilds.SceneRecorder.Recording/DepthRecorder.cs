using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

public sealed class DepthRecorder : RenderTextureRecorder
{
    public const string CameraGameObjectName = "Depth Camera";

    public int Width { get; set; } = 1920;

    public int Height { get; set; } = 1080;

    private OWCamera _OWCamera = null!;

    private RenderTexture _DepthRenderTexture = null!;

    private RenderTexture _ColorRenderTexture = null!;

    public DepthRecorder()
    {
        Awoken += OnAwoken;
        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
        BeforeFrameRecorded += OnBeforeFrameRecorded;
    }

    private void OnAwoken()
    {
        var playerCamera = Locator.GetPlayerCamera();

        var cameraParent = new GameObject(CameraGameObjectName);
        cameraParent.transform.parent = transform;
        cameraParent.transform.localPosition = Vector3.zero;
        cameraParent.transform.localRotation = Quaternion.identity;
        cameraParent.transform.localScale = Vector3.one;

        _OWCamera = playerCamera.CopyTo(cameraParent, copyPostProcessing: false);

        _OWCamera.renderSkybox = false;
        _OWCamera.useGUILayout = false;
        _OWCamera.useViewmodels = false;
        _OWCamera.targetTexture = null;

        _OWCamera.mainCamera.eventMask = 0;

        _OWCamera.enabled = false;

        _OWCamera.mainCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    protected override RenderTexture ProvideSourceRenderTexture()
    {
        _DepthRenderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.Depth);
        _ColorRenderTexture = new RenderTexture(Width, Height, 16);

        _OWCamera.targetTexture = _DepthRenderTexture;

        return _ColorRenderTexture;
    }

    private void OnRecordingStarted()
    {
        const float minFarClipPlane = 100f;
        var playerTransform = Locator.GetPlayerBody().transform;
        var distanceToPlayer = (_OWCamera.transform.position - playerTransform.position).magnitude;
        _OWCamera.farClipPlane = Math.Max(minFarClipPlane, distanceToPlayer * 2);

        _OWCamera.enabled = true;
    }

    private void OnBeforeFrameRecorded()
    {
        Graphics.Blit(_DepthRenderTexture, _ColorRenderTexture);
    }

    private void OnRecordingFinished()
    {
        _OWCamera.enabled = false;
    }

    private void OnDestroy()
    {
        DestroyImmediate(_DepthRenderTexture);
        DestroyImmediate(_ColorRenderTexture);
        DestroyImmediate(_OWCamera.gameObject);
        _OWCamera = null!;
    }
}

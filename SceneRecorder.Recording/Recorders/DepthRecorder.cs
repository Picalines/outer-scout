using Picalines.OuterWilds.SceneRecorder.Recording.Extensions;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

internal sealed class DepthRecorder : RenderTextureRecorder
{
    public int Width { get; set; } = 1920;

    public int Height { get; set; } = 1080;

    public OWCamera DepthCamera { get; private set; } = null!;

    private RenderTexture _DepthRenderTexture = null!;

    private RenderTexture _ColorRenderTexture = null!;

    public DepthRecorder()
    {
        Awoken += OnAwoken;
        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
        FrameEnded += OnFrameEnded;
    }

    private void OnAwoken()
    {
        var playerCamera = Locator.GetPlayerCamera();

        var cameraParent = new GameObject($"{nameof(SceneRecorder)} Depth Camera");
        cameraParent.transform.parent = transform;
        cameraParent.transform.localPosition = Vector3.zero;
        cameraParent.transform.localRotation = Quaternion.identity;
        cameraParent.transform.localScale = Vector3.one;

        DepthCamera = playerCamera.CopyTo(cameraParent, copyPostProcessing: false);

        DepthCamera.renderSkybox = false;
        DepthCamera.useGUILayout = false;
        DepthCamera.useViewmodels = false;
        DepthCamera.targetTexture = null;

        DepthCamera.mainCamera.eventMask = 0;
        DepthCamera.mainCamera.forceIntoRenderTexture = true;

        DepthCamera.enabled = false;

        DepthCamera.mainCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    protected override RenderTexture ProvideSourceRenderTexture()
    {
        _DepthRenderTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.Depth);
        _ColorRenderTexture = new RenderTexture(Width, Height, 16);

        DepthCamera.targetTexture = _DepthRenderTexture;

        return _ColorRenderTexture;
    }

    private void OnRecordingStarted()
    {
        DepthCamera.enabled = true;
    }

    private void OnFrameEnded()
    {
        Graphics.Blit(_DepthRenderTexture, _ColorRenderTexture);
    }

    private void OnRecordingFinished()
    {
        DepthCamera.enabled = false;
    }

    private void OnDestroy()
    {
        DestroyImmediate(_DepthRenderTexture);
        DestroyImmediate(_ColorRenderTexture);
        DestroyImmediate(DepthCamera.gameObject);
        DepthCamera = null!;
    }
}

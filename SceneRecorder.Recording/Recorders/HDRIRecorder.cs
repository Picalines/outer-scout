using Picalines.OuterWilds.SceneRecorder.Recording.Extensions;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using UnityEngine;
using UnityEngine.Rendering;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

internal sealed class HdriRecorder : RenderTextureRecorder
{
    public int CubemapFaceSize { get; set; } = 2048;

    private RenderTexture? _CubemapFrameTexture = null;

    private readonly OWCamera[] _OWCameras = new OWCamera[6];

    private RenderTexture _SourceRenderTexture = null!;

    private RenderTexture _FlippedFrameSourceTexture = null!;

    private static readonly Quaternion[] _CameraRotations = new Quaternion[6]
    {
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, -90, 0),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90, 0, 0),
        Quaternion.identity,
        Quaternion.Euler(0, 180, 0),
    };

    public HdriRecorder()
    {
        Awoken += OnAwoken;
        RecordingStarted += OnRecordingStarted;
        FrameEnded += OnFrameEnded;
        RecordingFinished += OnRecordingFinished;
    }

    private void OnAwoken()
    {
        var playerCamera = Locator.GetPlayerCamera();

        for (int i = 0; i < _OWCameras.Length; i++)
        {
            var cameraParent = new GameObject($"HDRI Camera #{i + 1}");
            cameraParent.transform.parent = transform;
            cameraParent.transform.localPosition = Vector3.zero;
            cameraParent.transform.localRotation = _CameraRotations[i];
            cameraParent.transform.localScale = Vector3.one;

            var owCamera = playerCamera.CopyTo(cameraParent);

            owCamera.useGUILayout = false;
            owCamera.useViewmodels = false;
            owCamera.mainCamera.depthTextureMode = DepthTextureMode.None;

            owCamera.mainCamera.eventMask = 0;
            owCamera.mainCamera.forceIntoRenderTexture = true;

            owCamera.aspect = 1;
            owCamera.fieldOfView = 90;

            owCamera.enabled = false;
            _OWCameras[i] = owCamera;
        }
    }

    protected override RenderTexture ProvideSourceRenderTexture()
    {
        _SourceRenderTexture = new RenderTexture(CubemapFaceSize, CubemapFaceSize / 2, 16);

        _FlippedFrameSourceTexture = new RenderTexture(_SourceRenderTexture);

        _CubemapFrameTexture = new RenderTexture(CubemapFaceSize, CubemapFaceSize, 16)
        {
            dimension = TextureDimension.Cube,
        };

        return _SourceRenderTexture;
    }

    private void OnRecordingStarted()
    {
        for (int i = 0; i < _OWCameras.Length; i++)
        {
            var owCamera = _OWCameras[i];

            if (owCamera.targetTexture == null)
            {
                owCamera.targetTexture = new RenderTexture(CubemapFaceSize, CubemapFaceSize, 16);

                var commandBuffer = new CommandBuffer();
                commandBuffer.CopyTexture(owCamera.targetTexture, 0, _CubemapFrameTexture, i);

                owCamera.mainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
            }

            owCamera.enabled = true;
        }
    }

    private void OnFrameEnded()
    {
        _CubemapFrameTexture!.ConvertToEquirect(_FlippedFrameSourceTexture, Camera.MonoOrStereoscopicEye.Mono);

        Graphics.Blit(_FlippedFrameSourceTexture, _SourceRenderTexture, new Vector2(1, -1), new Vector2(0, 1));
    }

    private void OnRecordingFinished()
    {
        foreach (var owCamera in _OWCameras)
        {
            owCamera.enabled = false;
        }
    }

    private void OnDestroy()
    {
        foreach (var owCamera in _OWCameras)
        {
            DestroyImmediate(owCamera.targetTexture);
            Destroy(owCamera.gameObject);
        }

        Destroy(_CubemapFrameTexture);
        Destroy(_FlippedFrameSourceTexture);
        Destroy(_SourceRenderTexture);
    }
}

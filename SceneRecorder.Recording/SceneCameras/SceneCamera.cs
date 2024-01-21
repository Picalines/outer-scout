using SceneRecorder.Recording.Domain;
using SceneRecorder.Recording.Extensions;
using SceneRecorder.Shared.DependencyInjection;
using SceneRecorder.Shared.DTOs;
using SceneRecorder.Shared.Validation;
using UnityEngine;

namespace SceneRecorder.Recording.SceneCameras;

[RequireComponent(typeof(OWCamera))]
internal sealed class SceneCamera : InitializedBehaviour<SceneCamera.Parameters>, ISceneCamera
{
    public record struct Parameters
    {
        public required SceneSettings SceneSettings { get; init; }

        public required string Id { get; init; }

        public required Vector2Int Resolution { get; init; }
    }

    public string Id { get; }

    public Transform Transform { get; private set; } = null!;

    private SceneSettings _sceneSettings;

    private OWCamera _colorCamera = null!;
    private OWCamera _depthCamera = null!;

    private RenderTexture _colorTexture = null!;
    private RenderTexture _depthTexture = null!;

    private SceneCamera()
        : base(out var parameters)
    {
        parameters.Id.Throw().IfNullOrWhiteSpace();
        parameters.Resolution.x.Throw().IfLessThan(1);
        parameters.Resolution.y.Throw().IfLessThan(1);

        Id = parameters.Id;
        _sceneSettings = parameters.SceneSettings;
        var resolution = parameters.Resolution;

        _colorTexture = new RenderTexture(
            resolution.x,
            resolution.y,
            0,
            RenderTextureFormat.ARGB32 // respect FFmpegTextureRecorder pixel format!
        );

        _depthTexture = new RenderTexture(
            resolution.x,
            resolution.y,
            32, // Unity supports 16, 24 or 32
            RenderTextureFormat.Depth
        );

        _colorCamera.targetTexture = _colorTexture;

        _depthCamera = CreateDepthCamera();
        _depthCamera.targetTexture = _depthTexture;
    }

    private void Awake()
    {
        name = $"{nameof(SceneRecorder)} camera {Id}";

        _colorCamera = GetComponent<OWCamera>();
        Transform = transform;
    }

    public CameraInfoDTO CameraInfo
    {
        get => CameraInfoDTO.FromOWCamera(_colorCamera);
        set
        {
            value.Apply(_colorCamera);
            value.Apply(_depthCamera);
        }
    }

    public RenderTexture? ColorTexture => _colorTexture;

    public RenderTexture? DepthTexture => _depthTexture;

    private OWCamera CreateDepthCamera()
    {
        var depthCameraObject = new GameObject()
        {
            name = $"{nameof(SceneRecorder)} depth camera {Id}",
        };

        var depthTransform = depthCameraObject.transform;

        depthTransform.parent = Transform;
        depthTransform.localPosition = Vector3.zero;
        depthTransform.localRotation = Quaternion.identity;
        depthTransform.localScale = Vector3.one;

        var depthCamera = _colorCamera.CopyTo(depthCameraObject, copyPostProcessing: false);

        depthCamera.renderSkybox = false;
        depthCamera.useGUILayout = false;
        depthCamera.useViewmodels = false;
        depthCamera.targetTexture = null;

        depthCamera.mainCamera.eventMask = 0;
        depthCamera.mainCamera.forceIntoRenderTexture = true;

        return depthCamera;
    }

    private void OnDestory()
    {
        Destroy(_colorTexture);
        Destroy(_depthTexture);
        Destroy(_depthCamera.gameObject);
    }
}

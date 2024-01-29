using SceneRecorder.Application.Extensions;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Application.SceneCameras;

[RequireComponent(typeof(OWCamera))]
public sealed class SceneCamera : InitializedBehaviour<SceneCamera.Parameters>, ISceneCamera
{
    public record struct Parameters
    {
        public required string Id { get; init; }

        public required Vector2Int Resolution { get; init; }
    }

    public string Id { get; }

    public Transform Transform { get; private set; } = null!;

    private CameraInfo _cameraInfo;

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

        _colorCamera.mainCamera.usePhysicalProperties = true;
        _colorCamera.targetTexture = _colorTexture;

        _cameraInfo = _colorCamera.GetCameraInfo();
    }

    private void Awake()
    {
        name = $"{nameof(SceneRecorder)} camera {Id}";

        _colorCamera = GetComponent<OWCamera>();
        Transform = transform;

        _depthCamera = CreateDepthCamera();
        _depthCamera.targetTexture = _depthTexture;
    }

    public RenderTexture? ColorTexture => _colorTexture;

    public RenderTexture? DepthTexture => _depthTexture;

    public CameraInfo CameraInfo
    {
        get => _cameraInfo;
        set
        {
            _cameraInfo = value;
            _colorCamera.ApplyCameraInfo(value);
            _depthCamera.ApplyCameraInfo(value);
        }
    }

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

        depthCamera.mainCamera.usePhysicalProperties = true;
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
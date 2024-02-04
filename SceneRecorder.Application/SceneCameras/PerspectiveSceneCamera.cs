using SceneRecorder.Application.Extensions;
using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;

namespace SceneRecorder.Application.SceneCameras;

[RequireComponent(typeof(OWCamera))]
public sealed class PerspectiveSceneCamera
    : InitializedBehaviour<PerspectiveSceneCamera.Parameters>,
        ISceneCamera
{
    public record struct Parameters
    {
        public required Vector2Int Resolution { get; init; }

        public required Camera.GateFitMode GateFit { get; init; }

        public required CameraPerspective Perspective { get; init; }
    }

    public Transform Transform { get; private set; } = null!;

    private readonly Camera.GateFitMode _gateFit;
    private CameraPerspective _perspective;

    private OWCamera _colorCamera = null!;
    private OWCamera _depthCamera = null!;

    private RenderTexture _colorTexture = null!;
    private RenderTexture _depthTexture = null!;

    private bool _disposed = false;

    private PerspectiveSceneCamera()
        : base(out var parameters)
    {
        parameters.Resolution.x.Throw().IfLessThan(1);
        parameters.Resolution.y.Throw().IfLessThan(1);

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

        _gateFit = parameters.GateFit;
        Perspective = parameters.Perspective;
    }

    private void Awake()
    {
        _colorCamera = GetComponent<OWCamera>();

        _colorCamera.mainCamera.usePhysicalProperties = true;
        _colorCamera.mainCamera.gateFit = _gateFit;
        _colorCamera.targetTexture = _colorTexture;

        Transform = transform;

        _depthCamera = CreateDepthCamera();
        _depthCamera.targetTexture = _depthTexture;
    }

    public RenderTexture? ColorTexture => _colorTexture;

    public RenderTexture? DepthTexture => _depthTexture;

    public CameraPerspective Perspective
    {
        get => _perspective;
        set
        {
            _perspective = value;
            _colorCamera.ApplyPerspective(value);
            _depthCamera.ApplyPerspective(value);
        }
    }

    public static PerspectiveSceneCamera? Create(Parameters parameters)
    {
        var playerCamera = Locator.GetPlayerCamera().OrNull();
        if (playerCamera is null)
        {
            return null;
        }

        var gameObject = new GameObject(
            $"{nameof(SceneRecorder)}.{nameof(PerspectiveSceneCamera)}"
        );

        playerCamera.CopyTo(gameObject, copyPostProcessing: true);

        return gameObject.AddComponent<PerspectiveSceneCamera, Parameters>(parameters);
    }

    private OWCamera CreateDepthCamera()
    {
        var depthCameraObject = new GameObject(
            $"{nameof(SceneRecorder)}.{nameof(PerspectiveSceneCamera)}.depth"
        );

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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        Destroy(gameObject);
    }
}

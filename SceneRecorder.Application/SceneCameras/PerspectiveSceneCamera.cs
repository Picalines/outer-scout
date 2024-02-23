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

    private readonly Camera.GateFitMode _gateFit;
    private CameraPerspective _perspective;

    private readonly RenderTexture _colorTexture;
    private readonly RenderTexture _depthTexture;
    private readonly RenderTexture _colorDepthTexture;

    private OWCamera? _colorCamera;
    private OWCamera? _depthCamera;

    private bool _disposed = false;

    private PerspectiveSceneCamera()
        : base(out var parameters)
    {
        var resolution = parameters.Resolution;

        _colorTexture = new RenderTexture(
            resolution.x,
            resolution.y,
            0,
            RenderTextureFormat.ARGB32
        );

        _depthTexture = new RenderTexture(
            resolution.x,
            resolution.y,
            32,
            RenderTextureFormat.Depth
        );

        _colorDepthTexture = new RenderTexture(
            resolution.x,
            resolution.y,
            0,
            RenderTextureFormat.ARGB32
        );

        _gateFit = parameters.GateFit;
        _perspective = parameters.Perspective;
    }

    private void Awake()
    {
        _colorCamera = GetComponent<OWCamera>();

        _colorCamera.mainCamera.forceIntoRenderTexture = true;
        _colorCamera.mainCamera.usePhysicalProperties = true;
        _colorCamera.mainCamera.gateFit = _gateFit;
        _colorCamera.targetTexture = _colorTexture;

        _depthCamera = CreateDepthCamera();

        Perspective = _perspective; // apply params to both cameras
    }

    public Transform Transform
    {
        get
        {
            AssertNotDisposed();
            return transform;
        }
    }

    public RenderTexture? ColorTexture
    {
        get
        {
            AssertNotDisposed();
            return _colorTexture;
        }
    }

    public RenderTexture? DepthTexture
    {
        get
        {
            AssertNotDisposed();

            return _depthCamera is not null ? _colorDepthTexture : null;
        }
    }

    public CameraPerspective Perspective
    {
        get
        {
            AssertNotDisposed();
            return _perspective;
        }
        set
        {
            AssertNotDisposed();

            _perspective = value;
            _colorCamera?.ApplyPerspective(value);
            _depthCamera?.ApplyPerspective(value);
        }
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

    public static PerspectiveSceneCamera? Create(Parameters parameters)
    {
        parameters.Resolution.x.Throw().IfLessThan(1);
        parameters.Resolution.y.Throw().IfLessThan(1);

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

    private void Update()
    {
        if (_depthCamera is not null)
        {
            // It's impossible move depth bits to colorBuffer without a shader
            // so we blit Depth texture to RGBA32.
            Graphics.Blit(_depthTexture, _colorDepthTexture);
        }
    }

    private OWCamera? CreateDepthCamera()
    {
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            return null;
        }

        _colorCamera.ThrowIfNull();

        var depthCameraObject = new GameObject(
            $"{nameof(SceneRecorder)}.{nameof(PerspectiveSceneCamera)}.depth"
        );

        var depthTransform = depthCameraObject.transform;
        depthTransform.parent = Transform;
        depthTransform.ResetLocal();

        var depthCamera = _colorCamera.CopyTo(depthCameraObject, copyPostProcessing: false);

        depthCamera.renderSkybox = false;
        depthCamera.useGUILayout = false;
        depthCamera.useViewmodels = false;
        depthCamera.targetTexture = null;

        depthCamera.mainCamera.depthTextureMode = DepthTextureMode.Depth;
        depthCamera.mainCamera.usePhysicalProperties = true;
        depthCamera.mainCamera.eventMask = 0;
        depthCamera.mainCamera.forceIntoRenderTexture = true;

        depthCamera.targetTexture = _depthTexture;

        return depthCamera;
    }

    private void AssertNotDisposed()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(PerspectiveSceneCamera)} is disposed");
        }
    }

    private void OnDestory()
    {
        Destroy(_colorTexture);
        Destroy(_depthTexture);
        Destroy(_colorDepthTexture);
        Destroy(_depthCamera?.gameObject);
    }
}

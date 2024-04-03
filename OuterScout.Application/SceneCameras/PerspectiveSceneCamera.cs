using OuterScout.Application.Extensions;
using OuterScout.Domain;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using UnityEngine;

namespace OuterScout.Application.SceneCameras;

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

        _colorCamera.mainCamera.usePhysicalProperties = true;
        _colorCamera.mainCamera.forceIntoRenderTexture = true;
        _colorCamera.mainCamera.usePhysicalProperties = true;
        _colorCamera.mainCamera.gateFit = _gateFit;
        _colorCamera.targetTexture = _colorTexture;

        _depthCamera = CreateDepthCamera();

        Perspective = _perspective; // apply params to both cameras
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

        Destroy(this);
    }

    public static PerspectiveSceneCamera Create(GameObject gameObject, Parameters parameters)
    {
        parameters.Resolution.x.Throw().IfLessThan(1);
        parameters.Resolution.y.Throw().IfLessThan(1);
        gameObject.Throw().If(gameObject.HasComponent<OWCamera>());

        Locator
            .GetPlayerCamera()
            .OrNull()
            .AssertNotNull()
            .OrReturn()
            .CopyTo(gameObject, copyPostProcessing: true);

        return gameObject.AddComponent<PerspectiveSceneCamera, Parameters>(parameters);
    }

    private OWCamera? CreateDepthCamera()
    {
        AssertNotDisposed();

        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth) is false)
        {
            return null;
        }

        _colorCamera.AssertNotNull();

        var depthCameraObject = new GameObject(
            $"{nameof(OuterScout)}.{nameof(PerspectiveSceneCamera)}.depth"
        );

        var depthCamera = _colorCamera.CopyTo(depthCameraObject, copyPostProcessing: false);

        var depthTransform = depthCameraObject.transform;
        depthTransform.parent = transform;
        depthTransform.ResetLocal();

        depthCamera.renderSkybox = false;
        depthCamera.useGUILayout = false;
        depthCamera.useViewmodels = false;
        depthCamera.targetTexture = null;

        depthCamera.mainCamera.usePhysicalProperties = true;
        depthCamera.mainCamera.depthTextureMode = DepthTextureMode.Depth;
        depthCamera.mainCamera.usePhysicalProperties = true;
        depthCamera.mainCamera.eventMask = 0;
        depthCamera.mainCamera.forceIntoRenderTexture = true;

        depthCamera.targetTexture = _depthTexture;

        depthCameraObject.AddComponent<DepthBlitter, PerspectiveSceneCamera>(this);

        return depthCamera;
    }

    private void AssertNotDisposed()
    {
        _disposed.Assert().IfTrue();
    }

    private void OnDestory()
    {
        Destroy(_colorTexture);
        Destroy(_depthTexture);
        Destroy(_colorDepthTexture);
        Destroy(_depthCamera?.gameObject);
    }

    private sealed class DepthBlitter : InitializedBehaviour<PerspectiveSceneCamera>
    {
        private readonly PerspectiveSceneCamera _camera;

        private DepthBlitter()
            : base(out var perspectiveCamera)
        {
            _camera = perspectiveCamera;
        }

        private void OnPostRender()
        {
            // It's impossible move depth bits to colorBuffer
            // without a shader AFAIK, so we blit Depth texture to RGBA32.
            Graphics.Blit(_camera._depthTexture, _camera._colorDepthTexture);
        }
    }
}

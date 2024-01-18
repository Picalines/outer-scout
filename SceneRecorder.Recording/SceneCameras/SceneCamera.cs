using OWML.Common;
using SceneRecorder.Recording.Extensions;
using SceneRecorder.Recording.Recorders;
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
        public required IModConfig ModConfig { get; init; }

        public required IModConsole ModConsole { get; init; }

        public required SceneSettingsDTO SceneSettings { get; init; }

        public required string Id { get; init; }

        public required Vector2Int Resolution { get; init; }
    }

    public string Id { get; }

    public Vector2Int Resolution { get; }

    public Transform Transform { get; private set; } = null!;

    // Main task of SceneCamera is to render a scene to RenderTextures using UnityEngine.Camera
    // and give them to TextureRecorders, which would send their pixel bytes to ffmpeg on CPU.
    // We get thouse bytes using AsyncGPUReadback, which can't access RenderTexture.depthBuffer,
    // so we need to convert depth to color using Graphics.Blit. It's also impossible to
    // render both color and depth textures from a single Camera
    //
    // That's why there're two OWCameras and three RenderTextures for *one* video. You're welcome!

    private IModConfig _modConfig;
    private IModConsole _modConsole;
    private SceneSettingsDTO _sceneSettings;

    private OWCamera _colorCamera = null!;
    private OWCamera _depthCamera = null!;

    private RenderTexture _colorTexture = null!;
    private RenderTexture _depthTexture = null!;
    private RenderTexture _depthColorTexture = null!;

    private SceneCamera()
        : base(out var parameters)
    {
        Id = parameters.Id;
        Resolution = parameters.Resolution;
        _modConfig = parameters.ModConfig;
        _modConsole = parameters.ModConsole;
        _sceneSettings = parameters.SceneSettings;

        Id.Throw().IfNullOrWhiteSpace();
        Resolution.x.Throw().IfLessThan(1);
        Resolution.y.Throw().IfLessThan(1);
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

    public IRecorder CreateColorRecorder()
    {
        return new RenderTextureRecorder()
        {
            ModConfig = _modConfig,
            ModConsole = _modConsole,
            Texture = _colorTexture,
            FrameRate = _sceneSettings.Frames.Rate,
            TargetFile = Path.Combine(_sceneSettings.OutputDirectory, "cameras", Id, "color.mp4"),
        };
    }

    public IRecorder CreateDepthRecorder()
    {
        return new RenderTextureRecorder()
        {
            ModConfig = _modConfig,
            ModConsole = _modConsole,
            Texture = _depthColorTexture,
            FrameRate = _sceneSettings.Frames.Rate,
            TargetFile = Path.Combine(_sceneSettings.OutputDirectory, "cameras", Id, "depth.mp4"),
        };
    }

    private void Start()
    {
        _colorTexture = new RenderTexture(
            Resolution.x,
            Resolution.y,
            0,
            RenderTextureFormat.ARGB32 // respect FFmpegTextureRecorder pixel format!
        );

        _depthTexture = new RenderTexture(
            Resolution.x,
            Resolution.y,
            32, // Unity supports 16, 24 or 32
            RenderTextureFormat.Depth
        );

        _depthColorTexture = new RenderTexture(
            Resolution.x,
            Resolution.y,
            0,
            RenderTextureFormat.RFloat // respect FFmpegTextureRecorder pixel format!
        );

        _colorCamera.targetTexture = _colorTexture;

        _depthCamera = CreateDepthCamera();
        _depthCamera.targetTexture = _depthTexture;

        var frameNotifier = gameObject.AddComponent<UnityFrameNotifier>();
        frameNotifier.FrameEnded += () =>
        {
            Graphics.Blit(_depthTexture, _depthColorTexture);
        };
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

        depthCamera.mainCamera.eventMask = 0;
        depthCamera.mainCamera.forceIntoRenderTexture = true;

        return depthCamera;
    }

    private void OnDestory()
    {
        Destroy(_colorTexture);
        Destroy(_depthTexture);
        Destroy(_depthColorTexture);
        Destroy(_depthCamera.gameObject);
    }
}

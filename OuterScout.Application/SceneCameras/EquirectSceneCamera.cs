using OuterScout.Application.Extensions;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;
using UnityEngine;
using UnityEngine.Rendering;

namespace OuterScout.Application.SceneCameras;

public sealed class EquirectSceneCamera
    : InitializedBehaviour<EquirectSceneCamera.Parameters>,
        ISceneCamera
{
    public readonly struct Parameters
    {
        public required int CubemapFaceSize { get; init; }
    }

    private const int CubeFaceCount = 6;

    private readonly int _faceSize;
    private readonly OWCamera[] _faceCameras = new OWCamera[CubeFaceCount];

    private readonly RenderTexture _cubemapTexture;
    private readonly RenderTexture _colorTexture;

    private bool _disposed = false;

    private static readonly RenderTextureFormat _colorTextureFormat = RenderTextureFormat.ARGB32;

    private static readonly Quaternion[] _cameraRotations = new Quaternion[CubeFaceCount]
    {
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, -90, 0),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90, 0, 0),
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 180, 0),
    };

    private EquirectSceneCamera()
        : base(out var parameters)
    {
        _faceSize = parameters.CubemapFaceSize;

        _colorTexture = new RenderTexture(_faceSize, _faceSize / 2, 0, _colorTextureFormat);

        _cubemapTexture = new RenderTexture(_faceSize, _faceSize, 0, _colorTextureFormat)
        {
            dimension = TextureDimension.Cube,
        };
    }

    private void Awake()
    {
        for (int i = 0; i < _faceCameras.Length; i++)
        {
            var faceCamera = _faceCameras[i] = CreateFaceCamera(transform);

            faceCamera.transform.localRotation = _cameraRotations[i];

            ConfigureFaceRenderTexture(faceCamera, _faceSize, _cubemapTexture, i);
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
            return null;
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

    public static EquirectSceneCamera Create(GameObject gameObject, Parameters parameters)
    {
        parameters.CubemapFaceSize.Throw().IfLessThan(1);

        return gameObject.AddComponent<EquirectSceneCamera, Parameters>(parameters);
    }

    private void Update()
    {
        var flippedColorTexture = RenderTexture.GetTemporary(_colorTexture.descriptor);

        _cubemapTexture.ConvertToEquirect(flippedColorTexture, Camera.MonoOrStereoscopicEye.Mono);

        Graphics.Blit(
            source: flippedColorTexture,
            dest: _colorTexture,
            scale: new Vector2(1, -1),
            offset: new Vector2(0, 1)
        );

        RenderTexture.ReleaseTemporary(flippedColorTexture);
    }

    private static OWCamera CreateFaceCamera(Transform parentTransform)
    {
        var playerCamera = Locator.GetPlayerCamera().OrNull();
        playerCamera.AssertNotNull();

        var cameraObject = new GameObject(
            $"{nameof(OuterScout)}.{nameof(EquirectSceneCamera)}.face"
        );

        var cameraTransfrom = cameraObject.transform;
        cameraTransfrom.parent = parentTransform;
        cameraTransfrom.ResetLocal();

        var faceCamera = playerCamera.CopyTo(cameraObject);

        faceCamera.aspect = 1;
        faceCamera.fieldOfView = 90;
        faceCamera.useGUILayout = false;
        faceCamera.useViewmodels = false;
        faceCamera.mainCamera.depthTextureMode = DepthTextureMode.None;

        faceCamera.mainCamera.eventMask = 0;
        faceCamera.mainCamera.forceIntoRenderTexture = true;

        return faceCamera;
    }

    private static void ConfigureFaceRenderTexture(
        OWCamera camera,
        int faceSize,
        RenderTexture targetCubemap,
        int targetFaceIndex
    )
    {
        var faceTexture = new RenderTexture(faceSize, faceSize, 0, _colorTextureFormat);

        var commandBuffer = new CommandBuffer();
        commandBuffer.CopyTexture(faceTexture, 0, targetCubemap, targetFaceIndex);

        camera.targetTexture = faceTexture;
        camera.mainCamera.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    private void AssertNotDisposed()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(EquirectSceneCamera)} is disposed");
        }
    }

    private void OnDestroy()
    {
        foreach (var faceCamera in _faceCameras)
        {
            Destroy(faceCamera.targetTexture);
            Destroy(faceCamera.gameObject);
        }

        Destroy(_cubemapTexture);
        Destroy(_colorTexture);
    }
}

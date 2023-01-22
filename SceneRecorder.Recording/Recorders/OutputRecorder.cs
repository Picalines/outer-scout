using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

public class OutputRecorder : RecorderComponent
{
    private bool _IsConfigured = false;

    private ComposedRecorder _ComposedRecorder = null!;

    private Action? _OnRecordingStarted = null;

    private Action? _OnRecordingFinished = null;

    public OutputRecorder()
    {
        Awoken += OnAwake;
    }

    private void OnAwake()
    {
        _ComposedRecorder = gameObject.AddComponent<ComposedRecorder>();

        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
    }

    private void OnRecordingStarted()
    {
        if (_IsConfigured is false)
        {
            throw new InvalidOperationException($"recording started before the {nameof(Configure)} method");
        }

        _OnRecordingStarted?.Invoke();

        _ComposedRecorder.enabled = true;
    }

    private void OnRecordingFinished()
    {
        _ComposedRecorder.enabled = false;

        _OnRecordingFinished?.Invoke();

        _IsConfigured = false;
    }

    public bool IsAbleToRecord
    {
        get
        {
            return Locator.GetPlayerBody() != null
                && GameObject.Find("FREECAM") is var freeCamObject
                && freeCamObject != null
                && freeCamObject.TryGetComponent<OWCamera>(out var freeCam)
                && freeCam.enabled;
        }
    }

    public void Configure(IModConsole modConsole, SceneSettings sceneSettings, string outputDirectory)
    {
        if (IsAbleToRecord is false)
        {
            throw new InvalidOperationException("unable to record");
        }

        var player = Locator.GetPlayerBody();
        var playerCameraContoller = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
        var freeCamera = GameObject.Find("FREECAM").GetComponent<OWCamera>();

        // background recorder
        var backgroundRecorder = GetOrAddComponent<BackgroundRecorder>(freeCamera.gameObject);
        (backgroundRecorder.Width, backgroundRecorder.Height) = sceneSettings.Resolution;
        backgroundRecorder.FrameRate = sceneSettings.FrameRate;

        // depth recorder
        var depthRecorder = GetOrAddComponent<DepthRecorder>(freeCamera.gameObject);
        (depthRecorder.Width, depthRecorder.Height) = sceneSettings.Resolution;
        depthRecorder.FrameRate = sceneSettings.FrameRate;

        // hdri recorder
        var hdriRecorder = GetOrAddComponent<HDRIRecorder>(playerCameraContoller.gameObject);
        hdriRecorder.CubemapFaceSize = sceneSettings.HDRIFaceSize;
        hdriRecorder.FrameRate = sceneSettings.FrameRate;

        // render hdri to GUI (TODO: replace with web)
        GetOrAddComponent<RenderTextureRecorderGUI>(hdriRecorder.gameObject);

        // free camera transform recorder
        var freeCameraTransformRecorder = GetOrAddComponent<TransformRecorder>(freeCamera.gameObject);

        // combine recorders
        if (_ComposedRecorder.Recorders.Count == 0)
        {
            _ComposedRecorder.Recorders = new IRecorder[] { backgroundRecorder, depthRecorder, hdriRecorder, freeCameraTransformRecorder };
        }

        foreach (var recorder in _ComposedRecorder.Recorders.OfType<RenderTextureRecorder>())
        {
            recorder.ModConsole = modConsole;

            recorder.TargetFile = Path.Combine(outputDirectory, recorder switch
            {
                BackgroundRecorder => "background.mp4",
                DepthRecorder => "depth.mp4",
                HDRIRecorder => "hdri.mp4",
                _ => throw new NotImplementedException(),
            });
        }

        var playerRenderersToToggle = sceneSettings.HidePlayerModel
            ? player.gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        _OnRecordingStarted = () =>
        {
            ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            playerCameraContoller.SetDegreesY(0);
            Locator.GetQuantumMoon().SetActivation(false);

            modConsole.WriteLine($"Recording started ({outputDirectory})");
        };

        _OnRecordingFinished = () =>
        {
            ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().SetActivation(true);

            modConsole.WriteLine($"Recording finished ({outputDirectory})");
        };

        _IsConfigured = true;

        static void ForEach<T>(T[] array, Action<T> action)
        {
            foreach (var item in array)
            {
                action(item);
            }
        }
    }

    private static TComponent GetOrAddComponent<TComponent>(GameObject gameObject)
        where TComponent : Component
    {
        if (gameObject.TryGetComponent<TComponent>(out var component) is false)
        {
            component = gameObject.AddComponent<TComponent>();
        }

        return component;
    }
}

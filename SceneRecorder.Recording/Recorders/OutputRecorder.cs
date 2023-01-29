using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;
using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

public sealed class OutputRecorder : RecorderComponent
{
    public IModConsole? ModConsole { get; set; } = null;

    public RecorderSettings? Settings { get; set; } = null;

    public string? OutputDirectory { get; set; } = null;

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
        BeforeFrameRecorded += OnBeforeFrameRecorded;
    }

    private void OnRecordingStarted()
    {
        Configure();

        _OnRecordingStarted?.Invoke();

        _ComposedRecorder.enabled = true;
    }

    private void OnRecordingFinished()
    {
        _ComposedRecorder.enabled = false;

        _OnRecordingFinished?.Invoke();
    }

    private void OnBeforeFrameRecorded()
    {
        if (FramesRecorded >= Settings!.FrameCount)
        {
            enabled = false;
        }
    }

    [MemberNotNullWhen(true, nameof(Settings), nameof(OutputDirectory), nameof(ModConsole))]
    public bool IsAbleToRecord
    {
        get
        {
            return IsRecording
                || (Settings is not null
                && OutputDirectory is not null
                && ModConsole is not null
                && LocatorExtensions.IsInSolarSystemScene()
                && GameObject.Find("FREECAM").Nullable() is { } freeCamObject
                && freeCamObject.TryGetComponent<Camera>(out var freeCam)
                && freeCam.enabled);
        }
    }

    private void Configure()
    {
        if (IsAbleToRecord is false)
        {
            throw new InvalidOperationException();
        }

        var player = Locator.GetPlayerBody();
        var playerCameraContoller = Locator.GetPlayerCamera().GetComponent<PlayerCameraController>();
        var freeCamera = GameObject.Find("FREECAM").GetComponent<OWCamera>();

        // background recorder
        var backgroundRecorder = GetOrAddComponent<BackgroundRecorder>(freeCamera.gameObject);
        (backgroundRecorder.Width, backgroundRecorder.Height) = Settings.Resolution;
        backgroundRecorder.FrameRate = Settings.FrameRate;

        // depth recorder
        var depthRecorder = GetOrAddComponent<DepthRecorder>(freeCamera.gameObject);
        (depthRecorder.Width, depthRecorder.Height) = Settings.Resolution;
        depthRecorder.FrameRate = Settings.FrameRate;

        // hdri recorder
        var hdriPivot = new GameObject($"{nameof(SceneRecorder)} HDRI Pivot");
        hdriPivot.transform.parent = LocatorExtensions.GetCurrentGroundBody()!.transform;
        playerCameraContoller.transform.CopyGlobalTransformTo(hdriPivot.transform);

        var hdriRecorder = GetOrAddComponent<HDRIRecorder>(hdriPivot);
        hdriRecorder.CubemapFaceSize = Settings.HDRIFaceSize;
        hdriRecorder.FrameRate = Settings.FrameRate;

        // render hdri to GUI (TODO: replace with web)
        GetOrAddComponent<RenderTextureRecorderGUI>(hdriRecorder.gameObject);

        // combine recorders
        if (_ComposedRecorder.Recorders.Count == 0)
        {
            _ComposedRecorder.Recorders = new IRecorder[] { backgroundRecorder, depthRecorder, hdriRecorder };
        }

        foreach (var recorder in _ComposedRecorder.Recorders.OfType<RenderTextureRecorder>())
        {
            recorder.ModConsole = ModConsole;

            recorder.TargetFile = Path.Combine(OutputDirectory, recorder switch
            {
                BackgroundRecorder => "background.mp4",
                DepthRecorder => "depth.mp4",
                HDRIRecorder => "hdri.mp4",
                _ => throw new NotImplementedException(),
            });
        }

        var playerRenderersToToggle = Settings.HidePlayerModel
            ? player.gameObject.GetComponentsInChildren<Renderer>()
                .Where(renderer => renderer.enabled)
                .ToArray()
            : Array.Empty<Renderer>();

        _OnRecordingStarted = () =>
        {
            ForEach(playerRenderersToToggle, renderer => renderer.enabled = false);
            playerCameraContoller.SetDegreesY(0);
            Locator.GetQuantumMoon().SetActivation(false);

            ModConsole.WriteLine($"Recording started ({OutputDirectory})");
        };

        _OnRecordingFinished = () =>
        {
            Destroy(hdriPivot);
            ForEach(playerRenderersToToggle, renderer => renderer.enabled = true);
            Locator.GetQuantumMoon().SetActivation(true);

            ModConsole.WriteLine($"Recording finished ({OutputDirectory})");
        };

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

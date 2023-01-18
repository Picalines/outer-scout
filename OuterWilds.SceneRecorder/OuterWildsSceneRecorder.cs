﻿using OWML.Common;
using OWML.ModHelper;
using Picalines.OuterWilds.SceneRecorder.Json;
using Picalines.OuterWilds.SceneRecorder.Recorders;
using Picalines.OuterWilds.SceneRecorder.Utils;
using Picalines.OuterWilds.SceneRecorder.WebInterop;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class OuterWildsSceneRecorder : ModBehaviour
{
    private const Key _RecordKey = Key.F9;

    private SceneRecorderSettings _Settings = null!;

    private WebUI? _WebUI = null;

    private string _RecordingOutputDir = null!;

    private readonly LazyUnityReference<OWRigidbody> _Player = new(Locator.GetPlayerBody);

    private readonly LazyUnityReference<OWCamera> _PlayerCamera = new(Locator.GetPlayerCamera);

    private readonly LazyUnityReference<PlayerAudioController> _PlayerAudioController = new(Locator.GetPlayerAudioController);

    private readonly LazyUnityReference<OWCamera> _FreeCamera = LazyUnityReference.FromFind<OWCamera>("FREECAM");

    private ComposedRecorder? _ComposedRecorder = null;

    public override void Configure(IModConfig config)
    {
        _WebUI?.Dispose();

        _Settings = new SceneRecorderSettings(config);

        _WebUI = new WebUI(ModHelper.Console, _Settings);

        if (_ComposedRecorder != null)
        {
            Destroy(_ComposedRecorder);
        }
    }

    private void Start()
    {
        ModHelper.Console.WriteLine($"{nameof(OuterWildsSceneRecorder)} is loaded!", MessageType.Success);
    }

    private bool ListenToInput
    {
        get => _Player.ObjectExists;
    }

    private void OnDestroy()
    {
        _WebUI?.Dispose();

        Destroy(_ComposedRecorder);
    }

    private void Update()
    {
        if (ListenToInput is false)
        {
            return;
        }

        if (Keyboard.current[_RecordKey].isPressed)
        {
            InitializeRecordersIfNot();
            StartRecordingIfNot();
        }
        else
        {
            StopRecordingIfCan();
        }
    }

    [MemberNotNull(nameof(_ComposedRecorder))]
    private void InitializeRecordersIfNot()
    {
        if (_ComposedRecorder != null)
        {
            return;
        }

        gameObject.SetActive(false);

        // background recorder
        var backgroundRecorder = _FreeCamera.Object.gameObject.AddComponent<BackgroundRecorder>();
        (backgroundRecorder.Width, backgroundRecorder.Height) = (_Settings.Width, _Settings.Height);
        backgroundRecorder.FrameRate = _Settings.FrameRate;

        // depth recorder
        var depthRecorder = _FreeCamera.Object.gameObject.AddComponent<DepthRecorder>();
        (depthRecorder.Width, depthRecorder.Height) = (_Settings.Width, _Settings.Height);
        depthRecorder.FrameRate = _Settings.FrameRate;

        // hdri recorder
        var hdriRecorder = _PlayerCamera.Object.gameObject.AddComponent<HDRIRecorder>();
        hdriRecorder.CubemapFaceSize = _Settings.HDRIFaceSize;
        hdriRecorder.LocalPositionOffset = _Settings.HDRIInFeet ? Vector3.down : Vector3.zero;
        hdriRecorder.FrameRate = _Settings.FrameRate;

        hdriRecorder.gameObject.AddComponent<RenderTextureRecorderGUI>();

        // free camera transform recorder
        var freeCameraTransformRecorder = _FreeCamera.Object.gameObject.AddComponent<TransformRecorder>();

        // composed recorder
        _ComposedRecorder = gameObject.AddComponent<ComposedRecorder>();
        _ComposedRecorder.Recorders = new IRecorder[] { backgroundRecorder, depthRecorder, hdriRecorder, freeCameraTransformRecorder };

        // on before recording started event
        Action? showPlayerModelAction = null;
        _ComposedRecorder.RecordingStarted += () =>
        {
            showPlayerModelAction = _Settings.HidePlayerModel ? DisableEnabledRenderers(_Player.Object.gameObject) : null;
            _PlayerCamera.Object.gameObject.GetComponent<PlayerCameraController>().SetDegreesY(0);
            Locator.GetQuantumMoon().SetActivation(false);

            ModHelper.Console.WriteLine($"Recording started ({_RecordingOutputDir})");
        };

        _ComposedRecorder.RecordingFinished += () =>
        {
            showPlayerModelAction?.Invoke();
            Locator.GetQuantumMoon().SetActivation(true);

            var sceneData = SceneData.Capture(_Settings, _ComposedRecorder.FramesRecorded, freeCameraTransformRecorder.RecordedValues);
            File.WriteAllText(Path.Combine(_RecordingOutputDir, ".owscene"), sceneData.ToJSON());

            ModHelper.Console.WriteLine($"Recording finished ({_RecordingOutputDir})");
        };

        gameObject.SetActive(true);
    }

    private void StartRecordingIfNot()
    {
        if (_ComposedRecorder == null || _ComposedRecorder.enabled is true)
        {
            return;
        }

        if (_FreeCamera.Object.GetComponent<Camera>() is { enabled: false })
        {
            _PlayerAudioController.Object.PlayNegativeUISound();
            return;
        }

        var dateTime = DateTime.Now;
        _RecordingOutputDir = Path.Combine(_Settings.OutputDirectory, $"{dateTime:dd.MM.yyyy}_{dateTime.Ticks}/");
        Directory.CreateDirectory(_RecordingOutputDir);

        foreach (var recorder in _ComposedRecorder.Recorders.OfType<RenderTextureRecorder>())
        {
            recorder.ModConsole = ModHelper.Console;

            recorder.TargetFile = Path.Combine(_RecordingOutputDir, recorder switch
            {
                BackgroundRecorder => "background.mp4",
                DepthRecorder => "depth.mp4",
                HDRIRecorder => "hdri.mp4",
                _ => throw new NotImplementedException(),
            });
        }

        _ComposedRecorder!.enabled = true;
    }

    private void StopRecordingIfCan()
    {
        if (!(_ComposedRecorder != null && _ComposedRecorder.enabled))
        {
            return;
        }

        _ComposedRecorder.enabled = false;
    }

    private static Action DisableEnabledRenderers(GameObject gameObject)
    {
        var enabledRenderers = gameObject.GetComponentsInChildren<Renderer>()
            .Where(renderer => renderer.enabled)
            .ToArray();

        foreach (var renderer in enabledRenderers)
        {
            renderer.enabled = false;
        }

        return () =>
        {
            foreach (var renderer in enabledRenderers)
            {
                renderer.enabled = true;
            }

            enabledRenderers = null!;
        };
    }
}
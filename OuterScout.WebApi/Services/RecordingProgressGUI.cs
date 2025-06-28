using OuterScout.Application.Recording;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using UnityEngine;

namespace OuterScout.WebApi.Services;

internal sealed class RecordingProgressGUI : InitializedBehaviour<IServiceContainer>
{
    private readonly ApiResourceRepository _resources;

    private readonly Texture2D _backgroundTexture;

    private readonly GUIStyle _backgroundStyle;

    private readonly GUIStyle _lineStyle;

    private readonly string[] _linesToDraw;

    private SceneRecorder? _sceneRecorder = null;

    private RecordingProgressGUI()
        : base(out var services)
    {
        _resources = services.Resolve<ApiResourceRepository>();

        _backgroundTexture = new Texture2D(1, 1);
        _backgroundTexture.SetPixel(0, 0, Color.black);
        _backgroundTexture.Apply();

        _backgroundStyle = new GUIStyle() { normal = { background = _backgroundTexture } };
        _lineStyle = new GUIStyle() { normal = { textColor = Color.white }, fontSize = 30 };

        _linesToDraw = new string[3];
        for (int i = 0; i < _linesToDraw.Length; i++)
        {
            _linesToDraw[i] = "";
        }

        _linesToDraw[0] = $"{nameof(OuterScout)} is recording a scene";
    }

    private void Start()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        _sceneRecorder = _resources.GlobalContainer.GetResource<SceneRecorder>();
    }

    private void OnDisable()
    {
        _sceneRecorder = null;
    }

    private void Update()
    {
        if (_sceneRecorder is not { IsRecording: true })
        {
            return;
        }

        var numberOfFrames = _sceneRecorder.FrameRange.Length + 1;

        _linesToDraw[1] = $"Recorded frames: {_sceneRecorder.FramesRecorded}/{numberOfFrames}";
        _linesToDraw[2] = $"Current frame: {_sceneRecorder.CurrentFrame}";
    }

    private void OnGUI()
    {
        if (_sceneRecorder is not { IsRecording: true })
        {
            return;
        }

        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none, _backgroundStyle);

        foreach (var (i, line) in _linesToDraw.Indexed())
        {
            var height = _lineStyle.fontSize + 10;
            GUI.Label(new Rect(25, 25 + i * height, Screen.width, height), line, _lineStyle);
        }
    }

    private void OnDestroy()
    {
        Destroy(_backgroundTexture);
    }
}

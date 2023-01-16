using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

[RequireComponent(typeof(Camera))]
internal sealed class BackgroundRecorder : RenderTextureRecorder
{
    public int Width { get; set; } = 1920;

    public int Height { get; set; } = 1080;

    private Camera _Camera = null!;

    private RenderTexture _SourceRenderTexture = null!;

    public BackgroundRecorder()
    {
        Awoken += OnAwoken;
        RecordingStarted += OnRecordingStarted;
        RecordingFinished += OnRecordingFinished;
    }

    protected override RenderTexture ProvideSourceRenderTexture()
    {
        return _SourceRenderTexture = new RenderTexture(Width, Height, 16);
    }

    private void OnAwoken()
    {
        _Camera = GetComponent<Camera>();
    }

    private void OnRecordingStarted()
    {
        _Camera.targetTexture = _SourceRenderTexture;
    }

    private void OnRecordingFinished()
    {
        _Camera.targetTexture = null;
    }

    private void OnDestroy()
    {
        DestroyImmediate(_SourceRenderTexture);
    }
}

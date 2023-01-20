using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

// TODO: replace with WebUI

[RequireComponent(typeof(RenderTextureRecorder))]
public sealed class RenderTextureRecorderGUI : MonoBehaviour
{
    private RenderTextureRecorder _TextureRecorder = null!;

    private DateTime _StartedRecordingAt;

    private void Awake()
    {
        _TextureRecorder = GetComponent<RenderTextureRecorder>();

        _TextureRecorder.RecordingStarted += OnRecordingStarted;
    }

    private void OnDestroy()
    {
        _TextureRecorder.RecordingStarted -= OnRecordingStarted;
    }

    private void OnRecordingStarted()
    {
        _StartedRecordingAt = DateTime.Now;
    }

    private void OnGUI()
    {
        if (_TextureRecorder is not { SourceRenderTexture: not null, IsRecording: true })
        {
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _TextureRecorder.SourceRenderTexture);

        TimeSpan elapsedRealtime = DateTime.Now - _StartedRecordingAt;
        TimeSpan elapsedVideo = TimeSpan.FromSeconds((float)_TextureRecorder.FramesRecorded / _TextureRecorder.FrameRate);
        double videoToRealtimeRatio = elapsedRealtime.TotalSeconds / elapsedVideo.TotalSeconds;

        GUI.Box(new Rect(0, 0, 350, 80), GUIContent.none);
        GUI.Label(new Rect(10, 10, 500, 30), $"Recorded {elapsedVideo:hh':'mm':'ss} ({_TextureRecorder.FramesRecorded} frames)");
        GUI.Label(new Rect(10, 40, 500, 30), $"Elapsed {elapsedRealtime:hh':'mm':'ss} ({videoToRealtimeRatio:0.000} times more)");
    }
}

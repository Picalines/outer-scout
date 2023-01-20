using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

public abstract class RecorderComponent : MonoBehaviour, IRecorder
{
    public event Action? Awoken;

    public event Action? RecordingStarted;

    public event Action? BeforeFrameRecorded;

    public event Action? RecordingFinished;

    public bool IsRecording { get; private set; } = false;

    public int FramesRecorded { get; private set; } = 0;

    private bool _IsInAwake = false;

    private void Awake()
    {
        _IsInAwake = true;

        enabled = false;

        Awoken?.Invoke();
        _IsInAwake = false;
    }

    private void OnEnable()
    {
        RecordingStarted?.Invoke();
        FramesRecorded = 0;
        IsRecording = true;
    }

    private void LateUpdate()
    {
        if (IsRecording is false)
        {
            return;
        }

        BeforeFrameRecorded?.Invoke();
        FramesRecorded++;
    }

    private void OnDisable()
    {
        if (_IsInAwake || IsRecording is false)
        {
            return;
        }

        IsRecording = false;
        RecordingFinished?.Invoke();
    }
}

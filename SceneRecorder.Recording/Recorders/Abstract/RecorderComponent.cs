using System.Collections;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;

public abstract class RecorderComponent : MonoBehaviour, IRecorder
{
    public event Action? Awoken;

    public event Action? RecordingStarted;

    public event Action? FrameStarted;

    public event Action? FrameEnded;

    public event Action? RecordingFinished;

    public bool IsRecording { get; private set; } = false;

    public int FramesRecorded { get; private set; } = 0;

    private bool _IsInAwake = false;

    private static readonly WaitForEndOfFrame _WaitForEndOfFrame = new();

    internal RecorderComponent() { }

    private void Awake()
    {
        _IsInAwake = true;

        enabled = false;

        Awoken?.Invoke();
        _IsInAwake = false;
    }

    private void OnEnable()
    {
        if (IsRecording)
        {
            return;
        }

        IsRecording = true;
        StartCoroutine(RecorderCoroutine());
    }

    private void OnDisable()
    {
        if (_IsInAwake)
        {
            return;
        }

        IsRecording = false;
    }

    private IEnumerator RecorderCoroutine()
    {
        FramesRecorded = 0;
        RecordingStarted?.Invoke();

        while (IsRecording)
        {
            FrameStarted?.Invoke();

            yield return _WaitForEndOfFrame;

            FramesRecorded++;
            FrameEnded?.Invoke();
        }

        yield return null;

        RecordingFinished?.Invoke();
    }
}

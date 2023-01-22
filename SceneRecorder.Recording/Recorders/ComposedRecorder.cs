using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders;

internal sealed class ComposedRecorder : MonoBehaviour, IRecorder
{
    private IReadOnlyList<IRecorder> _Recorders = Array.Empty<IRecorder>();

    private bool _Awoken = false;

    public IReadOnlyList<IRecorder> Recorders
    {
        get => _Recorders;
        set
        {
            if (IsRecording is true)
            {
                throw new InvalidOperationException();
            }

            _Recorders = value;
        }
    }

    public IRecorder? MainRecorder
    {
        get => Recorders.FirstOrDefault();
    }

    public bool IsRecording
    {
        get => MainRecorder?.IsRecording ?? false;
    }

    public int FramesRecorded
    {
        get => MainRecorder?.FramesRecorded ?? 0;
    }

    public event Action RecordingStarted
    {
        add
        {
            if (MainRecorder is not null)
                MainRecorder.RecordingStarted += value;
        }
        remove
        {
            if (MainRecorder is not null)
                MainRecorder.RecordingStarted -= value;
        }
    }

    public event Action BeforeFrameRecorded
    {
        add
        {
            if (MainRecorder is not null)
                MainRecorder.BeforeFrameRecorded += value;
        }
        remove
        {
            if (MainRecorder is not null)
                MainRecorder.BeforeFrameRecorded -= value;
        }
    }

    public event Action RecordingFinished
    {
        add
        {
            if (MainRecorder is not null)
                MainRecorder.RecordingFinished += value;
        }
        remove
        {
            if (MainRecorder is not null)
                MainRecorder.RecordingFinished -= value;
        }
    }

    private void Awake()
    {
        enabled = false;
        _Awoken = true;
    }

    private void OnEnable()
    {
        foreach (var recorder in Recorders)
        {
            if (recorder is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = true;
            }
        }
    }

    private void OnDisable()
    {
        if (_Awoken is false)
        {
            return;
        }

        foreach (var recorder in Recorders)
        {
            if (recorder is MonoBehaviour monoBehaviour)
            {
                monoBehaviour.enabled = false;
            }
        }
    }

    private void OnDestory()
    {
        foreach (var recorder in Recorders)
        {
            if (recorder is MonoBehaviour monoBehaviour && monoBehaviour != null)
            {
                Destroy(monoBehaviour);
            }
        }
    }
}

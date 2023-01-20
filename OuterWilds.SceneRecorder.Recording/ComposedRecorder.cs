using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recording;

public sealed class ComposedRecorder : MonoBehaviour, IRecorder
{
    private IReadOnlyList<IRecorder> _Recorders = Array.Empty<IRecorder>();

    private bool _Awoken = false;

    public IReadOnlyList<IRecorder> Recorders
    {
        get => _Recorders;
        set
        {
            if (_Awoken is true)
            {
                throw new InvalidOperationException();
            }

            _Recorders = value;
        }
    }

    public IRecorder MainRecorder
    {
        get => Recorders[0];
    }

    public bool IsRecording
    {
        get => MainRecorder.IsRecording;
    }

    public int FramesRecorded
    {
        get => MainRecorder.FramesRecorded;
    }

    public event Action RecordingStarted
    {
        add => MainRecorder.RecordingStarted += value;
        remove => MainRecorder.RecordingStarted -= value;
    }

    public event Action BeforeFrameRecorded
    {
        add => MainRecorder.BeforeFrameRecorded += value;
        remove => MainRecorder.BeforeFrameRecorded -= value;
    }

    public event Action RecordingFinished
    {
        add => MainRecorder.RecordingFinished += value;
        remove => MainRecorder.RecordingFinished -= value;
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal sealed class ComposedRecorder : MonoBehaviour, IRecorder
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

    public int Framerate
    {
        get => MainRecorder.Framerate;
        set
        {
            foreach (var recorder in Recorders)
            {
                recorder.Framerate = value;
            }
        }
    }

    public bool IsRecording
    {
        get => MainRecorder.IsRecording;
    }

    public int FramesRecorded
    {
        get => MainRecorder.FramesRecorded;
    }

    public event Action BeforeRecordingStarted
    {
        add => MainRecorder.BeforeRecordingStarted += value;
        remove => MainRecorder.BeforeRecordingStarted -= value;
    }

    public event Action BeforeFrameRecorded
    {
        add => MainRecorder.BeforeFrameRecorded += value;
        remove => MainRecorder.BeforeFrameRecorded -= value;
    }

    public event Action AfterRecordingFinished
    {
        add => MainRecorder.AfterRecordingFinished += value;
        remove => MainRecorder.AfterRecordingFinished -= value;
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

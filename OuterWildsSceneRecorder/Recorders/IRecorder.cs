using System;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal interface IRecorder
{
    public bool IsRecording { get; }

    public int FramesRecorded { get; }

    public event Action RecordingStarted;

    public event Action BeforeFrameRecorded;

    public event Action RecordingFinished;
}

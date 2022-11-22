using System;

namespace Picalines.OuterWilds.SceneRecorder.Recorders;

internal interface IRecorder
{
    public int Framerate { get; set; }

    public bool IsRecording { get; }

    public int FramesRecorded { get; }

    public event Action BeforeRecordingStarted;

    public event Action BeforeFrameRecorded;

    public event Action AfterRecordingFinished;
}

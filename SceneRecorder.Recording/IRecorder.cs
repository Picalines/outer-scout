namespace SceneRecorder.Recording;

public interface IRecorder
{
    public bool IsRecording { get; }

    public int FramesRecorded { get; }

    public event Action RecordingStarted;

    public event Action FrameStarted;

    public event Action RecordingFinished;
}

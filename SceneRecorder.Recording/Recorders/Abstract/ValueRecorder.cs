namespace Picalines.OuterWilds.SceneRecorder.Recording.Recorders.Abstract;

internal abstract class ValueRecorder<T> : RecorderComponent
{
    private List<T>? _ValueFrames = null;

    public ValueRecorder()
    {
        RecordingStarted += OnBeforeRecordingStarted;
        BeforeFrameRecorded += OnBeforeFrameRecorded;
    }

    protected abstract T CaptureValue();

    public IReadOnlyList<T> RecordedValues
    {
        get => _ValueFrames as IReadOnlyList<T> ?? Array.Empty<T>();
    }

    private void OnBeforeRecordingStarted()
    {
        _ValueFrames ??= new List<T>();
        _ValueFrames.Clear();
    }

    private void OnBeforeFrameRecorded()
    {
        _ValueFrames!.Add(CaptureValue());
    }
}

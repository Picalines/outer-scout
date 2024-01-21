using SceneRecorder.Shared.Extensions;

namespace SceneRecorder.Recording.Recorders;

internal sealed class ComposedRecorder : IRecorder
{
    public required IRecorder[] Recorders { get; init; }

    public void StartRecording()
    {
        Recorders.ForEach(r => r.StartRecording());
    }

    public void RecordData()
    {
        Recorders.ForEach(r => r.RecordData());
    }

    public void StopRecording()
    {
        Recorders.ForEach(r => r.StopRecording());
    }
}

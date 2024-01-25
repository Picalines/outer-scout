using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Recording.Recorders;

internal sealed class ComposedRecorder : IRecorder
{
    private readonly HashSet<IRecorder> _recorders = [];

    private bool _isRecording = false;

    public IEnumerable<IRecorder> Recorders => _recorders;

    public void AddRecorder(IRecorder recorder)
    {
        _isRecording.Throw().IfTrue();

        _recorders.Add(recorder);
    }

    public void StartRecording()
    {
        _isRecording = true;

        _recorders.ForEach(r => r.StartRecording());
    }

    public void RecordData()
    {
        _recorders.ForEach(r => r.RecordData());
    }

    public void StopRecording()
    {
        _recorders.ForEach(r => r.StopRecording());

        _isRecording = false;
    }
}

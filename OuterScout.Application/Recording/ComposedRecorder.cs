using OuterScout.Shared.Extensions;

namespace OuterScout.Application.Recording;

internal sealed class ComposedRecorder : IRecorder
{
    private readonly IReadOnlyList<IRecorder> _recorders;

    private bool _disposed = false;

    public ComposedRecorder(IReadOnlyList<IRecorder> recorders)
    {
        _recorders = recorders;
    }

    public void Capture()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(ComposedRecorder)} is disposed");
        }

        _recorders.ForEach(recorder => recorder.Capture());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _recorders.ForEach(recorder => recorder.Dispose());

        _disposed = true;
    }
}

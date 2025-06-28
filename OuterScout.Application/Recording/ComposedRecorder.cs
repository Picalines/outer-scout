using OuterScout.Shared.Extensions;

namespace OuterScout.Application.Recording;

internal sealed class ComposedRecorder(IReadOnlyList<IRecorder> recorders) : IRecorder
{
    private bool _disposed = false;

    public void Capture()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(ComposedRecorder)} is disposed");
        }

        recorders.ForEach(recorder => recorder.Capture());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        recorders.ForEach(recorder => recorder.Dispose());

        _disposed = true;
    }
}

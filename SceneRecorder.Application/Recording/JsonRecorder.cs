using Newtonsoft.Json;

namespace OuterScout.Application.Recording;

public sealed partial class JsonRecorder : IRecorder
{
    private readonly Func<object?> _valueGetter;

    private readonly JsonSerializer _jsonSerializer;

    private readonly JsonWriter _jsonWriter;

    private bool _disposed = false;

    private JsonRecorder(
        Func<object?> valueGetter,
        JsonSerializer jsonSerializer,
        JsonWriter jsonWriter
    )
    {
        _valueGetter = valueGetter;
        _jsonSerializer = jsonSerializer;
        _jsonWriter = jsonWriter;
    }

    public void Capture()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(JsonRecorder)} is disposed");
        }

        _jsonSerializer.Serialize(_jsonWriter, _valueGetter());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _jsonWriter.Close();

        _disposed = true;
    }
}

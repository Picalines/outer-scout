using Newtonsoft.Json;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Application.Recording;

public sealed class JsonRecorder<T> : IRecorder
{
    public sealed class Parameters
    {
        public required string TargetFile { get; init; }

        public required Func<T> ValueGetter { get; init; }

        public JsonSerializer JsonSerializer { get; init; } = new();

        public string ValuesArrayName { get; init; } = "values";

        public IDictionary<string, object?> AdditionalProperties { get; init; } =
            new Dictionary<string, object?>();
    }

    private readonly Func<T> _valueGetter;

    private readonly JsonSerializer _jsonSerializer;

    private readonly JsonWriter _jsonWriter;

    private bool _disposed = false;

    private JsonRecorder(Func<T> valueGetter, JsonSerializer jsonSerializer, JsonWriter jsonWriter)
    {
        _valueGetter = valueGetter;
        _jsonSerializer = jsonSerializer;
        _jsonWriter = jsonWriter;
    }

    public static JsonRecorder<T> StartRecording(Parameters parameters)
    {
        parameters.ValuesArrayName.Throw().IfNullOrWhiteSpace();

        var fileWriter = new StreamWriter(parameters.TargetFile, append: false);

        var jsonSerializer = parameters.JsonSerializer;
        var jsonWriter = new JsonTextWriter(fileWriter)
        {
            CloseOutput = true,
            AutoCompleteOnClose = true,
            Indentation = 0
        };

        jsonWriter.WriteStartObject();

        foreach (var (key, value) in parameters.AdditionalProperties)
        {
            key.Throw().IfEquals(parameters.ValuesArrayName);

            jsonWriter.WritePropertyName(key);
            jsonSerializer.Serialize(jsonWriter, value);
        }

        jsonWriter.WritePropertyName(parameters.ValuesArrayName);
        jsonWriter.WriteStartArray();

        return new JsonRecorder<T>(parameters.ValueGetter, jsonSerializer, jsonWriter);
    }

    public void Capture()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(JsonRecorder<T>)} is disposed");
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

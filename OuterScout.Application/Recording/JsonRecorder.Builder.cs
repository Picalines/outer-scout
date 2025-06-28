using Newtonsoft.Json;
using OuterScout.Shared.Validation;

namespace OuterScout.Application.Recording;

public sealed partial class JsonRecorder
{
    public sealed class Builder : IRecorder.IBuilder
    {
        private readonly string _targetFile;

        private readonly Func<object?> _valueGetter;

        private string _valuesArrayName = "values";

        private Dictionary<string, object?> _additionalProperties = [];

        private JsonSerializer? _jsonSerializer = null;

        public Builder(string targetFile, Func<object?> valueGetter)
        {
            _targetFile = targetFile;
            _valueGetter = valueGetter;
        }

        public IRecorder StartRecording()
        {
            var fileWriter = new StreamWriter(_targetFile, append: false);

            var jsonSerializer = _jsonSerializer ?? new();
            var jsonWriter = new JsonTextWriter(fileWriter)
            {
                CloseOutput = true,
                AutoCompleteOnClose = true,
                Indentation = 0
            };

            jsonWriter.WriteStartObject();

            foreach (var (key, value) in _additionalProperties)
            {
                jsonWriter.WritePropertyName(key);
                jsonSerializer.Serialize(jsonWriter, value);
            }

            jsonWriter.WritePropertyName(_valuesArrayName);
            jsonWriter.WriteStartArray();

            return new JsonRecorder(_valueGetter, jsonSerializer, jsonWriter);
        }

        public Builder WithValuesArrayNamed(string valuesArrayName)
        {
            valuesArrayName.Throw().If(_additionalProperties.ContainsKey(valuesArrayName));

            _valuesArrayName = valuesArrayName;
            return this;
        }

        public Builder WithAdditionalProperty(string key, object? value)
        {
            key.Throw().If(_additionalProperties.ContainsKey(key)).IfEquals(_valuesArrayName);

            _additionalProperties.Add(key, value);
            return this;
        }

        public Builder WithJsonSerializer(JsonSerializer jsonSerializer)
        {
            _jsonSerializer = jsonSerializer;
            return this;
        }
    }
}

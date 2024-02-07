using Newtonsoft.Json;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.WebApi.DTOs.Json;

internal abstract class NumberArrayConverter<T> : JsonConverter<T>
{
    private readonly int _arrayLength;

    public NumberArrayConverter(int arrayLength)
    {
        _arrayLength = arrayLength;
    }

    protected abstract T ReadJson(ReadOnlySpan<float> array);

    protected abstract void WriteJson(in T value, ref Span<float> array);

    public sealed override T ReadJson(
        JsonReader reader,
        Type objectType,
        T? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        Span<float> array = stackalloc float[_arrayLength];

        var arrayIndex = 0;

        while (reader.Read())
        {
            if (reader.TokenType is JsonToken.EndArray)
            {
                break;
            }

            if (reader.TokenType is not (JsonToken.Float or JsonToken.Integer))
            {
                ThrowReaderException();
            }

            array[arrayIndex++] = Convert.ToSingle(reader.Value);
        }

        if (arrayIndex != _arrayLength)
        {
            ThrowReaderException();
        }

        return ReadJson(array);
    }

    public sealed override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        writer.WriteStartArray();

        Span<float> array = stackalloc float[_arrayLength];

        WriteJson(value, ref array);

        for (int i = 0; i < array.Length; i++)
        {
            writer.WriteValue(array[i]);
        }

        writer.WriteEndArray();
    }

    private void ThrowReaderException()
    {
        throw new JsonReaderException(
            $"{typeof(T)} expects a number array of {_arrayLength} elements"
        );
    }
}

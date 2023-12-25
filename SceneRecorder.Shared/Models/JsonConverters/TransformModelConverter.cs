using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SceneRecorder.Shared.Models.JsonConverters;

internal sealed class TransformDTOConverter : JsonConverter<TransformDTO>
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    private static readonly Vector3Converter _Vector3Converter = new();

    private static readonly QuaternionConverter _QuaternionConverter = new();

    public override TransformDTO ReadJson(
        JsonReader reader,
        Type objectType,
        TransformDTO existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        reader.Read();

        var result = new TransformDTO
        {
            Position = _Vector3Converter.ReadJson(
                reader,
                typeof(Vector3),
                default,
                false,
                serializer
            ),
            Rotation = _QuaternionConverter.ReadJson(
                reader,
                typeof(Quaternion),
                default,
                false,
                serializer
            ),
            Scale = _Vector3Converter.ReadJson(reader, typeof(Vector3), default, false, serializer),
        };

        reader.Read();

        return result;
    }

    public override void WriteJson(
        JsonWriter writer,
        TransformDTO value,
        JsonSerializer serializer
    )
    {
        var (position, rotation, scale) = (value.Position, value.Rotation, value.Scale);

        writer.WriteStartArray();
        _Vector3Converter.WriteJson(writer, position, serializer);
        _QuaternionConverter.WriteJson(writer, rotation, serializer);
        _Vector3Converter.WriteJson(writer, scale, serializer);
        writer.WriteEndArray();
    }
}

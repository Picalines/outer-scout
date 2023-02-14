using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

internal sealed class TransformModelConverter : JsonConverter<TransformModel>
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    private static readonly Vector3Converter _Vector3Converter = new();

    private static readonly QuaternionConverter _QuaternionConverter = new();

    public override TransformModel ReadJson(JsonReader reader, Type objectType, TransformModel existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var baseArray = JArray.Load(reader).Values<JArray>().ToArray();

        var position = _Vector3Converter.ReadJson(baseArray[0]!.CreateReader(), typeof(Vector3), default, false, serializer);
        var rotation = _QuaternionConverter.ReadJson(baseArray[1]!.CreateReader(), typeof(Quaternion), default, false, serializer);
        var scale = _Vector3Converter.ReadJson(baseArray[2]!.CreateReader(), typeof(Vector3), default, false, serializer);

        return new TransformModel(position, rotation, scale);
    }

    public override void WriteJson(JsonWriter writer, TransformModel value, JsonSerializer serializer)
    {
        var (position, rotation, scale) = (value.Position, value.Rotation, value.Scale);

        writer.WriteStartArray();
        _Vector3Converter.WriteJson(writer, position, serializer);
        _QuaternionConverter.WriteJson(writer, rotation, serializer);
        _Vector3Converter.WriteJson(writer, scale, serializer);
        writer.WriteEndArray();
    }
}

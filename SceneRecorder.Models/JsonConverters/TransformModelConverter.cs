using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Models.JsonConverters;

internal sealed class TransformModelConverter : JsonConverter<TransformModel>
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    private static readonly float[] _PositionBuffer = new float[3];
    private static readonly float[] _RotationBuffer = new float[4];
    private static readonly float[] _ScaleBuffer = new float[3];

    public override TransformModel ReadJson(JsonReader reader, Type objectType, TransformModel existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        ReadToken(reader, JsonToken.StartArray);

        ReadFloatArray(reader, _PositionBuffer);
        ReadFloatArray(reader, _RotationBuffer);
        ReadFloatArray(reader, _ScaleBuffer);

        ReadToken(reader, JsonToken.EndArray);

        var transform = new TransformModel(
            Position: new Vector3(_PositionBuffer[0], _PositionBuffer[1], _PositionBuffer[2]),
            Rotation: new Quaternion(_RotationBuffer[0], _RotationBuffer[1], _RotationBuffer[2], _RotationBuffer[3]),
            Scale: new Vector3(_ScaleBuffer[0], _ScaleBuffer[1], _ScaleBuffer[2]));

        return transform;
    }

    public override void WriteJson(JsonWriter writer, TransformModel value, JsonSerializer serializer)
    {
        var (position, rotation, scale) = (value.Position, value.Rotation, value.Scale);

        var array = new JArray()
        {
            new JArray()
            {
                position.x,
                position.y,
                position.z,
            },

            new JArray()
            {
                rotation.x,
                rotation.y,
                rotation.z,
                rotation.w,
            },

            new JArray()
            {
                scale.x,
                scale.y,
                scale.z,
            },
        };

        array.WriteTo(writer);
    }

    private static void ReadToken(JsonReader reader, JsonToken expectedTokenType)
    {
        if (reader.Read() is false || reader.TokenType != expectedTokenType)
        {
            throw new JsonSerializationException($"expected {expectedTokenType} at {reader.Path}");
        }
    }

    private static void ReadFloatArray(JsonReader reader, float[] destination)
    {
        ReadToken(reader, JsonToken.StartArray);

        for (int i = 0; i < destination.Length; i++)
        {
            destination[i] = (float)(reader.ReadAsDouble()
                ?? throw new JsonSerializationException($"expected float at {reader.Path}"));
        }

        ReadToken(reader, JsonToken.EndArray);
    }
}

using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public abstract class VectorJsonConverter<TVector> : JsonConverter<TVector>
    where TVector : struct
{
    protected abstract int NumberOfAxes { get; }

    protected abstract double GetAxis(in TVector vector, int axisIndex);

    protected abstract void SetAxis(ref TVector vector, int axisIndex, double axisValue);

    public override TVector ReadJson(
        JsonReader reader,
        Type objectType,
        TVector existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        TVector vector = default;

        int axisIndex = 0;
        while (reader.Read())
        {
            if (reader.TokenType is JsonToken.EndArray)
            {
                break;
            }

            if (reader.TokenType is JsonToken.Float or JsonToken.Integer)
            {
                SetAxis(ref vector, axisIndex++, (double)(reader.Value ?? 0.0));
            }
        }

        return vector;
    }

    public sealed override void WriteJson(
        JsonWriter writer,
        TVector value,
        JsonSerializer serializer
    )
    {
        writer.WriteStartArray();

        for (int axisIndex = 0; axisIndex < NumberOfAxes; axisIndex++)
        {
            writer.WriteValue(GetAxis(value, axisIndex));
        }

        writer.WriteEndArray();
    }
}

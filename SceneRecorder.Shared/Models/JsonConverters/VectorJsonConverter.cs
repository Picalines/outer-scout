using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public abstract class VectorJsonConverter<TVector> : JsonConverter<TVector>
    where TVector : struct
{
    protected abstract int NumberOfAxes { get; }

    protected abstract double GetAxis(in TVector vector, int axisIndex);

    protected abstract void SetAxis(ref TVector vector, int axisIndex, double axisValue);

    public override TVector ReadJson(JsonReader reader, Type objectType, TVector existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        TVector vector = default;

        reader.Read();
        for (int i = 0; i < NumberOfAxes; i++)
        {
            var axisValue = reader.ReadAsDouble().GetValueOrDefault();
            SetAxis(ref vector, i, axisValue);
        }
        reader.Read();

        return vector;
    }

    public sealed override void WriteJson(JsonWriter writer, TVector value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        for (int i = 0; i < NumberOfAxes; i++)
        {
            writer.WriteValue(GetAxis(value, i));
        }
        writer.WriteEndArray();
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public sealed class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var vectorComponents = JArray.Load(reader).Values<float>().ToArray();
        return new Vector3(vectorComponents[0], vectorComponents[1], vectorComponents[2]);
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        new JArray() { value.x, value.y, value.z }.WriteTo(writer);
    }
}

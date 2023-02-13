using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public sealed class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var vectorComponents = JArray.Load(reader).Values<float>().ToArray();
        return new Vector2(vectorComponents[0], vectorComponents[1]);
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        new JArray() { value.x, value.y }.WriteTo(writer);
    }
}

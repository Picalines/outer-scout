using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public sealed class QuaternionConverter : JsonConverter<Quaternion>
{
    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var quaternionComponents = JArray.Load(reader).Values<float>().ToArray();
        return new Quaternion(quaternionComponents[0], quaternionComponents[1], quaternionComponents[2], quaternionComponents[3]);
    }

    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {
        new JArray() { value.x, value.y, value.z, value.w }.WriteTo(writer);
    }
}

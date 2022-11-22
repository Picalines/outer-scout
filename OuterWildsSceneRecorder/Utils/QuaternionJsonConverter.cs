using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Utils;

internal sealed class QuaternionJsonConverter : JsonConverter<Quaternion>
{
    public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
    {
        var array = new JArray() { value.x, value.y, value.z, value.w };
        array.WriteTo(writer);
    }
}

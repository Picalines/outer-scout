using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Utils.Json;

internal sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
    {
        var array = new JArray() { value.x, value.y, value.z };
        array.WriteTo(writer);
    }
}

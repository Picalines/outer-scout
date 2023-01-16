using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

using static Picalines.OuterWilds.SceneRecorder.Recorders.TransformRecorder;

namespace Picalines.OuterWilds.SceneRecorder.Json;

internal sealed class TransformDataConverter : JsonConverter<TransformData>
{
    public override TransformData ReadJson(JsonReader reader, Type objectType, TransformData existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override void WriteJson(JsonWriter writer, TransformData value, JsonSerializer serializer)
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
}

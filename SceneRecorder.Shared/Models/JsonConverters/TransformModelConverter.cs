using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

internal sealed class TransformModelConverter : JsonConverter<TransformModel>
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    public override TransformModel ReadJson(JsonReader reader, Type objectType, TransformModel existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var baseArray = JArray.Load(reader);

        var positionArray = ((JArray)baseArray.ElementAt(0)).Values<float>().ToArray();
        var rotationArray = ((JArray)baseArray.ElementAt(1)).Values<float>().ToArray();
        var scaleArray = ((JArray)baseArray.ElementAt(2)).Values<float>().ToArray();

        var transform = new TransformModel(
            Position: new Vector3(positionArray[0], positionArray[1], positionArray[2]),
            Rotation: new Quaternion(rotationArray[0], rotationArray[1], rotationArray[2], rotationArray[3]),
            Scale: new Vector3(scaleArray[0], scaleArray[1], scaleArray[2]));

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
}

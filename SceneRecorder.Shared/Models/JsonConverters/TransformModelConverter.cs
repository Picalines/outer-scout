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
        var baseArray = JArray.Load(reader).Values<JArray>().ToArray();

        var position = serializer.Deserialize<Vector3>(baseArray[0]!.CreateReader());
        var rotation = serializer.Deserialize<Quaternion>(baseArray[1]!.CreateReader());
        var scale = serializer.Deserialize<Vector3>(baseArray[2]!.CreateReader());

        return new TransformModel(position, rotation, scale);
    }

    public override void WriteJson(JsonWriter writer, TransformModel value, JsonSerializer serializer)
    {
        var (position, rotation, scale) = (value.Position, value.Rotation, value.Scale);

        var array = new JArray();

        var arrayWriter = array.CreateWriter();
        serializer.Serialize(arrayWriter, position);
        serializer.Serialize(arrayWriter, rotation);
        serializer.Serialize(arrayWriter, scale);

        array.WriteTo(writer);
    }
}

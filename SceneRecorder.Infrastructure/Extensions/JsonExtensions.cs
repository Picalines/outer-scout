using Newtonsoft.Json;

namespace SceneRecorder.Infrastructure.Extensions;

public static class JsonExtensions
{
    public static JsonConverterCollection Add(
        this JsonConverterCollection converters,
        IEnumerable<JsonConverter> convertersToAdd
    )
    {
        foreach (var converter in convertersToAdd)
        {
            converters.Add(converter);
        }

        return converters;
    }
}

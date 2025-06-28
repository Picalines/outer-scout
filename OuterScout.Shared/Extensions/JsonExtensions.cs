using Newtonsoft.Json;

namespace OuterScout.Shared.Extensions;

public static class JsonExtensions
{
    public static T? Deserialize<T>(this JsonSerializer jsonSerializer, TextReader reader)
    {
        using var jsonReader = new JsonTextReader(reader) { CloseInput = false };

        return jsonSerializer.Deserialize<T>(jsonReader);
    }

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

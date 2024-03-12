using Newtonsoft.Json;

namespace OuterScout.Infrastructure.Extensions;

public static class JsonExtensions
{
    public static T? Deserialize<T>(this JsonSerializer jsonSerializer, string str)
    {
        using var jsonReader = new JsonTextReader(new StringReader(str));

        return jsonSerializer.Deserialize<T>(jsonReader);
    }

    public static object? Deserialize(this JsonSerializer jsonSerializer, string str, Type type)
    {
        using var jsonReader = new JsonTextReader(new StringReader(str));

        return jsonSerializer.Deserialize(jsonReader, type);
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

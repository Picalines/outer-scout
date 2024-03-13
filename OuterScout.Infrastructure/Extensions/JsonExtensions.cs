using Newtonsoft.Json;

namespace OuterScout.Infrastructure.Extensions;

public static class JsonExtensions
{
    public static T? Deserialize<T>(this JsonSerializer jsonSerializer, TextReader reader)
    {
        using var jsonReader = new JsonTextReader(reader) { CloseInput = false };

        return jsonSerializer.Deserialize<T>(jsonReader);
    }

    public static T IgnoreMissingMembers<T>(this JsonSerializer jsonSerializer, Func<T> func)
    {
        lock (jsonSerializer)
        {
            var missingMemberHandling = jsonSerializer.MissingMemberHandling;
            jsonSerializer.MissingMemberHandling = MissingMemberHandling.Ignore;

            try
            {
                return func();
            }
            finally
            {
                jsonSerializer.MissingMemberHandling = missingMemberHandling;
            }
        }
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

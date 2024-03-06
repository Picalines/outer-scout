using Newtonsoft.Json;

namespace OuterScout.Infrastructure.Extensions;

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

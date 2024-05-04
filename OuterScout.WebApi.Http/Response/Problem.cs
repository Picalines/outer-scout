using Newtonsoft.Json;

namespace OuterScout.WebApi.Http.Response;

// https://datatracker.ietf.org/doc/html/rfc7807

public sealed class Problem
{
    public string Type { get; }

    public string? Title { get; init; }

    public string? Detail { get; init; }

    [JsonExtensionData]
    public Dictionary<string, object> Data { get; init; } = [];

    public Problem(string type)
    {
        Type = type;
    }
}

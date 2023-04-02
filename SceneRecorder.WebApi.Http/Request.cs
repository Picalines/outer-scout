using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public string Url { get; }

    public string Content { get; }

    internal Dictionary<string, string> MutableRouteParameters { get; } = new();

    internal Dictionary<string, string> MutableQueryParameters { get; } = new();

    internal Request(HttpMethod httpMethod, string url, string content)
    {
        HttpMethod = httpMethod;
        Url = url;
        Content = content;
    }

    public IReadOnlyDictionary<string, string> RouteParameters => MutableRouteParameters;

    public IReadOnlyDictionary<string, string> QueryParameters => MutableQueryParameters;

    public T ParseContentJson<T>()
    {
        return JsonConvert.DeserializeObject<T>(Content)!;
    }
}

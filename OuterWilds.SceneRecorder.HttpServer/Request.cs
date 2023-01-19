namespace OuterWilds.SceneRecorder.HttpServer;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public string Url { get; }

    private readonly Dictionary<string, string> _RouteParameters = new();

    private readonly Dictionary<string, string> _QueryParameters = new();

    internal Request(HttpMethod httpMethod, string url)
    {
        HttpMethod = httpMethod;
        Url = url;
    }

    internal void AddRouteParameter(string name, string value)
    {
        _RouteParameters[name] = value;
    }

    internal void AddQueryParameter(string name, string value)
    {
        _QueryParameters[name] = value;
    }

    public IReadOnlyDictionary<string, string> RouteParameters
    {
        get => _RouteParameters;
    }

    public IReadOnlyDictionary<string, string> QueryParameters
    {
        get => _QueryParameters;
    }
}

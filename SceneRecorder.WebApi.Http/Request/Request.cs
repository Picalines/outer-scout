namespace SceneRecorder.WebApi.Http;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public Uri Uri { get; }

    public TextReader BodyReader { get; }

    public IReadOnlyDictionary<string, string> RouteParameters { get; }

    public IReadOnlyDictionary<string, string> QueryParameters { get; }

    private Request(
        HttpMethod httpMethod,
        Uri uri,
        TextReader bodyReader,
        IReadOnlyDictionary<string, string> routeParameters,
        IReadOnlyDictionary<string, string> queryParameters
    )
    {
        HttpMethod = httpMethod;
        Uri = uri;
        BodyReader = bodyReader;
        RouteParameters = routeParameters;
        QueryParameters = queryParameters;
    }

    public sealed class Builder
    {
        private HttpMethod _httpMethod = HttpMethod.Get;

        private Uri _uri = new("about:blank");

        private TextReader _bodyReader = new StringReader("");

        private readonly Dictionary<string, string> _routeParameters = [];

        private readonly Dictionary<string, string> _queryParameters = [];

        public HttpMethod HttpMethod => _httpMethod;

        public Uri Uri => _uri;

        public Request Build()
        {
            return new Request(
                _httpMethod,
                _uri,
                _bodyReader,
                _routeParameters.ToDictionary(p => p.Key, p => p.Value),
                _queryParameters.ToDictionary(p => p.Key, p => p.Value)
            );
        }

        public Builder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public Builder WithUri(Uri uri)
        {
            _uri = uri;
            return this;
        }

        public Builder WithBodyReader(TextReader bodyReader)
        {
            _bodyReader = bodyReader;
            return this;
        }

        public Builder WithBody(string body)
        {
            return WithBodyReader(new StringReader(body));
        }

        public Builder WithRouteParameter(string key, string value)
        {
            _routeParameters[key] = value;
            return this;
        }

        public Builder WithQueryParameter(string key, string value)
        {
            _queryParameters[key] = value;
            return this;
        }
    }
}

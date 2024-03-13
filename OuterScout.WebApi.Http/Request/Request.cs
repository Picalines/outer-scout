using System.Web;
using OuterScout.Infrastructure.Extensions;

namespace OuterScout.WebApi.Http;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public StreamReader BodyReader { get; }

    public IReadOnlyList<string> Path { get; }

    public IReadOnlyDictionary<string, string> QueryParameters { get; }

    private Request(
        HttpMethod httpMethod,
        StreamReader bodyReader,
        IReadOnlyList<string> path,
        IReadOnlyDictionary<string, string> queryParameters
    )
    {
        HttpMethod = httpMethod;
        BodyReader = bodyReader;
        Path = path;
        QueryParameters = queryParameters;
    }

    public sealed class Builder
    {
        private HttpMethod _httpMethod = HttpMethod.Get;

        private StreamReader _bodyReader = StreamReader.Null;

        private readonly List<string> _path = [];

        private readonly Dictionary<string, string> _queryParameters = [];

        public HttpMethod HttpMethod => _httpMethod;

        public Request Build()
        {
            IEnumerable<string> path = _path.Count > 0 ? _path : [""];

            return new Request(
                _httpMethod,
                _bodyReader,
                path.ToArray(),
                _queryParameters.ToDictionary()
            );
        }

        public Builder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public Builder WithBodyReader(StreamReader bodyReader)
        {
            _bodyReader = bodyReader;
            return this;
        }

        public Builder WithPathPart(string value)
        {
            _path.Add(value);
            return this;
        }

        public Builder WithQueryParameter(string key, string value)
        {
            _queryParameters[key] = value;
            return this;
        }

        public Builder WithPathAndQuery(Uri uri)
        {
            _path.AddRange(uri.LocalPath.Trim('/').Split('/'));

            var queryParameters = HttpUtility.ParseQueryString(uri.Query);

            queryParameters
                .AllKeys.SelectMany(queryParameters.GetValues, (key, value) => new { key, value })
                .ForEach(pair => _queryParameters[pair.key] = pair.value);

            return this;
        }
    }
}

using System.Text.RegularExpressions;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed class Route
{
    public enum SegmentType
    {
        Constant,
        Parameter
    }

    public sealed record Segment(SegmentType Type, string Value)
    {
        public override string ToString()
        {
            return Type switch
            {
                SegmentType.Constant => Value,
                SegmentType.Parameter => ":" + Value,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public HttpMethod HttpMethod { get; }

    public IReadOnlyList<Segment> Segments { get; }

    public IReadOnlyDictionary<string, int> ParameterIndexes { get; }

    private static readonly Regex _StringSegmentRegex = new(@"^([a-zA-Z\-_]+)|(:[a-zA-Z_]+)$");

    private Route(
        HttpMethod httpMethod,
        IReadOnlyList<Segment> segments,
        IReadOnlyDictionary<string, int> parameterIndexes
    )
    {
        HttpMethod = httpMethod;
        Segments = segments;
        ParameterIndexes = parameterIndexes;
    }

    public IEnumerable<string> Parameters
    {
        get => ParameterIndexes.Keys;
    }

    public override string ToString()
    {
        return $"{HttpMethod.Method} {string.Join("/", Segments)}";
    }

    public sealed class Builder
    {
        private HttpMethod _httpMethod = HttpMethod.Get;

        private readonly List<Segment> _segments = [];

        private readonly Dictionary<string, int> _parameterIndexes = [];

        public Route Build()
        {
            return new Route(_httpMethod, _segments.ToArray(), _parameterIndexes.ToDictionary());
        }

        public Builder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public Builder WithConstantSegment(string value)
        {
            _segments.Add(new Segment(SegmentType.Constant, value));
            return this;
        }

        public Builder WithParameterSegment(string parameterName)
        {
            parameterName.Throw().If(_parameterIndexes.ContainsKey(parameterName));

            _parameterIndexes[parameterName] = _segments.Count;
            _segments.Add(new Segment(SegmentType.Parameter, parameterName));
            return this;
        }
    }

    public static Route? FromString(HttpMethod method, string str)
    {
        var builder = new Builder().WithHttpMethod(method);

        if (str is "")
        {
            return builder.Build();
        }

        var pathParts = str.Split('/');
        var parameterNames = new HashSet<string>();

        foreach (var pathPart in pathParts)
        {
            if (!_StringSegmentRegex.IsMatch(pathPart))
            {
                return null;
            }

            if (pathPart.StartsWith(":"))
            {
                var parameterName = pathPart.Substring(1);
                if (parameterNames.Add(parameterName) is false)
                {
                    return null;
                }

                builder.WithParameterSegment(parameterName);
            }
            else
            {
                builder.WithConstantSegment(pathPart);
            }
        }

        return builder.Build();
    }
}

using System.Text.RegularExpressions;

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

    private static readonly Regex _StringSegmentRegex = new(@"^([a-zA-Z\-_]+)|(:[a-zA-Z_]+)$");

    private Route(HttpMethod httpMethod, IReadOnlyList<Segment> segments)
    {
        HttpMethod = httpMethod;
        Segments = segments;
    }

    public IEnumerable<string> Parameters
    {
        get =>
            Segments
                .Where(segment => segment is { Type: SegmentType.Parameter })
                .Select(segment => segment.Value);
    }

    public override string ToString()
    {
        return $"{HttpMethod.Method} {string.Join("/", Segments)}";
    }

    public sealed class Builder
    {
        private HttpMethod _httpMethod = HttpMethod.Get;

        private readonly List<Segment> _segments = [];

        public Route Build()
        {
            return new Route(_httpMethod, _segments.ToArray());
        }

        public Builder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public Builder AddConstantSegment(string value)
        {
            _segments.Add(new Segment(SegmentType.Constant, value));
            return this;
        }

        public Builder AddParameterSegment(string parameterName)
        {
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

                builder.AddParameterSegment(parameterName);
            }
            else
            {
                builder.AddConstantSegment(pathPart);
            }
        }

        return builder.Build();
    }
}

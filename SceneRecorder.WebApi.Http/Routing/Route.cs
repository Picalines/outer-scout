using System.Text.RegularExpressions;

namespace OuterScout.WebApi.Http.Routing;

internal sealed partial class Route
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

    private static readonly Regex _stringSegmentRegex = new(@"^([a-zA-Z\-_]+)|(:[a-zA-Z_]+)$");

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
            if (!_stringSegmentRegex.IsMatch(pathPart))
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

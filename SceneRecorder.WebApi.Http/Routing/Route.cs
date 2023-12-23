using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed record Route(HttpMethod HttpMethod, IReadOnlyList<Route.Segment> Segments)
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

    private static readonly Regex _StringSegmentRegex = new("^:?\\w+$");

    public static bool TryFromString(
        HttpMethod method,
        string str,
        [NotNullWhen(true)] out Route? route
    )
    {
        if (str == "")
        {
            route = new Route(method, Array.Empty<Segment>());
            return true;
        }

        route = null;

        var strSegments = str.Split('/');
        var segments = new List<Segment>();
        var parameterNames = new HashSet<string>();

        foreach (var strSegment in strSegments)
        {
            if (!_StringSegmentRegex.IsMatch(strSegment))
            {
                return false;
            }

            Segment segment;

            bool isParameter = strSegment.StartsWith(":");
            if (isParameter)
            {
                var parameterName = strSegment.Substring(1);
                if (parameterNames.Add(parameterName) is false)
                {
                    return false;
                }

                segment = new(SegmentType.Parameter, parameterName);
            }
            else
            {
                segment = new(SegmentType.Constant, strSegment);
            }

            segments.Add(segment);
        }

        route = new Route(method, segments.ToArray());
        return true;
    }

    public IEnumerable<string> Parameters =>
        Segments
            .Where(segment => segment is { Type: SegmentType.Parameter })
            .Select(segment => segment.Value);

    public override string ToString()
    {
        return $"{HttpMethod.Method} {string.Join("/", Segments)}";
    }
}

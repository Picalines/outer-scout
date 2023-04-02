using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed record Route(HttpMethod HttpMethod, IReadOnlyList<Route.Segment> Segments)
{
    public enum SegmentType
    {
        Plain, Parameter
    }

    public sealed record Segment(SegmentType Type, string Value)
    {
        public override string ToString()
        {
            return Type switch
            {
                SegmentType.Plain => Value,
                SegmentType.Parameter => ":" + Value,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public override string ToString()
    {
        return string.Join("/", Segments);
    }

    private static readonly Regex _StringSegmentRegex = new("^:?[a-z]+$");

    public static bool TryFromString(HttpMethod method, string str, [NotNullWhen(true)] out Route? route)
    {
        route = null;

        var strSegments = str.Split('/');
        var segments = new List<Segment>();

        foreach (var strSegment in strSegments)
        {
            if (!_StringSegmentRegex.IsMatch(strSegment))
            {
                return false;
            }

            bool isParameter = strSegment.StartsWith(":");

            segments.Add(isParameter
                ? new(SegmentType.Parameter, strSegment.Substring(1))
                : new(SegmentType.Plain, strSegment));
        }

        route = new Route(method, segments.ToArray());
        return true;
    }
}

using System.Text.RegularExpressions;

namespace OuterWilds.SceneRecorder.HttpServer;

internal sealed record Route(
    HttpMethod HttpMethod,
    IReadOnlyList<Route.Segment> Segments)
{
    public abstract record Segment;

    public sealed record PlainSegment(string Value) : Segment;

    public sealed record ParameterSegment(string ParameterName, Regex? Regex) : Segment;

    private static readonly Regex _ParameterSegmentSyntaxRegex = new(@"\{(?<name>\w+)(:(?<regex>.*?))?\}");

    public static IReadOnlyList<Segment> ParsePathString(string pathString)
    {
        if (pathString.StartsWith("/"))
        {
            throw new ArgumentException("must not start with /", nameof(pathString));
        }

        return pathString.Split('/')
            .Select<string, Segment>((pathPart, index) =>
            {
                if (index > 0 && pathPart.Length is 0)
                {
                    throw new ArgumentException($"invalid route segment. Must not be empty");
                }

                if ((pathPart.Contains('{') || pathPart.Contains('}')) is false)
                {
                    return new PlainSegment(pathPart);
                }

                if (_ParameterSegmentSyntaxRegex.Match(pathPart) is not { Success: true } match)
                {
                    throw new ArgumentException($"invalid route parameter segment. Must match regex: {_ParameterSegmentSyntaxRegex}", nameof(pathString));
                }

                var matchRegexGroup = match.Groups["regex"];

                return new ParameterSegment(
                    match.Groups["name"].Value,
                    matchRegexGroup.Value.Length > 0 ? new Regex(matchRegexGroup.Value) : null);
            })
            .ToArray();
    }

    public bool TrySetRequestParameters(Request request)
    {
        var queryParts = request.Url.Split('&');
        var urlRouteParts = queryParts[0].Split('/');

        if (Segments.Count != urlRouteParts.Length)
        {
            return false;
        }

        var routeParameters = new Dictionary<string, string>();

        for (int i = 0; i < urlRouteParts.Length; i++)
        {
            var urlPart = urlRouteParts[i];
            var segment = Segments[i];

            switch (segment)
            {
                case PlainSegment plainSegment:
                {
                    if (plainSegment.Value != urlPart)
                    {
                        return false;
                    }
                }
                break;

                case ParameterSegment parameterSegment:
                {
                    if (parameterSegment.Regex?.Match(urlPart) is { Success: false })
                    {
                        return false;
                    }

                    routeParameters[parameterSegment.ParameterName] = urlPart;
                }
                break;

                default:
                    throw new NotImplementedException();
            }
        }

        foreach (var (name, value) in routeParameters)
        {
            request.AddRouteParameter(name, value);
        }

        for (int i = 1; i < queryParts.Length; i++)
        {
            var queryArgumentParts = queryParts[i].Split(new char[] { '=' }, 2);
            request.AddQueryParameter(queryArgumentParts[0], queryArgumentParts[1]);
        }

        return true;
    }
}

using System.Text.RegularExpressions;

namespace Picalines.OuterWilds.SceneRecorder.Http;

internal sealed partial record Route(
    HttpMethod HttpMethod,
    IReadOnlyList<Route.Segment> Segments)
{
    private static readonly Regex _ParameterSegmentSyntaxRegex = new(@"\{(?<name>\w+):(?<type>.*?)\}");

    public static IReadOnlyList<Segment> ParsePathString(string pathString)
    {
        if (pathString.StartsWith("/"))
        {
            throw new ArgumentException("must not start with /", nameof(pathString));
        }

        return GetUrlParts(pathString)
            .Select((part, index) => PathPartToSegment(part.Value, index, part.IsQuery
                ? ParameterSegmentType.Query : ParameterSegmentType.Path))
            .ToArray();

        static Segment PathPartToSegment(string pathPart, int partIndex, ParameterSegmentType? parameterType)
        {
            if (partIndex > 0 && pathPart.Length is 0)
            {
                throw new ArgumentException($"invalid route segment at index {partIndex}: must not be empty", nameof(pathString));
            }

            if ((pathPart.Contains('{') || pathPart.Contains('}')) is false)
            {
                return new PlainSegment(pathPart);
            }

            if (_ParameterSegmentSyntaxRegex.Match(pathPart) is not { Success: true } match)
            {
                throw new ArgumentException($"invalid route segment at index {partIndex}: parameter must match regex: {_ParameterSegmentSyntaxRegex}", nameof(pathString));
            }

            var parameterName = match.Groups["name"].Value;

            return match.Groups["type"].Value switch
            {
                "bool" => new BoolParameterSegment(parameterName, parameterType.GetValueOrDefault()),
                "int" => new IntParameterSegment(parameterName, parameterType.GetValueOrDefault()),
                "float" => new FloatParameterSegment(parameterName, parameterType.GetValueOrDefault()),
                "string" => new StringParameterSegment(parameterName, parameterType.GetValueOrDefault()),
                var type => throw new ArgumentException($"invalid route segment at index {partIndex}: type '{type}' is not supported")
            };
        }
    }

    public bool TrySetRequestParameters(Request request)
    {
        var urlRouteParts = GetUrlParts(request.Url).ToArray();

        if (Segments.Count != urlRouteParts.Length)
        {
            return false;
        }

        var routeParameters = new Dictionary<string, object?>();
        var queryParameters = new Dictionary<string, object?>();

        for (int i = 0; i < urlRouteParts.Length; i++)
        {
            var (urlPart, isQuery) = urlRouteParts[i];
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
                    if ((parameterSegment.Type, isQuery) is (not ParameterSegmentType.Query, true))
                    {
                        return false;
                    }

                    if (isQuery)
                    {
                        var queryParts = urlPart.Split('=');
                        if (queryParts[0] != parameterSegment.ParameterName)
                        {
                            return false;
                        }

                        urlPart = queryParts[1];
                    }

                    if (parameterSegment.TryParseValue(urlPart, out var parameterValue) is false)
                    {
                        return false;
                    }

                    (isQuery ? queryParameters : routeParameters)[parameterSegment.ParameterName] = parameterValue;
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

        foreach (var (name, value) in queryParameters)
        {
            request.AddQueryParameter(name, value);
        }

        return true;
    }

    private static IEnumerable<(string Value, bool IsQuery)> GetUrlParts(string url)
    {
        int firstQueryParameterIndex = url.Count(chr => chr == '/') + 1;

        return url.Split('/', '&')
            .Select((pathPart, partIndex) => (pathPart, partIndex >= firstQueryParameterIndex));
    }
}

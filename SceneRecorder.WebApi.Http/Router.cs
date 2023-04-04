namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed class Router
{
    private sealed class RouteNode
    {
        public RequestHandler? Handler { get; set; }

        public Dictionary<string, RouteNode> PlainChildren { get; } = new();

        public (string ParameterName, RouteNode ChildNode)? ParameterChild { get; set; }
    }

    private readonly RouteNode _RouteTreeRoot;

    public Router(IReadOnlyList<RequestHandler> requestHandlers)
    {
        _RouteTreeRoot = CreateRouteTree(requestHandlers);
    }

    public RequestHandler? Match(Request request)
    {
        request.MutableRouteParameters.Clear();

        var urlRouteSegments = request.Uri.LocalPath.Split('/');

        if (urlRouteSegments.Length is 0)
        {
            return _RouteTreeRoot.Handler;
        }

        var currentNode = _RouteTreeRoot;

        foreach (var (urlRouteSegment, _, isLast) in EnumerateWithEndpointFlags(urlRouteSegments))
        {
            if (currentNode.PlainChildren.ContainsKey(urlRouteSegment))
            {
                currentNode = currentNode.PlainChildren[urlRouteSegment];
            }
            else if (currentNode.ParameterChild is { ChildNode: var parameterChild, ParameterName: var parameterName })
            {
                currentNode = parameterChild;
                request.MutableRouteParameters[parameterName] = urlRouteSegment;
            }

            if (isLast && currentNode.Handler is { } requestHandler)
            {
                return requestHandler;
            }
        }

        return null;
    }

    private RouteNode CreateRouteTree(IReadOnlyList<RequestHandler> requestHandlers)
    {
        var root = new RouteNode();

        foreach (var requestHandler in requestHandlers)
        {
            var route = requestHandler.Route;
            var currentNode = root;

            foreach (var (routeSegment, _, isLast) in EnumerateWithEndpointFlags(route.Segments))
            {
                switch (routeSegment.Type)
                {
                    case Route.SegmentType.Plain:
                        var plainSegment = routeSegment.Value;

                        currentNode = currentNode.PlainChildren.ContainsKey(plainSegment)
                            ? currentNode.PlainChildren[plainSegment]
                            : (currentNode.PlainChildren[plainSegment] = new RouteNode());
                        break;

                    case Route.SegmentType.Parameter:
                        if (currentNode.ParameterChild is not null)
                        {
                            throw new InvalidOperationException($"route {route} is ambiguous at parameter segment {routeSegment}");
                        }

                        var childNode = new RouteNode();
                        currentNode.ParameterChild = (routeSegment.Value, childNode);
                        currentNode = childNode;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                if (isLast)
                {
                    currentNode.Handler = requestHandler;
                }
            }

            if (route.Segments.Count is 0)
            {
                if (root.Handler is not null)
                {
                    throw new InvalidOperationException("index route is ambiguous");
                }

                root.Handler = requestHandler;
            }
        }

        return root;
    }

    private IEnumerable<(T Element, bool IsFirst, bool IsLast)> EnumerateWithEndpointFlags<T>(IReadOnlyList<T> list)
    {
        int index = 0;
        var lastIndex = list.Count - 1;
        foreach (var element in list)
        {
            yield return (element, index == 0, index == lastIndex);
            index++;
        }
    }
}

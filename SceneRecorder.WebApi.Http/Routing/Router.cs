using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed partial class Router
{
    private sealed record RouteTreeLeaf(Route Route, IRequestHandler Handler);

    private sealed class RouteTreeNode
    {
        public Dictionary<HttpMethod, RouteTreeLeaf> Leaves { get; } = [];

        public Dictionary<string, RouteTreeNode> PlainChildren { get; } = [];

        public RouteTreeNode? ParameterChild { get; set; }
    }

    private readonly RouteTreeNode _routeTreeRoot;

    private Router(RouteTreeNode routeTreeRoot)
    {
        _routeTreeRoot = routeTreeRoot;
    }

    public (Route Route, IRequestHandler Handler)? Match(Request request)
    {
        var httpMethod = request.HttpMethod;

        var currentNode = _routeTreeRoot;

        foreach (var (pathPart, isLast) in request.Path.WithIsLast())
        {
            if (currentNode.PlainChildren.ContainsKey(pathPart))
            {
                currentNode = currentNode.PlainChildren[pathPart];
            }
            else if (currentNode.ParameterChild is { } parameterChild)
            {
                currentNode = parameterChild;
            }

            if (isLast)
            {
                if (currentNode.Leaves.TryGetValue(httpMethod, out var leaf) is false)
                {
                    return null;
                }

                var (route, requestHandler) = leaf;

                return (route, requestHandler);
            }
        }

        return null;
    }
}

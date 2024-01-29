using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed partial class Router
{
    private sealed record RouteTreeLeaf(Route Route, IRequestHandler Handler);

    private sealed class RouteTreeNode
    {
        public Dictionary<HttpMethod, RouteTreeLeaf> Leaves { get; } = [];

        public Dictionary<string, RouteTreeNode> PlainChildren { get; } = [];

        public (string ParameterName, RouteTreeNode ChildNode)? ParameterChild { get; set; }
    }

    private readonly RouteTreeNode _routeTreeRoot;

    private Router(RouteTreeNode routeTreeRoot)
    {
        _routeTreeRoot = routeTreeRoot;
    }

    public (Route Route, IRequestHandler Handler)? Match(Request.Builder requestBuilder)
    {
        var httpMethod = requestBuilder.HttpMethod;

        var currentNode = _routeTreeRoot;

        var urlRouteSegments = requestBuilder.Uri.LocalPath.Trim('/').Split('/');

        foreach (var (urlRouteSegment, isLast) in urlRouteSegments.WithIsLast())
        {
            if (currentNode.PlainChildren.ContainsKey(urlRouteSegment))
            {
                currentNode = currentNode.PlainChildren[urlRouteSegment];
            }
            else if (
                urlRouteSegment.Length > 0
                && currentNode.ParameterChild
                    is { ChildNode: var parameterChild, ParameterName: var parameterName }
            )
            {
                currentNode = parameterChild;
                requestBuilder.WithRouteParameter(parameterName, urlRouteSegment);
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

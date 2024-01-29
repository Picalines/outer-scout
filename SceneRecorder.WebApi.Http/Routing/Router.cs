using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed class Router
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

    public sealed class Builder
    {
        private readonly RouteTreeNode _root = new();

        private bool _built = false;

        public Router Build()
        {
            if (_built)
            {
                throw new InvalidOperationException($"{nameof(Build)} called twice");
            }

            _built = true;

            return new Router(_root);
        }

        public Builder WithRoute(Route route, IRequestHandler requestHandler)
        {
            var currentNode = _root;

            foreach (var (routeSegment, isLast) in route.Segments.WithIsLast())
            {
                switch (routeSegment.Type)
                {
                    case Route.SegmentType.Constant:
                        var plainSegment = routeSegment.Value;

                        currentNode = currentNode.PlainChildren.ContainsKey(plainSegment)
                            ? currentNode.PlainChildren[plainSegment]
                            : (currentNode.PlainChildren[plainSegment] = new RouteTreeNode());
                        break;

                    case Route.SegmentType.Parameter:
                        if (currentNode.ParameterChild is not (_, { } childNode))
                        {
                            childNode = new RouteTreeNode();
                            currentNode.ParameterChild = (routeSegment.Value, childNode);
                        }

                        currentNode = childNode;
                        break;

                    default:
                        throw new NotImplementedException();
                }

                if (isLast)
                {
                    if (currentNode.Leaves.ContainsKey(route.HttpMethod))
                    {
                        throw new InvalidOperationException(
                            $"{route.HttpMethod.Method} {route} route is ambiguous"
                        );
                    }

                    currentNode.Leaves[route.HttpMethod] = new(route, requestHandler);
                }
            }

            if (route.Segments.Count is 0)
            {
                if (_root.Leaves.ContainsKey(route.HttpMethod))
                {
                    throw new InvalidOperationException(
                        $"index {route.HttpMethod.Method} route is ambiguous"
                    );
                }

                _root.Leaves[route.HttpMethod] = new(route, requestHandler);
            }

            return this;
        }
    }
}

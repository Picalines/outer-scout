using OuterScout.Infrastructure.Extensions;

namespace OuterScout.WebApi.Http.Routing;

internal sealed partial class Router<T>
{
    public sealed class Builder
    {
        private readonly RouteTreeNode _root = new();

        private bool _built = false;

        public Router<T> Build()
        {
            if (_built)
            {
                throw new InvalidOperationException($"{nameof(Build)} called twice");
            }

            _built = true;

            return new Router<T>(_root);
        }

        public Builder WithRoute(Route route, T value)
        {
            var currentNode = _root;

            foreach (var (routeSegment, isLast) in route.Segments.WithIsLast())
            {
                currentNode = routeSegment switch
                {
                    { Type: Route.SegmentType.Constant, Value: var pathPart }
                        => currentNode.PlainChildren.ContainsKey(pathPart)
                            ? currentNode.PlainChildren[pathPart]
                            : (currentNode.PlainChildren[pathPart] = new RouteTreeNode()),

                    { Type: Route.SegmentType.Parameter }
                        => currentNode.ParameterChild ??= new RouteTreeNode(),

                    _ => throw new NotImplementedException(),
                };

                if (isLast)
                {
                    if (currentNode.Leaves.ContainsKey(route.HttpMethod))
                    {
                        throw new InvalidOperationException(
                            $"{route.HttpMethod.Method} {route} route is ambiguous"
                        );
                    }

                    currentNode.Leaves[route.HttpMethod] = new(route, value);
                    return this;
                }
            }

            throw new InvalidOperationException(
                $"failed to add route {route} to {nameof(Router<T>)}"
            );
        }
    }
}

namespace OuterScout.Shared.Extensions;

public sealed class UsingStack<T>
{
    private readonly Stack<Node> _nodes = [];

    public IDisposable Use(T value)
    {
        var node = new Node(this, value);

        _nodes.Push(node);

        return node;
    }

    public IEnumerable<T> Values
    {
        get => _nodes.Select(node => node.Value);
    }

    public T? Top
    {
        get => _nodes.TryPeek(out var top) ? top.Value : default;
    }

    private sealed record Node(UsingStack<T> Stack, T Value) : IDisposable
    {
        private bool _disposed = false;

        void IDisposable.Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            while (Stack._nodes.TryPop(out var top) && ReferenceEquals(this, top) is false)
            {
                top._disposed = true;
            }
        }
    }
}

namespace OuterScout.Infrastructure.DependencyInjection;

public interface IServiceContainer : IDisposable
{
    public bool Contains(Type type);

    public object? ResolveOrNull(Type type);

    public object Resolve(Type type);

    public IEnumerable<object> ResolveAll(Type type);

    public bool Contains<T>()
        where T : class;

    public T? ResolveOrNull<T>()
        where T : class;

    public T Resolve<T>()
        where T : class;

    public IEnumerable<T> ResolveAll<T>()
        where T : class;
}

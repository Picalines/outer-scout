namespace OuterScout.Shared.DependencyInjection;

public sealed partial class ServiceContainer : IServiceScope
{
    private readonly Scope _containerScope;

    private ServiceContainer(ScopeRegistry scopeRegistry, ServiceRegistry globalServices)
    {
        _containerScope = new Scope(scopeRegistry, globalServices);
    }

    public bool Contains(Type type)
    {
        return _containerScope.Contains(type);
    }

    public bool Contains<T>()
        where T : class
    {
        return _containerScope.Contains<T>();
    }

    public IEnumerable<object> ResolveAll(Type type)
    {
        return _containerScope.ResolveAll(type);
    }

    public object Resolve(Type type)
    {
        return _containerScope.Resolve(type);
    }

    public IEnumerable<T> ResolveAll<T>()
        where T : class
    {
        return _containerScope.ResolveAll<T>();
    }

    public T Resolve<T>()
        where T : class
    {
        return _containerScope.Resolve<T>();
    }

    public object? ResolveOrNull(Type type)
    {
        return _containerScope.ResolveOrNull(type);
    }

    public T? ResolveOrNull<T>()
        where T : class
    {
        return _containerScope.ResolveOrNull<T>();
    }

    public IServiceScope StartScope(string identifier)
    {
        return _containerScope.StartScope(identifier);
    }

    public void Dispose()
    {
        _containerScope.Dispose();
    }
}

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer : ServiceContainer.IScope
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

    public object Resolve(Type type)
    {
        return _containerScope.Resolve(type);
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

    public IScope StartScope(string identifier)
    {
        return _containerScope.StartScope(identifier);
    }

    public void Dispose()
    {
        _containerScope.Dispose();
    }
}

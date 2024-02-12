namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer : IDisposable
{
    private readonly IReadOnlyDictionary<Type, IEnumerable<ILifetimeManager<object>>> _services;

    private bool _disposed = false;

    private ServiceContainer(
        IReadOnlyDictionary<Type, IEnumerable<ILifetimeManager<object>>> services
    )
    {
        _services = services;
    }

    public object? ResolveOrNull(Type serviceType)
    {
        return ResolveFirstService(serviceType)?.GetInstance();
    }

    public object Resolve(Type serviceType)
    {
        return ResolveOrNull(serviceType)
            ?? throw new InvalidOperationException(
                $"{nameof(ServiceContainer)} does not contain service of type {serviceType}"
            );
    }

    public T? ResolveOrNull<T>()
        where T : class
    {
        return ResolveOrNull(typeof(T)) as T;
    }

    public T Resolve<T>()
        where T : class
    {
        return (Resolve(typeof(T)) as T)!;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var serviceList in _services.Values)
        {
            foreach (var service in serviceList.AsEnumerable().Reverse())
            {
                (service as IDisposable)?.Dispose();
            }
        }
    }

    private ILifetimeManager<object>? ResolveFirstService(Type type)
    {
        AssertNotDisposed();

        return _services.TryGetValue(type, out var serviceList)
            ? serviceList.FirstOrDefault()
            : null;
    }

    private void AssertNotDisposed()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(ServiceContainer)} is disposed");
        }
    }
}

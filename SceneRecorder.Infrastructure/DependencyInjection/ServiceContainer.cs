namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, LinkedList<IService<object>>> _services = [];

    private bool _disposed = false;

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

    public IDisposable RegisterService<T>(IService<T> service)
        where T : class
    {
        AssertNotDisposed();

        var serviceList = GetOrCreateServiceList<T>();
        serviceList.AddFirst(service);
        return new ServiceDisposer(serviceList, service);
    }

    public IDisposable RegisterFallback<T>(IService<T> service)
        where T : class
    {
        AssertNotDisposed();

        var serviceList = GetOrCreateServiceList<T>();
        serviceList.AddLast(service);
        return new ServiceDisposer(serviceList, service);
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

            serviceList.Clear();
        }
    }

    private IService<object>? ResolveFirstService(Type type)
    {
        AssertNotDisposed();

        return _services.TryGetValue(type, out var serviceList)
            ? serviceList.FirstOrDefault()
            : null;
    }

    private LinkedList<IService<object>> GetOrCreateServiceList<T>()
    {
        var serviceType = typeof(T);
        if (_services.TryGetValue(serviceType, out var serviceList) is false)
        {
            _services[serviceType] = serviceList = [];
        }

        return serviceList;
    }

    private void AssertNotDisposed()
    {
        if (_disposed)
        {
            throw new InvalidOperationException($"{nameof(ServiceContainer)} is disposed");
        }
    }

    private sealed class ServiceDisposer(
        LinkedList<IService<object>> serviceList,
        IService<object> service
    ) : IDisposable
    {
        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            serviceList.Remove(service);

            (service as IDisposable)?.Dispose();
        }
    }
}

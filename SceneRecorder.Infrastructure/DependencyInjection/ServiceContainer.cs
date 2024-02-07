namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, List<IService<object>>> _services = [];

    private bool _disposed = false;

    public object? Resolve(Type serviceType)
    {
        return ResolveLastService(serviceType)?.GetInstance();
    }

    public T? Resolve<T>()
        where T : class
    {
        return (ResolveLastService(typeof(T)) as IService<T>)?.GetInstance();
    }

    public IDisposable RegisterService<T>(IService<T> service)
        where T : class
    {
        AssertNotDisposed();

        var serviceList = GetOrCreateServiceList<T>();
        serviceList.Add(service);
        return new ServiceDisposer(serviceList, service);
    }

    public IDisposable RegisterServiceFallback<T>(IService<T> service)
        where T : class
    {
        AssertNotDisposed();

        var serviceList = GetOrCreateServiceList<T>();
        serviceList.Insert(0, service);
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

    private IService<object>? ResolveLastService(Type type)
    {
        AssertNotDisposed();

        return _services.TryGetValue(type, out var serviceList)
            ? serviceList.LastOrDefault()
            : null;
    }

    private List<IService<object>> GetOrCreateServiceList<T>()
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
        IList<IService<object>> serviceList,
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

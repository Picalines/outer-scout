namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, List<IService<object>>> _services = [];

    private bool _disposed = false;

    public ServiceContainer()
    {
        RegisterInstance(this);
    }

    public object? Resolve(Type serviceType)
    {
        return ResolveLastService(serviceType)?.GetInstance();
    }

    public T? Resolve<T>()
        where T : class
    {
        return (ResolveLastService(typeof(T)) as IService<T>)?.GetInstance();
    }

    public IDisposable RegisterInstance<T>(T instance)
        where T : class
    {
        return RegisterService(new SingletonService<T>(instance));
    }

    public IDisposable RegisterFactory<T>(Func<T> instanceFactory)
        where T : class
    {
        return RegisterService(new FactoryService<T>(instanceFactory));
    }

    public IDisposable RegisterLazy<T>(Lazy<T> lazyInstance)
        where T : class
    {
        return RegisterService(new LazyService<T>(lazyInstance));
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

    private IDisposable RegisterService<T>(IService<T> service)
        where T : class
    {
        AssertNotDisposed();

        var serviceType = typeof(T);

        if (_services.TryGetValue(serviceType, out var serviceList) is false)
        {
            _services[serviceType] = serviceList = [];
        }

        serviceList.Add(service);

        return new ServiceDisposer(serviceList, service);
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
        }
    }
}

internal interface IService<out T>
    where T : class
{
    public T GetInstance();
}

internal sealed class SingletonService<T>(T instance) : IService<T>, IDisposable
    where T : class
{
    public T GetInstance()
    {
        return instance;
    }

    public void Dispose()
    {
        (instance as IDisposable)?.Dispose();
    }
}

internal sealed class FactoryService<T>(Func<T> instanceFactory) : IService<T>
    where T : class
{
    public T GetInstance()
    {
        return instanceFactory.Invoke();
    }
}

internal sealed class LazyService<T>(Lazy<T> lazyInstance) : IService<T>, IDisposable
    where T : class
{
    public T GetInstance()
    {
        return lazyInstance.Value;
    }

    public void Dispose()
    {
        if (lazyInstance.IsValueCreated)
        {
            (lazyInstance.Value as IDisposable)?.Dispose();
        }
    }
}

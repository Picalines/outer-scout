namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed class ServiceContainer
{
    private readonly Dictionary<Type, IService<object>> _services = [];

    public object? Resolve(Type serviceType)
    {
        return _services.TryGetValue(serviceType, out var service) ? service.GetInstance() : null;
    }

    public T? Resolve<T>()
        where T : class
    {
        return
            _services.TryGetValue(typeof(T), out var anyService)
            && anyService is IService<T> serviceOfT
            ? serviceOfT.GetInstance()
            : null;
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

    private IDisposable RegisterService<T>(IService<T> service)
        where T : class
    {
        var serviceType = typeof(T);

        if (_services.ContainsKey(serviceType))
        {
            throw new InvalidOperationException(
                $"{nameof(ServiceContainer)} already contains service for type {serviceType.FullName}"
            );
        }

        _services[serviceType] = service;

        return new ServiceDisposer(this, serviceType);
    }

    private sealed class ServiceDisposer(ServiceContainer container, Type serviceType) : IDisposable
    {
        private bool _disposed = false;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            container._services.Remove(serviceType);
        }
    }
}

internal interface IService<out T>
    where T : class
{
    public T GetInstance();
}

internal sealed class SingletonService<T>(T instance) : IService<T>
    where T : class
{
    public T GetInstance()
    {
        return instance;
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

internal sealed class LazyService<T>(Lazy<T> lazyInstance) : IService<T>
    where T : class
{
    public T GetInstance()
    {
        return lazyInstance.Value;
    }
}

namespace SceneRecorder.WebApi.Http.DependencyInjection;

public sealed class ServiceCollection
{
    private interface IService { }

    private interface IService<T> : IService
    {
        public T Instance { get; }
    }

    private readonly Dictionary<Type, IService> _services = [];

    public void AddService<T>(T instance)
    {
        AddService(new SingletonService<T>(instance));
    }

    public void AddService<T>(Func<T> instanceFactory)
    {
        AddService(new FactoryService<T>(instanceFactory));
    }

    public T ResolveInstance<T>()
    {
        var serviceType = typeof(T);

        if (
            _services.TryGetValue(serviceType, out var service) is false
            || service is not IService<T> serviceOfT
        )
        {
            throw new InvalidOperationException(
                $"failed to resolve instance of service {serviceType.Name}"
            );
        }

        return serviceOfT.Instance;
    }

    private void AddService(Type type, IService service)
    {
        if (_services.ContainsKey(type))
        {
            throw new InvalidOperationException(
                $"{nameof(ServiceCollection)} already contains {type.Name} service"
            );
        }

        _services[type] = service;
    }

    private sealed class SingletonService<T>(T instance) : IService<T>
    {
        public T Instance { get; } = instance;
    }

    private sealed class FactoryService<T>(Func<T> instanceFactory) : IService<T>
    {
        public T Instance => instanceFactory();
    }
}

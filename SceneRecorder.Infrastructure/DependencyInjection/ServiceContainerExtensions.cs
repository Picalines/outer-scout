namespace SceneRecorder.Infrastructure.DependencyInjection;

public static class ServiceContainerExtensions
{
    public static IDisposable RegisterInstance<T>(this ServiceContainer services, T instance)
        where T : class
    {
        return services.RegisterService(new SingletonService<T>(instance));
    }

    public static IDisposable RegisterFallbackInstance<T>(
        this ServiceContainer services,
        T instance
    )
        where T : class
    {
        return services.RegisterFallback(new SingletonService<T>(instance));
    }

    public static IDisposable RegisterFactory<T>(
        this ServiceContainer services,
        Func<T> instanceFactory
    )
        where T : class
    {
        return services.RegisterService(new FactoryService<T>(instanceFactory));
    }

    public static IDisposable RegisterLazy<T>(this ServiceContainer services, Lazy<T> lazyInstance)
        where T : class
    {
        return services.RegisterService(new LazyService<T>(lazyInstance));
    }

    public static IDisposable RegisterLazy<T>(
        this ServiceContainer services,
        Func<T> instanceFactory
    )
        where T : class
    {
        return services.RegisterLazy(new Lazy<T>(instanceFactory));
    }
}

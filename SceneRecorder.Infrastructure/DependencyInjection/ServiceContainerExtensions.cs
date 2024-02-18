namespace SceneRecorder.Infrastructure.DependencyInjection;

using SceneRecorder.Infrastructure.Validation;
using static ServiceContainer;

public static class ServiceContainerExtensions
{
    public static IRegistration<T> AsSingleton<T>(this IRegistration<T> registration)
        where T : class
    {
        return registration.ManageBy(new SingletonLifetime<T>());
    }

    public static IRegistration<T> AsExternalReference<T>(
        this IRegistration<T> registration,
        T reference
    )
        where T : class
    {
        return registration.ManageBy(new ReferenceLifetime<T>(reference));
    }

    public static IRegistration<T> InstantiatePerResolve<T>(this IRegistration<T> registration)
        where T : class
    {
        return registration.ManageBy(new PerResolveLifetime<T>());
    }

    public static IRegistration<T> InstantiateBy<T>(
        this IRegistration<T> registration,
        Func<T> factory
    )
        where T : class
    {
        return registration.InstantiateBy(new LambdaInstantiator<T>(factory));
    }

    public static IRegistration<T> InstantiatePerResolve<T>(
        this IRegistration<T> registration,
        Func<T> factory
    )
        where T : class
    {
        return registration.InstantiatePerResolve().InstantiateBy(factory);
    }

    private sealed class PerResolveLifetime<T> : ILifetime<T>, IStartupHandler
        where T : class
    {
        private IInstantiator<T>? _instantiator;

        public T GetInstance()
        {
            _instantiator.ThrowIfNull();
            return _instantiator.Instantiate();
        }

        void IStartupHandler.OnContainerStartup(ServiceContainer container)
        {
            _instantiator = container.Resolve<IInstantiator<T>>();
        }
    }

    private sealed class LambdaInstantiator<T>(Func<T> instantiate) : IInstantiator<T>
        where T : class
    {
        public T Instantiate()
        {
            return instantiate.Invoke();
        }
    }
}

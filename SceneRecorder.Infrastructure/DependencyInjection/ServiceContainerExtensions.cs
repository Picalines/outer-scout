using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Infrastructure.DependencyInjection;

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
        Func<IContainer, T> factory
    )
        where T : class
    {
        return registration.InstantiateBy(new LambdaInstantiator<T>(factory));
    }

    public static IRegistration<T> InstantiateBy<T>(
        this IRegistration<T> registration,
        Func<T> factory
    )
        where T : class
    {
        return registration.InstantiateBy(_ => factory());
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

        void IStartupHandler.InitializeService(IContainer container)
        {
            _instantiator = container.Resolve<IInstantiator<T>>();
        }
    }

    private sealed class LambdaInstantiator<T>(Func<IContainer, T> instantiate)
        : IInstantiator<T>,
            IStartupHandler
        where T : class
    {
        private IContainer? _container;

        public T Instantiate()
        {
            _container.ThrowIfNull();
            return instantiate.Invoke(_container);
        }

        void IStartupHandler.InitializeService(IContainer container)
        {
            _container = container;
        }
    }
}

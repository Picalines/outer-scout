namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IRegistration<T>
        where T : class
    {
        public IRegistration<T> As<U>()
            where U : class;

        public IRegistration<T> InstantiateBy(IInstantiator<T> instantiator);

        public IRegistration<T> ManageBy(ILifetime<T> lifetime);
    }

    private interface IRegistration
    {
        public Type InstanceType { get; }

        public IEnumerable<Type> InterfaceTypes { get; }

        public ILifetime<object> Lifetime { get; }

        public void RegisterDependencies(ServiceContainer.Builder builder);
    }

    private sealed class Registration<T> : IRegistration<T>, IRegistration
        where T : class
    {
        public Type InstanceType { get; } = typeof(T);

        private IInstantiator<T>? _instantiator = null;

        private ILifetime<T>? _lifetime = null;

        private readonly HashSet<Type> _interfaceTypes = [];

        public IRegistration<T> As<U>()
            where U : class
        {
            if (typeof(U) is not { IsInterface: true } interfaceType)
            {
                throw new InvalidOperationException("interface was expected");
            }

            _interfaceTypes.Add(interfaceType);
            return this;
        }

        public IRegistration<T> InstantiateBy(IInstantiator<T> instantiator)
        {
            if (_instantiator is not null)
            {
                throw new InvalidOperationException($"ambiguous instantiation of {InstanceType}");
            }

            _instantiator = instantiator;
            return this;
        }

        public IRegistration<T> ManageBy(ILifetime<T> lifetime)
        {
            if (_lifetime is not null)
            {
                throw new InvalidOperationException($"ambiguous lifetime of {InstanceType}");
            }

            _lifetime = lifetime;
            return this;
        }

        public IEnumerable<Type> InterfaceTypes
        {
            get => _interfaceTypes;
        }

        public ILifetime<object> Lifetime
        {
            get => _lifetime ?? new SingletonLifetime<T>();
        }

        public void RegisterDependencies(Builder builder)
        {
            builder
                .Register<IInstantiator<T>>()
                .ManageBy(
                    new ReferenceLifetime<IInstantiator<T>>(
                        _instantiator ?? new ConstructorInstantiator<T>()
                    )
                );
        }
    }
}

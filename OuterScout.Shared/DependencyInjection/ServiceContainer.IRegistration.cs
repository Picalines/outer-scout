namespace OuterScout.Shared.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IRegistration<T>
        where T : class
    {
        public IRegistration<T> As<U>()
            where U : class;

        public IRegistration<T> InstantiateBy(IInstantiator<T> instantiator);

        public IRegistration<T> ManageBy(ILifetime<T> lifetime);

        public IRegistration<T> InScope(string scope);
    }

    private interface IRegistration
    {
        public Type InstanceType { get; }

        public IEnumerable<Type> InterfaceTypes { get; }

        public ILifetime<object> Lifetime { get; }

        public string? ScopeIdentifier { get; }

        public void RegisterDependencies();
    }

    private sealed class Registration<T> : IRegistration<T>, IRegistration
        where T : class
    {
        public Type InstanceType { get; } = typeof(T);

        private readonly ServiceContainer.Builder _containerBuilder;

        private IInstantiator<T>? _instantiator = null;

        private ILifetime<T>? _lifetime = null;

        private readonly HashSet<Type> _interfaceTypes = [];

        private string? _scopeIdentifier = null;

        public Registration(ServiceContainer.Builder containerBuilder)
        {
            _containerBuilder = containerBuilder;
        }

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

        public IRegistration<T> InScope(string scope)
        {
            if (_scopeIdentifier is not null)
            {
                throw new InvalidOperationException($"ambiguous scope of {InstanceType}");
            }

            _scopeIdentifier = scope;
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

        public string? ScopeIdentifier
        {
            get => _scopeIdentifier;
        }

        public void RegisterDependencies()
        {
            var instantiatorRegistration = _containerBuilder
                .Register<IInstantiator<T>>()
                .ManageBy(
                    new ReferenceLifetime<IInstantiator<T>>(
                        _instantiator ?? new DefaultInstantiator<T>()
                    )
                );

            if (_scopeIdentifier is not null)
            {
                instantiatorRegistration.InScope(_scopeIdentifier);
            }
        }
    }
}

using OuterScout.Shared.Extensions;

namespace OuterScout.Shared.DependencyInjection;

public sealed partial class ServiceContainer
{
    public sealed class Builder
    {
        private readonly Dictionary<Type, IRegistration> _registrations = [];

        private bool _dependenciesAreRegistered = false;

        private readonly UsingStack<string> _scopeStack = new();

        public ServiceContainer Build()
        {
            if (_dependenciesAreRegistered is false)
            {
                _registrations
                    .Values.ToArray()
                    .ForEach(registration => registration.RegisterDependencies());

                _dependenciesAreRegistered = true;
            }

            var globalServiceRegistry = new ServiceRegistry();
            var scopeRegistry = new ScopeRegistry();

            foreach (var registration in _registrations.Values)
            {
                var serviceRegistry = registration.ScopeIdentifier switch
                {
                    { } scope => scopeRegistry.GetOrAddScope(scope),
                    _ => globalServiceRegistry,
                };

                serviceRegistry.AddService(registration.InstanceType, registration.Lifetime);

                foreach (var interfaceType in registration.InterfaceTypes)
                {
                    serviceRegistry.AddInterface(registration.InstanceType, interfaceType);
                }
            }

            return new ServiceContainer(scopeRegistry, globalServiceRegistry);
        }

        public IRegistration<T> Register<T>()
            where T : class
        {
            var instanceType = typeof(T);

            AssertServiceType(instanceType);

            if (_registrations.ContainsKey(instanceType) is true)
            {
                ThrowAlreadyRegistered(instanceType);
            }

            var registration = new Registration<T>(this);
            _registrations.Add(instanceType, registration);

            if (_scopeStack.Top is { } scope)
            {
                registration.InScope(scope);
            }

            return registration;
        }

        public IRegistration<T> RegisterIfMissing<T>()
            where T : class
        {
            var instanceType = typeof(T);

            AssertServiceType(instanceType);

            var registration = new Registration<T>(this);

            if (_registrations.ContainsKey(instanceType) is false)
            {
                _registrations.Add(instanceType, registration);
            }

            if (_scopeStack.Top is { } scope)
            {
                registration.InScope(scope);
            }

            return registration;
        }

        public IRegistration<T> Override<T>()
            where T : class
        {
            var instanceType = typeof(T);

            AssertServiceType(instanceType);

            _registrations.Remove(instanceType);

            return Register<T>();
        }

        public IDisposable InScope(string scopeIdentifier)
        {
            return _scopeStack.Use(scopeIdentifier);
        }

        private void AssertServiceType(Type type)
        {
            if (
                type == typeof(ServiceContainer)
                || type == typeof(IServiceContainer)
                || type == typeof(IServiceScope)
            )
            {
                ThrowAlreadyRegistered(type);
            }
        }

        private void ThrowAlreadyRegistered(Type type)
        {
            throw new InvalidOperationException($"type {type} is already registered");
        }
    }
}

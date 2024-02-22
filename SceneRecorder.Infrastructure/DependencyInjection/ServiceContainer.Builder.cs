using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public sealed class Builder
    {
        private readonly Dictionary<Type, IRegistration> _registrations = [];

        private bool _dependenciesAreRegistered = false;

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

            if (_registrations.ContainsKey(instanceType) is true)
            {
                throw new InvalidOperationException($"type {instanceType} is already registered");
            }

            var registration = new Registration<T>(this);
            _registrations.Add(instanceType, registration);

            return registration;
        }

        public IRegistration<T> RegisterIfMissing<T>()
            where T : class
        {
            var registration = new Registration<T>(this);

            var instanceType = typeof(T);
            if (_registrations.ContainsKey(instanceType) is false)
            {
                _registrations.Add(instanceType, registration);
            }

            return registration;
        }

        public IRegistration<T> Override<T>()
            where T : class
        {
            _registrations.Remove(typeof(T));

            return Register<T>();
        }
    }
}

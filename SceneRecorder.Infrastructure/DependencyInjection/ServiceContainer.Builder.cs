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
                    .ForEach(registration => registration.RegisterDependencies(this));

                _dependenciesAreRegistered = true;
            }

            var lifetimes = new Dictionary<Type, ILifetime<object>>();

            var interfaces = new Dictionary<Type, LinkedList<Type>>();

            foreach (var registration in _registrations.Values)
            {
                var lifetime = registration.Lifetime;

                lifetimes[registration.InstanceType] = lifetime;

                foreach (var interfaceType in registration.InterfaceTypes)
                {
                    interfaces.GetOrCreate(interfaceType).AddLast(registration.InstanceType);
                }
            }

            return new ServiceContainer(
                new ContainerScope(
                    lifetimes,
                    interfaces.ToDictionary(p => p.Key, p => p.Value.AsEnumerable())
                )
            );
        }

        public IRegistration<T> Register<T>()
            where T : class
        {
            var instanceType = typeof(T);

            if (_registrations.ContainsKey(instanceType) is true)
            {
                throw new InvalidOperationException($"type {instanceType} is already registered");
            }

            var registration = new Registration<T>();
            _registrations.Add(instanceType, registration);

            return registration;
        }
    }
}

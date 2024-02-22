using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    private interface IServiceRegistry
    {
        public bool ContainsService(Type type);

        public IEnumerable<ILifetime<object>> GetMatchingLifetimes(Type type);

        public IEnumerable<ILifetime<object>> AllLifetimes { get; }
    }

    private sealed class ServiceRegistry : IServiceRegistry
    {
        private readonly Dictionary<Type, ILifetime<object>> _lifetimes = [];

        private readonly Dictionary<Type, LinkedList<Type>> _interfaces = [];

        public void AddService(Type instanceType, ILifetime<object> lifetime)
        {
            if (_lifetimes.ContainsKey(instanceType))
            {
                throw new InvalidOperationException();
            }

            _lifetimes.Add(instanceType, lifetime);
        }

        public void AddInterface(Type instanceType, Type interfaceType)
        {
            if (_lifetimes.ContainsKey(instanceType) is false)
            {
                throw new InvalidOperationException();
            }

            _interfaces.GetOrCreate(interfaceType).AddFirst(instanceType);
        }

        public bool ContainsService(Type type)
        {
            return _lifetimes.ContainsKey(type) || _interfaces.ContainsKey(type);
        }

        public IEnumerable<ILifetime<object>> GetMatchingLifetimes(Type type)
        {
            if (_lifetimes.TryGetValue(type, out var concreteLifetime))
            {
                yield return concreteLifetime;
                yield break;
            }

            if (_interfaces.TryGetValue(type, out var instanceTypes))
            {
                foreach (var instanceType in instanceTypes)
                {
                    yield return _lifetimes[instanceType];
                }
            }
        }

        public IEnumerable<ILifetime<object>> AllLifetimes
        {
            get => _lifetimes.Values;
        }
    }
}

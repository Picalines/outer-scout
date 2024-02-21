using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    private sealed class ServiceRegistry : IDisposable
    {
        private readonly Dictionary<Type, ILifetime<object>> _lifetimes = [];

        private readonly Dictionary<Type, LinkedList<Type>> _interfaces = [];

        private bool _disposed = false;

        public void AddService(Type instanceType, ILifetime<object> lifetime)
        {
            AssertNotDisposed();

            if (_lifetimes.ContainsKey(instanceType))
            {
                throw new InvalidOperationException();
            }

            _lifetimes.Add(instanceType, lifetime);
        }

        public void AddInterface(Type instanceType, Type interfaceType)
        {
            AssertNotDisposed();

            if (_lifetimes.ContainsKey(instanceType) is false)
            {
                throw new InvalidOperationException();
            }

            _interfaces.GetOrCreate(interfaceType).AddLast(instanceType);
        }

        public bool ContainsService(Type type)
        {
            AssertNotDisposed();

            return _lifetimes.ContainsKey(type) || _interfaces.ContainsKey(type);
        }

        public IEnumerable<ILifetime<object>> GetMatchingLifetimes(Type type)
        {
            AssertNotDisposed();

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
            get
            {
                AssertNotDisposed();
                return _lifetimes.Values;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _lifetimes.Values.OfType<IDisposable>().ForEach(disposable => disposable.Dispose());

            _lifetimes.Clear();
            _interfaces.Clear();
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new InvalidOperationException($"{nameof(ServiceRegistry)} is disposed");
            }
        }
    }
}

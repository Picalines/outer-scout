using SceneRecorder.Infrastructure.Extensions;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    private sealed class ServiceRegistry : IDisposable
    {
        private readonly Dictionary<Type, ILifetime<object>> _lifetimes = [];

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

        public bool ContainsService(Type instanceType)
        {
            return _lifetimes.ContainsKey(instanceType);
        }

        public ILifetime<object>? GetLifetime(Type instanceType)
        {
            AssertNotDisposed();

            return _lifetimes.GetValueOrDefault(instanceType);
        }

        public IEnumerable<ILifetime<object>> Lifetimes
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

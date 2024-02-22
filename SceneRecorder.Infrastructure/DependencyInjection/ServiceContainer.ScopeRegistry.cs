namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    private interface IScopeRegistry : IDisposable
    {
        public ServiceRegistry ActivateScopeOrThrow(string scope);

        public void DeactivateScopeOrThrow(string scope);
    }

    private sealed class ScopeRegistry : IScopeRegistry
    {
        private readonly Dictionary<string, ServiceRegistry> _scopedServices = [];

        private readonly HashSet<string> _activeScopes = [];

        private bool _disposed = false;

        public ServiceRegistry AddScope(string identifier)
        {
            AssertNotDisposed();

            var services = new ServiceRegistry();
            _scopedServices.Add(identifier, services);
            return services;
        }

        public ServiceRegistry GetOrAddScope(string identifier)
        {
            AssertNotDisposed();

            return _scopedServices.GetValueOrDefault(identifier) ?? AddScope(identifier);
        }

        public ServiceRegistry GetServices(string scope)
        {
            AssertNotDisposed();

            if (_scopedServices.TryGetValue(scope, out var services) is false)
            {
                throw new InvalidOperationException($"unknown scope {scope}");
            }

            return services;
        }

        public ServiceRegistry ActivateScopeOrThrow(string scope)
        {
            AssertNotDisposed();

            var wasActive = _activeScopes.Add(scope) is false;

            if (wasActive)
            {
                throw new InvalidOperationException($"scope {scope} is already active");
            }

            return _scopedServices[scope];
        }

        public void DeactivateScopeOrThrow(string scope)
        {
            AssertNotDisposed();

            var wasActive = _activeScopes.Remove(scope) is true;

            if (wasActive is false)
            {
                throw new InvalidOperationException($"scope {scope} was not active");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            foreach (var services in _scopedServices.Values)
            {
                services.Dispose();
            }
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new InvalidOperationException($"{nameof(ScopeRegistry)} is disposed");
            }
        }
    }
}

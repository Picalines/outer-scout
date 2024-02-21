namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    private interface IScopeRegistry
    {
        public ServiceRegistry ActivateScopeOrThrow(string scope);

        public void DeactivateScopeOrThrow(string scope);
    }

    private sealed class ScopeRegistry : IScopeRegistry
    {
        private readonly Dictionary<string, ServiceRegistry> _scopedServices = [];

        private readonly HashSet<string> _activeScopes = [];

        public ServiceRegistry AddScope(string identifier)
        {
            var services = new ServiceRegistry();
            _scopedServices.Add(identifier, services);
            return services;
        }

        public ServiceRegistry GetOrAddScope(string identifier)
        {
            return _scopedServices.GetValueOrDefault(identifier) ?? AddScope(identifier);
        }

        public ServiceRegistry GetServices(string scope)
        {
            if (_scopedServices.TryGetValue(scope, out var services) is false)
            {
                throw new InvalidOperationException($"unknown scope {scope}");
            }

            return services;
        }

        public ServiceRegistry ActivateScopeOrThrow(string scope)
        {
            var wasActive = _activeScopes.Add(scope) is false;

            if (wasActive)
            {
                throw new InvalidOperationException($"scope {scope} is already active");
            }

            return _scopedServices[scope];
        }

        public void DeactivateScopeOrThrow(string scope)
        {
            var wasActive = _activeScopes.Remove(scope) is true;

            if (wasActive is false)
            {
                throw new InvalidOperationException($"scope {scope} was not active");
            }
        }
    }
}

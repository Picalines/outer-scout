namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public sealed class Builder
    {
        private readonly List<IRegistration>

        private bool _built = false;

        public ServiceContainer Build()
        {
            AssertNotBuilt();
            _built = true;

            return new ServiceContainer(
                _services.ToDictionary(p => p.Key, p => p.Value.AsEnumerable())
            );
        }

        public IRegistration<T> Register<T>()
            where T : class
        {
            AssertNotBuilt();

            var serviceType = typeof(T);

            if (_services.TryGetValue(serviceType, out var serviceList) is false)
            {
                serviceList = _services[serviceType] = [];
            }

            serviceList.AddFirst(lifetimeManager);

            return this;
        }

        private void AssertNotBuilt()
        {
            if (_built)
            {
                throw new InvalidOperationException($"{nameof(Build)} mehtod called twice");
            }
        }
    }
}

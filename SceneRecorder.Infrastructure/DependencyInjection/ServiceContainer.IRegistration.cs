namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IRegistration<T>
        where T : class
    {
        public IRegistration<T> As<U>()
            where U : class;

        public IRegistration<T> InstantiateBy(IInstantiator<T> instantiator);

        public IRegistration<T> ManageBy(ILifetime lifetime);
    }

    private interface IRegistration
    {
        public IEnumerable<Type> Types { get; }

        public IInstantiator<object> Instantiator { get; }

        public ILifetime Lifetime { get; }
    }

    private sealed class Registration<T> : IRegistration<T>, IRegistration
        where T : class
    {
        private readonly HashSet<Type> _types = [typeof(T)];

        public IInstantiator<object> Instantiator { get; private set; } = null; // TODO

        public ILifetime Lifetime { get; private set; } = null; // TODO

        public IRegistration<T> As<U>()
            where U : class
        {
            _types.Add(typeof(U));
            return this;
        }

        public IRegistration<T> InstantiateBy(IInstantiator<T> instantiator)
        {
            Instantiator = instantiator;
            return this;
        }

        public IRegistration<T> ManageBy(ILifetime lifetime)
        {
            Lifetime = lifetime;
            return this;
        }

        public IEnumerable<Type> Types
        {
            get => _types;
        }
    }
}

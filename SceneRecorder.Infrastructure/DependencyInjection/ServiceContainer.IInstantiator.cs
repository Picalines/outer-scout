using System.Reflection;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IInstantiator<out T>
        where T : class
    {
        public T Instantiate();
    }

    public sealed class ConstructorInstantiator<T> : IInstantiator<T>, IStartupHandler
        where T : class
    {
        private readonly Type _type;

        private IServiceContainer? _container = null;

        private ConstructorInfo? _constructor = null;

        public ConstructorInstantiator()
        {
            _type = typeof(T);

            if (_type is not { IsClass: true, IsAbstract: false })
            {
                throw new InvalidOperationException(
                    $"instance of {typeof(T)} cannot be constructed"
                );
            }
        }

        public T Instantiate()
        {
            _container.ThrowIfNull();
            _constructor.ThrowIfNull();

            return (T)
                _constructor.Invoke(
                    _constructor
                        .GetParameters()
                        .Select(p =>
                            p.HasDefaultValue
                                ? _container.ResolveOrNull(p.ParameterType) ?? p.DefaultValue
                                : _container.Resolve(p.ParameterType)
                        )
                        .ToArray()
                );
        }

        void IStartupHandler.InitializeService(IServiceContainer container)
        {
            _container = container;

            _constructor = _type
                .GetConstructors()
                .Select(c => new { Constructor = c, Parameters = c.GetParameters() })
                .OrderBy(c => c.Parameters.Length)
                .Where(c => c.Parameters.All(p => container.Contains(p.ParameterType)))
                .Select(c => c.Constructor)
                .LastOrDefault();

            if (_constructor is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(ServiceContainer)} can't construct an instance of type {_type.Name}"
                );
            }
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.Infrastructure.Validation;

namespace OuterScout.Infrastructure.DependencyInjection;

public sealed partial class ServiceContainer
{
    public interface IInstantiator<out T>
        where T : class
    {
        public T Instantiate();
    }

    public sealed class DefaultInstantiator<T> : IInstantiator<T>, IStartupHandler
        where T : class
    {
        private readonly Type _type;

        private IServiceContainer? _container = null;

        private ConstructorInfo? _constructor = null;

        private PropertyInfo[] _requiredProperties = Array.Empty<PropertyInfo>();

        public DefaultInstantiator()
        {
            _type = typeof(T);
        }

        public T Instantiate()
        {
            _container.AssertNotNull();

            ScanInstanceType();

            var constructorParameters = _constructor
                .GetParameters()
                .Select(p =>
                    p.HasDefaultValue
                        ? _container.ResolveOrNull(p.ParameterType) ?? p.DefaultValue
                        : _container.Resolve(p.ParameterType)
                )
                .ToArray();

            var instance = (T)_constructor.Invoke(constructorParameters);

            _requiredProperties.ForEach(property =>
                property.SetValue(instance, _container.Resolve(property.PropertyType))
            );

            return instance;
        }

        void IStartupHandler.InitializeService(IServiceContainer container)
        {
            _container = container;
        }

        [MemberNotNull(nameof(_constructor))]
        private void ScanInstanceType()
        {
            if (_constructor is not null)
            {
                return;
            }

            _container.AssertNotNull();

            if (_type is not { IsClass: true, IsAbstract: false })
            {
                throw new InvalidOperationException(
                    $"instance of {typeof(T)} cannot be constructed"
                );
            }

            _constructor =
                _type
                    .GetConstructors()
                    .Select(c => new { Constructor = c, Parameters = c.GetParameters() })
                    .OrderBy(c => c.Parameters.Length)
                    .Where(c => c.Parameters.All(p => _container.Contains(p.ParameterType)))
                    .Select(c => c.Constructor)
                    .LastOrDefault()
                ?? throw new InvalidOperationException(
                    $"{nameof(ServiceContainer)} can't construct an instance of type {_type.Name}"
                );

            _requiredProperties = _type
                .GetProperties(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy
                )
                .Where(property => property.CanWrite && property.IsRequired())
                .Tap(property =>
                {
                    if (_container.Contains(property.PropertyType) is false)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(ServiceContainer)} can't supply required property {_type.Name}.{property.Name}"
                        );
                    }
                })
                .ToArray();
        }
    }
}

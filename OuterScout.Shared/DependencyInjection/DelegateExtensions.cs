using System.Reflection;

namespace OuterScout.Shared.DependencyInjection;

public static class DelegateExtensions
{
    public static Func<IServiceContainer, object?> BindByContainer(this Delegate @delegate)
    {
        var method = @delegate.Method;
        var target = @delegate.Target;
        var parameters = method.GetParameters();

        if (parameters.Length is 0)
        {
            return _ => method.Invoke(target, []);
        }

        return container =>
        {
            var arguments = new object?[parameters.Length];

            foreach (var parameter in parameters)
            {
                var binder = container
                    .ResolveAll<IParameterBinder>()
                    .FirstOrDefault(b => b.CanBind(parameter));

                arguments[parameter.Position] = binder is not null
                    ? binder.Bind(parameter)
                    : BindDefault(container, parameter);
            }

            return method.Invoke(target, arguments);
        };
    }

    private static object? BindDefault(IServiceContainer container, ParameterInfo parameter)
    {
        if (parameter.HasDefaultValue)
        {
            return container.ResolveOrNull(parameter.ParameterType) ?? parameter.DefaultValue;
        }

        return container.Resolve(parameter.ParameterType);
    }
}

public interface IParameterBinder
{
    public bool CanBind(ParameterInfo parameter);

    public object? Bind(ParameterInfo parameter);
}

using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OuterScout.Infrastructure.Extensions;

public static class ReflectionExtensions
{
    public static bool IsRequired(this PropertyInfo property)
    {
        return Attribute.IsDefined(property, typeof(RequiredMemberAttribute));
    }

    public static bool IsNullable(this ParameterInfo parameter)
    {
        var nullableAttribute = parameter.CustomAttributes.FirstOrDefault(attribute =>
            attribute
                is { AttributeType.FullName: "System.Runtime.CompilerServices.NullableAttribute" }
        );

        if (nullableAttribute is not { ConstructorArguments.Count: 1 })
        {
            return false;
        }

        var nullabilityArgument = nullableAttribute.ConstructorArguments[0];

        if (nullabilityArgument.ArgumentType == typeof(byte[]))
        {
            var nullabilityBytes =
                (ReadOnlyCollection<CustomAttributeTypedArgument>)nullabilityArgument.Value!;

            if (nullabilityBytes.Count > 0 && nullabilityBytes[0].ArgumentType == typeof(byte))
            {
                return (byte)nullabilityBytes[0].Value == 2;
            }
        }
        else if (nullabilityArgument.ArgumentType == typeof(byte))
        {
            return (byte)nullabilityArgument.Value == 2;
        }

        return false;
    }
}

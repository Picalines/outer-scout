using System.Reflection;
using System.Runtime.CompilerServices;

namespace OuterScout.Shared.Extensions;

public static class ReflectionExtensions
{
    public static bool IsRequired(this MemberInfo member)
    {
        // NOTE: don't use Attribute.IsDefined, because the attribute type comes from PolySharp
        return member.CustomAttributes.Any(attribute =>
            attribute.AttributeType.FullName == typeof(RequiredMemberAttribute).FullName
        );
    }

    public static bool IsNullable(this FieldInfo field)
    {
        return IsNullable(field.FieldType, field.CustomAttributes);
    }

    public static bool IsNullable(this PropertyInfo property)
    {
        return IsNullable(property.PropertyType, property.CustomAttributes);
    }

    public static bool IsNullable(this ParameterInfo parameter)
    {
        return IsNullable(parameter.ParameterType, parameter.CustomAttributes);
    }

    private static bool IsNullable(
        Type memberType,
        IEnumerable<CustomAttributeData> customAttributes
    )
    {
        if (memberType.IsValueType)
        {
            return Nullable.GetUnderlyingType(memberType) is not null;
        }

        var nullableAttribute = customAttributes.FirstOrDefault(attribute =>
            attribute
                is { AttributeType.FullName: "System.Runtime.CompilerServices.NullableAttribute" }
        );

        if (nullableAttribute is not { ConstructorArguments.Count: 1 })
        {
            return false;
        }

        var nullabilityArgument = nullableAttribute.ConstructorArguments[0];

        byte nullabilityByte = 0;

        if (nullabilityArgument.ArgumentType == typeof(byte[]))
        {
            if (
                nullabilityArgument
                    is {
                        Value: IReadOnlyList<CustomAttributeTypedArgument>
                        {
                            Count: > 0
                        } nullabilityBytes
                    }
                && nullabilityBytes[0] is { } firstNullabilityByte
                && firstNullabilityByte.ArgumentType == typeof(byte)
            )
            {
                nullabilityByte = (byte)nullabilityBytes[0].Value;
            }
        }
        else if (nullabilityArgument.ArgumentType == typeof(byte))
        {
            nullabilityByte = (byte)nullabilityArgument.Value;
        }

        return nullabilityByte is (byte)2;
    }
}

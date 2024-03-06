namespace OuterScout.Infrastructure.Extensions;

public static class EnumExtensions
{
    public static string ToStringWithType(this Enum @enum)
    {
        return $"{@enum.GetType().Name}.{@enum}";
    }
}

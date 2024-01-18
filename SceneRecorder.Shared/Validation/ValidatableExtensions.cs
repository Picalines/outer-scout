using System.Runtime.CompilerServices;

namespace SceneRecorder.Shared.Validation;

public static class ValidatableExtensions
{
    public static Validatable<T> If<T>(
        this Validatable<T> validatable,
        bool condition,
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = ""
    )
    {
        if (condition)
        {
            throw validatable.CreateException(
                paramName =>
                    throw new ArgumentException(
                        $"condition {conditionExpression} is true",
                        paramName
                    )
            );
        }

        return validatable;
    }

    public static Validatable<T> If<T>(
        this Validatable<T> validatable,
        Func<bool> condition,
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = ""
    )
    {
        return validatable.If(condition(), conditionExpression);
    }

    public static Validatable<T> If<T>(
        this Validatable<T> validatable,
        Func<T, bool> condition,
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = ""
    )
    {
        return validatable.If(condition(validatable.Value), conditionExpression);
    }

    public static Validatable<T> IfNull<T>(this Validatable<T?> validatable)
    {
        if (validatable.Value == null)
        {
            throw validatable.CreateException(
                paramName => throw new ArgumentNullException(paramName)
            );
        }

        return validatable!;
    }

    public static Validatable<bool> IfTrue(this Validatable<bool> validatable)
    {
        if (validatable.Value is true)
        {
            throw validatable.CreateException(
                paramName => throw new ArgumentException($"{paramName} is true", paramName)
            );
        }

        return validatable;
    }

    public static Validatable<bool> IfFalse(this Validatable<bool> validatable)
    {
        if (validatable.Value is false)
        {
            throw validatable.CreateException(
                paramName => throw new ArgumentException($"{paramName} is false", paramName)
            );
        }

        return validatable;
    }

    public static Validatable<string> IsEmpty(this Validatable<string> validatable)
    {
        if (validatable.Value is "")
        {
            throw validatable.CreateException(
                paramName => throw new ArgumentException($"string {paramName} is empty", paramName)
            );
        }

        return validatable;
    }

    public static Validatable<string> IfNullOrWhiteSpace(this Validatable<string> validatable)
    {
        if (string.IsNullOrWhiteSpace(validatable.Value))
        {
            throw validatable.CreateException(
                paramName =>
                    throw new ArgumentException(
                        $"string {paramName} is null or whitespace",
                        paramName
                    )
            );
        }

        return validatable;
    }

    public static Validatable<int> IfLessThan(this Validatable<int> validatable, int threshold)
    {
        if (validatable.Value < threshold)
        {
            throw validatable.CreateException(
                paramName =>
                    throw new ArgumentOutOfRangeException(
                        paramName,
                        $"{paramName} is less than {threshold}"
                    )
            );
        }

        return validatable;
    }

    public static Validatable<int> IfGreaterThan(this Validatable<int> validatable, int threshold)
    {
        if (validatable.Value > threshold)
        {
            throw validatable.CreateException(
                paramName =>
                    throw new ArgumentOutOfRangeException(
                        paramName,
                        $"{paramName} is greater than {threshold}"
                    )
            );
        }

        return validatable;
    }
}

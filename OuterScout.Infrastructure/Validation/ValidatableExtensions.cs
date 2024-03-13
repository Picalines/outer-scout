using System.Runtime.CompilerServices;

namespace OuterScout.Infrastructure.Validation;

public static class ValidatableExtensions
{
    public static T OrReturn<T>(this Validatable<T> validatable)
    {
        return validatable.Value;
    }

    public static Validatable<T> If<T>(
        this Validatable<T> validatable,
        bool condition,
        [CallerArgumentExpression(nameof(condition))] string conditionExpression = ""
    )
    {
        if (condition)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"condition {conditionExpression} is true", expression)
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

    public static Validatable<T> IfEquals<T>(
        this Validatable<T> validatable,
        T value,
        EqualityComparer<T>? equalityComparer = null,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        equalityComparer ??= EqualityComparer<T>.Default;

        if (equalityComparer.Equals(validatable.Value, value) is true)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"{expression} equals {valueExpression}", expression)
            );
        }

        return validatable;
    }

    public static Validatable<T> IfNotEquals<T>(
        this Validatable<T> validatable,
        T value,
        EqualityComparer<T>? equalityComparer = null,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        equalityComparer ??= EqualityComparer<T>.Default;

        if (equalityComparer.Equals(validatable.Value, value) is false)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"{expression} equals {valueExpression}", expression)
            );
        }

        return validatable;
    }

    public static Validatable<T> IfNull<T>(this Validatable<T?> validatable)
    {
        if (validatable.Value == null)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentNullException(expression)
            );
        }

        return validatable!;
    }

    public static Validatable<bool> IfTrue(this Validatable<bool> validatable)
    {
        if (validatable.Value is true)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"{expression} is true", expression)
            );
        }

        return validatable;
    }

    public static Validatable<bool> IfFalse(this Validatable<bool> validatable)
    {
        if (validatable.Value is false)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"{expression} is false", expression)
            );
        }

        return validatable;
    }

    public static Validatable<string> IsEmpty(this Validatable<string> validatable)
    {
        if (validatable.Value is "")
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException($"string {expression} is empty", expression)
            );
        }

        return validatable;
    }

    public static Validatable<string> IfNullOrWhiteSpace(this Validatable<string> validatable)
    {
        if (string.IsNullOrWhiteSpace(validatable.Value))
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentException(
                    $"string {expression} is null or whitespace",
                    expression
                )
            );
        }

        return validatable;
    }

    public static Validatable<int> IfLessThan(this Validatable<int> validatable, int threshold)
    {
        if (validatable.Value < threshold)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentOutOfRangeException(
                    expression,
                    $"{expression} is less than {threshold}"
                )
            );
        }

        return validatable;
    }

    public static Validatable<int> IfGreaterThan(this Validatable<int> validatable, int threshold)
    {
        if (validatable.Value > threshold)
        {
            throw validatable.CustomExceptionOr(expression =>
                throw new ArgumentOutOfRangeException(
                    expression,
                    $"{expression} is greater than {threshold}"
                )
            );
        }

        return validatable;
    }
}

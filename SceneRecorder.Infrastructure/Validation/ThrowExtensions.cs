using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SceneRecorder.Infrastructure.Validation;

public static class ThrowExtensions
{
    public static Validatable<T> Throw<T>(
        this T value,
        Validatable<T>.ExceptionFactory? exceptionFactory = null,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        return new Validatable<T>(value, valueExpression, exceptionFactory);
    }

    public static Validatable<T> Throw<T>(
        this T value,
        string argumentMessage,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        return value.Throw(
            _ => new ArgumentException(message: argumentMessage, paramName: valueExpression)
        );
    }

    public static Validatable<T> ThrowIfNull<T>(
        [NotNull] this T? value,
        Validatable<T>.ExceptionFactory? exceptionFactory = null,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        if (value is not { } notNull)
        {
            throw exceptionFactory?.Invoke(valueExpression)
                ?? new ArgumentNullException(paramName: valueExpression);
        }

        return notNull.Throw(exceptionFactory, valueExpression);
    }

    public static Validatable<T> ThrowIfNull<T>(
        [NotNull] this T? value,
        string argumentMessage,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        return value.ThrowIfNull(
            _ => new ArgumentException(message: argumentMessage, paramName: valueExpression)
        );
    }
}

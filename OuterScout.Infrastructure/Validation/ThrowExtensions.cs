using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OuterScout.Infrastructure.Validation;

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

    public static Validatable<T> Assert<T>(
        this T value,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        return value.Throw(e => new AssertionException(e), valueExpression);
    }

    public static Validatable<T> AssertNotNull<T>(
        [NotNull] this T? value,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        if (value is not { } notNull)
        {
            throw new AssertionException($"{valueExpression} is null");
        }

        return notNull.Assert(valueExpression);
    }
}

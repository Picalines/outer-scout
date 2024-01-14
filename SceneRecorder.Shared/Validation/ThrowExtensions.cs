using System.Runtime.CompilerServices;

namespace SceneRecorder.Shared.Validation;

public static class ThrowExtensions
{
    public static Validatable<T> Throw<T>(this T value, Func<Exception> exceptionFactory)
    {
        return new Validatable<T>(value, exceptionFactory);
    }

    public static Validatable<T> ThrowArgument<T>(
        this T value,
        string exceptionMessage,
        [CallerArgumentExpression(nameof(value))] string valueExpression = ""
    )
    {
        return value.Throw(() => new ArgumentException(exceptionMessage, valueExpression));
    }

    public static Validatable<T> ThrowInvalidOperation<T>(this T value, string exceptionMessage)
    {
        return value.Throw(() => new InvalidOperationException(exceptionMessage));
    }
}

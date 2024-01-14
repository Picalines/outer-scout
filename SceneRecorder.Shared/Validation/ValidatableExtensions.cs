namespace SceneRecorder.Shared.Validation;

public static class ValidatableExtensions
{
    public static Validatable<T> If<T>(this Validatable<T> validatable, bool condition)
    {
        if (condition)
        {
            throw validatable.ExceptionFactory();
        }

        return validatable;
    }

    public static Validatable<T> If<T>(this Validatable<T> validatable, Func<bool> condition)
    {
        return validatable.If(condition());
    }

    public static Validatable<T> If<T>(this Validatable<T> validatable, Func<T, bool> condition)
    {
        return validatable.If(condition(validatable.Value));
    }

    public static Validatable<T> IfNull<T>(this Validatable<T?> validatable)
    {
        if (validatable.Value is not { } value)
        {
            throw validatable.ExceptionFactory();
        }

        return new Validatable<T>(value, validatable.ExceptionFactory);
    }

    public static Validatable<bool> IfTrue(this Validatable<bool> validatable)
    {
        if (validatable.Value is true)
        {
            throw validatable.ExceptionFactory();
        }

        return validatable;
    }

    public static Validatable<bool> IfFalse(this Validatable<bool> validatable)
    {
        if (validatable.Value is false)
        {
            throw validatable.ExceptionFactory();
        }

        return validatable;
    }
}

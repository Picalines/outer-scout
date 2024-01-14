namespace SceneRecorder.Shared.Validation;

public readonly ref struct Validatable<T>
{
    public T Value { get; }

    public Func<Exception> ExceptionFactory { get; }

    internal Validatable(T value, Func<Exception> exceptionFactory)
    {
        Value = value;
        ExceptionFactory = exceptionFactory;
    }
}

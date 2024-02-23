namespace SceneRecorder.Infrastructure.Validation;

public readonly ref struct Validatable<T>
{
    public delegate Exception ExceptionFactory(string expression);

    internal T Value { get; }

    private string _valueExpression { get; }

    private ExceptionFactory? _exceptionFactory { get; }

    internal Validatable(T value, string valueExpression, ExceptionFactory? exceptionFactory)
    {
        Value = value;
        _valueExpression = valueExpression;
        _exceptionFactory = exceptionFactory;
    }

    internal Exception CustomExceptionOr(ExceptionFactory standard)
    {
        return _exceptionFactory?.Invoke(_valueExpression) ?? standard(_valueExpression);
    }
}

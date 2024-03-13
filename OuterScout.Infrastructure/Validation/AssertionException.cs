namespace OuterScout.Infrastructure.Validation;

public sealed class AssertionException : Exception
{
    public AssertionException(string message)
        : base(message) { }
}

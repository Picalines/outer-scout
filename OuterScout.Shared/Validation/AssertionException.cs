namespace OuterScout.Shared.Validation;

public sealed class AssertionException : Exception
{
    public AssertionException(string message)
        : base(message) { }
}

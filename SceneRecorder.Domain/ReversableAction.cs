namespace OuterScout.Domain;

public sealed class ReversableAction
{
    private Action? _perform = null;

    private Action? _reverse = null;

    private bool _performed = false;

    public ReversableAction(Action perform, Action reverse)
    {
        _perform = perform;
        _reverse = reverse;
    }

    public void Perform()
    {
        if (_performed)
        {
            throw new InvalidOperationException(
                $"{nameof(ReversableAction)}.{nameof(Perform)} called twice"
            );
        }

        _performed = true;

        _perform?.Invoke();
    }

    public void Reverse()
    {
        if (_performed is false)
        {
            throw new InvalidOperationException(
                $"{nameof(ReversableAction)}.{nameof(Reverse)} called before {nameof(Perform)}"
            );
        }

        _performed = false;

        _reverse?.Invoke();
    }
}

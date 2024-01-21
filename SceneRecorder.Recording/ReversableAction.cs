namespace SceneRecorder.Recording;

internal sealed class ReversableAction
{
    private Action? _perform = null;

    private Action? _reverse = null;

    public ReversableAction(Action perform, Action reverse)
    {
        _perform = perform;
        _reverse = reverse;
    }

    public ReversableAction(Func<Action> performAndGetReverser)
    {
        _perform = () =>
        {
            _reverse = performAndGetReverser();
        };
    }

    public void Perform()
    {
        _perform?.Invoke();
    }

    public void Reverse()
    {
        _reverse?.Invoke();
    }
}

namespace OuterScout.Application.Recording;

public sealed class ReversableAction(Action perform, Action reverse)
{
    private bool _performed = false;

    public void PerformIfNotAlready()
    {
        if (_performed is false)
        {
            _performed = true;
            perform.Invoke();
        }
    }

    public void ReverseIfPerformed()
    {
        if (_performed)
        {
            _performed = false;
            reverse.Invoke();
        }
    }
}

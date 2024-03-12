using OWML.Common;

namespace OuterScout.Infrastructure.Logging;

internal sealed class FilteredModConsole : IModConsole
{
    private readonly IModConsole _modConsole;

    private readonly Func<string, MessageType, bool> _filter;

    public FilteredModConsole(IModConsole modConsole, Func<string, MessageType, bool> filter)
    {
        _modConsole = modConsole;
        _filter = filter;
    }

    [Obsolete]
    public void WriteLine(params object[] objects)
    {
        WriteLine(string.Join(", ", objects), MessageType.Message);
    }

    public void WriteLine(string line)
    {
        WriteLine(line, MessageType.Message);
    }

    public void WriteLine(string line, MessageType type)
    {
        if (_filter.Invoke(line, type))
        {
            _modConsole.WriteLine(line, type);
        }
    }

    public void WriteLine(string line, MessageType type, string senderType)
    {
        if (_filter.Invoke(line, type))
        {
            _modConsole.WriteLine(line, type, senderType);
        }
    }
}

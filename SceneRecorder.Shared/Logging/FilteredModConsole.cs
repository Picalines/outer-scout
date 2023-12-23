using OWML.Common;

namespace SceneRecorder.Shared.Logging;

internal sealed class FilteredModConsole : IModConsole
{
    private readonly IModConsole _ModConsole;

    private readonly Func<string, MessageType, bool> _Filter;

    public FilteredModConsole(IModConsole modConsole, Func<string, MessageType, bool> filter)
    {
        _ModConsole = modConsole;
        _Filter = filter;
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
        if (_Filter.Invoke(line, type))
        {
            _ModConsole.WriteLine(line, type);
        }
    }

    public void WriteLine(string line, MessageType type, string senderType)
    {
        if (_Filter.Invoke(line, type))
        {
            _ModConsole.WriteLine(line, type, senderType);
        }
    }
}

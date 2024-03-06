using OWML.Common;

namespace OuterScout.Infrastructure.Logging;

internal sealed class SilentModConsole : IModConsole
{
    public static IModConsole Instance { get; private set; } = new SilentModConsole();

    private SilentModConsole() { }

    public void WriteLine(params object[] objects) { }

    public void WriteLine(string line) { }

    public void WriteLine(string line, MessageType type) { }

    public void WriteLine(string line, MessageType type, string senderType) { }
}

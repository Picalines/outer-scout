using System.Text;

namespace Picalines.OuterWilds.SceneRecorder.Recording.FFmpeg;

internal readonly ref struct CommandLineArguments
{
    private readonly StringBuilder _StringBuilder;

    public CommandLineArguments()
    {
        _StringBuilder = new StringBuilder();
    }

    public CommandLineArguments Add(string argument)
    {
        _StringBuilder.Append(argument);
        _StringBuilder.Append(' ');
        return this;
    }

    public override string ToString()
    {
        _StringBuilder.Remove(_StringBuilder.Length - 1, 1);
        return _StringBuilder.ToString();
    }
}

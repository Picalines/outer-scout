using System.Text;

namespace SceneRecorder.Application.FFmpeg;

internal readonly ref struct CommandLineArguments
{
    private readonly StringBuilder _stringBuilder;

    public CommandLineArguments()
    {
        _stringBuilder = new StringBuilder();
    }

    public CommandLineArguments Add(string argument)
    {
        _stringBuilder.Append(argument);
        _stringBuilder.Append(' ');
        return this;
    }

    public override string ToString()
    {
        _stringBuilder.Remove(_stringBuilder.Length - 1, 1);
        return _stringBuilder.ToString();
    }
}

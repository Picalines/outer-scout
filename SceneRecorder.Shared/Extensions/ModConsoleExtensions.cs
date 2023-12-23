using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Shared.Logging;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Extensions;

public static class ModConsoleExtensions
{
    public static IModConsole WithFiltering(
        this IModConsole modConsole,
        Func<string, MessageType, bool> keepLog
    )
    {
        return new FilteredModConsole(modConsole, keepLog);
    }

    public static IModConsole WithOnlyMessagesOfType(
        this IModConsole modConsole,
        params MessageType[] allowedMessageTypes
    )
    {
        if (allowedMessageTypes.Length is 0)
        {
            return SilentModConsole.Instance;
        }

        var allowedMessageTypesHashSet = new HashSet<MessageType>(allowedMessageTypes);

        return WithFiltering(modConsole, (_, type) => allowedMessageTypesHashSet.Contains(type));
    }

    public static IModConsole WithoutMessagesOfType(
        this IModConsole modConsole,
        params MessageType[] notAllowedMessageTypes
    )
    {
        if (notAllowedMessageTypes.Length is 0)
        {
            return modConsole;
        }

        var notAllowedMessageTypesHashSet = new HashSet<MessageType>(notAllowedMessageTypes);

        return WithFiltering(
            modConsole,
            (_, type) => !notAllowedMessageTypesHashSet.Contains(type)
        );
    }
}

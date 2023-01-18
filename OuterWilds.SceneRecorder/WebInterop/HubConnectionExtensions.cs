using Microsoft.AspNetCore.SignalR.Client;
using OWML.Common;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

internal static class HubConnectionExtensions
{
    public static async Task StartAsyncWithLogs(this HubConnection hubConnection, IModConsole modConsole)
    {
        try
        {
            Console.WriteLine(RuntimeInformation.OSDescription);
            await hubConnection!.StartAsync();
        }
        catch (Exception exception)
        {
            modConsole.WriteLine($"{nameof(SceneRecorder)} couldn't start the hub connection:", MessageType.Error);
            modConsole.WriteLine(exception.ToString(), MessageType.Error);
            return;
        }

        modConsole.WriteLine($"{nameof(SceneRecorder)} hub connection started");
    }

    public static HubConnection LogOnClosed(this HubConnection hubConnection, IModConsole modConsole)
    {
        hubConnection!.Closed += exception =>
        {
            modConsole.WriteLine($"{nameof(SceneRecorder)} hub connection was closed", exception is null ? MessageType.Info : MessageType.Error);

            if (exception is not null)
            {
                modConsole.WriteLine(exception.ToString(), MessageType.Error);
            }

            return Task.CompletedTask;
        };

        return hubConnection;
    }
}

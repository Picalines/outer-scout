using Microsoft.AspNetCore.SignalR;

namespace Picalines.OuterWilds.SceneRecorder.WebUI.Hubs;

internal sealed class StateHub : Hub
{
    public async Task GetTimeSinceStartup()
    {
        await Clients.All.SendAsync(nameof(GetTimeSinceStartup));
    }

    public async Task ReceiveTimeSinceStartup(float time)
    {
        await Clients.All.SendAsync(nameof(ReceiveTimeSinceStartup), time);
    }
}

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using OWML.Common;

namespace Picalines.OuterWilds.SceneRecorder.WebInterop;

public sealed class WebAPI
{
    private readonly IModConsole _ModConsole;

    private readonly IWebHost _WebHost;

    public WebAPI(IModConsole modConsole)
    {
        _ModConsole = modConsole;

        _WebHost = WebHost.CreateDefaultBuilder()
            .UseStartup<Startup>()
            .UseUrls("http://localhost:5000")
            .Build();
    }

    public void Run()
    {
        Task.Run(async () =>
        {
            _ModConsole.WriteLine("WebAPI started running");

            try
            {
                await _WebHost.RunAsync();
            }
            catch (Exception ex)
            {
                _ModConsole.WriteLine("WebAPI raised an exception:", MessageType.Error);
                _ModConsole.WriteLine(ex.ToString(), MessageType.Error);
                if (ex is TypeLoadException tle)
                {
                    _ModConsole.WriteLine($"Type name: {tle.TypeName}");
                }
            }

            _ModConsole.WriteLine("WebAPI stopped running");
        });
    }

    public void Stop()
    {
        _WebHost.StopAsync(TimeSpan.FromSeconds(5)).Wait();
    }
}

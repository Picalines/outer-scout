using Picalines.OuterWilds.SceneRecorder.WebUI.Hubs;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<StateHub>("/signalr");
app.MapFallbackToPage("/_Host");

_ = typeof(RuntimeInformation);

app.Run();
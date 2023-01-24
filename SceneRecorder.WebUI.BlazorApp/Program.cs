using Picalines.OuterWilds.SceneRecorder.WebUI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<IModApiClient, ModApiClient>(services =>
{
    return new ModApiClient(
        httpClientFactory: services.GetRequiredService<IHttpClientFactory>(),
        baseApiUrl: GetCommandLineFlagValue("--api-url"));
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

string GetCommandLineFlagValue(string flagName)
{
    try
    {
        return args.SkipWhile(arg => arg != flagName)
            .Skip(1)
            .First();
    }
    catch (InvalidOperationException)
    {
        throw new ArgumentException($"missing command line flag '{flagName}'");
    }
}

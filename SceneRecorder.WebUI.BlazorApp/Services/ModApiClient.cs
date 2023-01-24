using Picalines.OuterWilds.SceneRecorder.Json;
using System.Text.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebUI.Services;

internal sealed class ModApiClient : IModApiClient
{
    private readonly HttpClient _HttpClient;

    private readonly string _BaseApiUrl;

    public ModApiClient(IHttpClientFactory httpClientFactory, string baseApiUrl)
    {
        _HttpClient = httpClientFactory.CreateClient();
        _BaseApiUrl = baseApiUrl.Trim('/');
    }

    public async Task<SceneSettings?> GetSceneSettingsAsync()
    {
        return await ParseResponseOrDefault<SceneSettings>(HttpMethod.Get, "scene/settings");
    }

    public async Task<int?> GetRecorderFramesRecorded()
    {
        return await ParseResponseOrDefault<int>(HttpMethod.Get, "recorder/frames_recorded");
    }

    public async Task<bool> GetRecorderEnabledAsync()
    {
        return await ParseResponseOrDefault<bool>(HttpMethod.Get, "recorder/enabled");
    }

    public async Task<bool> SetRecorderEnabledAsync(bool enabled)
    {
        return (await GetResponse(HttpMethod.Put, $"recorder?enabled={enabled}")).IsSuccessStatusCode;
    }

    private async Task<HttpResponseMessage> GetResponse(HttpMethod httpMethod, string relativeUrl)
    {
        var request = new HttpRequestMessage(httpMethod, $"{_BaseApiUrl}/{relativeUrl}");
        return await _HttpClient.SendAsync(request);
    }

    private async Task<T?> ParseResponseOrDefault<T>(HttpMethod httpMethod, string relativeUrl)
    {
        return await GetResponse(httpMethod, relativeUrl) switch
        {
            { IsSuccessStatusCode: false } => default,

            { Content.Headers.ContentType.MediaType: "application/json", Content: var content } =>
                JsonSerializer.Deserialize<T>(await content.ReadAsStringAsync()),

            { Content.Headers.ContentType.MediaType: "text/plain", Content: var content, } =>
                await content.ReadAsStringAsync() switch
                {
                    T expectedMessage => expectedMessage,
                    _ => default,
                },

            _ => default,
        };
    }
}

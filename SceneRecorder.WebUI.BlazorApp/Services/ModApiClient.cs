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

    public async Task<bool> SetRecorderEnabledAsync(bool enabled)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{_BaseApiUrl}/recorder?enabled={enabled}");
        var response = await _HttpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}

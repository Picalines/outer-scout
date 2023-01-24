namespace Picalines.OuterWilds.SceneRecorder.WebUI.Services;

public interface IModApiClient
{
    public Task<bool> SetRecorderEnabledAsync(bool enabled);
}

using Picalines.OuterWilds.SceneRecorder.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebUI.Services;

public interface IModApiClient
{
    public Task<SceneSettings?> GetSceneSettingsAsync();

    public Task<int?> GetRecorderFramesRecorded();

    public Task<bool> GetRecorderEnabledAsync();

    public Task<bool> SetRecorderEnabledAsync(bool enabled);
}

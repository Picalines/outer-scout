using Newtonsoft.Json;
using OWML.Common;
using System.Reflection;

namespace Picalines.OuterWilds.SceneRecorder.Json;

#pragma warning disable CS8618

internal sealed class SceneRecorderSettings
{
    [JsonProperty("output_dir")]
    public string OutputDirectory { get; private set; }

    [JsonProperty("framerate")]
    public int FrameRate { get; private set; }

    [JsonProperty("width")]
    public int Width { get; private set; }

    [JsonProperty("height")]
    public int Height { get; private set; }

    [JsonProperty("hdri_face_size")]
    public int HDRIFaceSize { get; private set; }

    [JsonProperty("hide_player_model")]
    public bool HidePlayerModel { get; private set; }

    [JsonProperty("hdri_in_feet")]
    public bool HDRIInFeet { get; private set; }

    [JsonProperty("web_ui_port"), JsonIgnore]
    private readonly int _WebUIPort = 5000;

    [JsonProperty("web_api_port"), JsonIgnore]
    private readonly int _WebApiPort = 5001;

    private static readonly (PropertyInfo, JsonPropertyAttribute)[] _JsonProperties;

    private static readonly MethodInfo _GetSettingsValueMethod;

    static SceneRecorderSettings()
    {
        _JsonProperties = typeof(SceneRecorderSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.IsDefined(typeof(JsonPropertyAttribute)))
            .Select(property => (property, property.GetCustomAttribute<JsonPropertyAttribute>()))
            .ToArray();

        _GetSettingsValueMethod = typeof(IModConfig).GetMethod(nameof(IModConfig.GetSettingsValue));
    }

    public SceneRecorderSettings(IModConfig modConfig)
    {
        foreach (var (property, jsonProperty) in _JsonProperties)
        {
            var getSettingsMethod = _GetSettingsValueMethod.MakeGenericMethod(property.PropertyType);
            property.SetValue(this, getSettingsMethod.Invoke(modConfig, new[] { jsonProperty.PropertyName }));
        }
    }

    public string WebUIUrl
    {
        get => $"http://localhost:{_WebUIPort}/";
    }

    public string WebApiUrl
    {
        get => $"http://localgost:{_WebApiPort}/";
    }
}

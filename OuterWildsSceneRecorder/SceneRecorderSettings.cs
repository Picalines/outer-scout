using Newtonsoft.Json;
using OWML.Common;
using System.Linq;
using System.Reflection;

namespace Picalines.OuterWilds.SceneRecorder;

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
}

using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Models;

internal sealed class CameraInfo
{
    [JsonProperty("fov")]
    public required float FieldOfView { get; init; } // in degrees

    [JsonProperty("near_clip_plane")]
    public required float NearClipPlane { get; init; }

    [JsonProperty("far_clip_plane")]
    public required float FarClipPlane { get; init; }

    public static CameraInfo FromOWCamera(OWCamera owCamera)
    {
        return new()
        {
            FieldOfView = owCamera.fieldOfView,
            NearClipPlane = owCamera.nearClipPlane,
            FarClipPlane = owCamera.farClipPlane,
        };
    }

    public void ApplyToOWCamera(OWCamera owCamera)
    {
        owCamera.fieldOfView = FieldOfView;
        owCamera.nearClipPlane = NearClipPlane;
        owCamera.farClipPlane = FarClipPlane;
    }
}

using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Models;

internal sealed class CameraInfo
{
    [JsonProperty("fov")]
    public required float FieldOfView { get; init; }

    [JsonProperty("near_clip_plane")]
    public required float NearClipPlane { get; init; }

    [JsonProperty("far_clip_plane")]
    public required float FarClipPlane { get; init; }

    [JsonProperty("resolution_x")]
    public required float ResolutionX { get; init; }

    [JsonProperty("resolution_y")]
    public required float ResolutionY { get; init; }

    public static CameraInfo FromOWCamera(OWCamera owCamera)
    {
        return new()
        {
            FieldOfView = owCamera.fieldOfView,
            NearClipPlane = owCamera.nearClipPlane,
            FarClipPlane = owCamera.farClipPlane,
            ResolutionX = owCamera.pixelWidth,
            ResolutionY = owCamera.pixelHeight,
        };
    }

    public void ApplyToOWCamera(OWCamera owCamera)
    {
        owCamera.fieldOfView = FieldOfView;
        owCamera.nearClipPlane = NearClipPlane;
        owCamera.farClipPlane = FarClipPlane;

        var camera = owCamera.mainCamera;
        camera.pixelRect = new UnityEngine.Rect(0, 0, ResolutionX, ResolutionY);
    }
}

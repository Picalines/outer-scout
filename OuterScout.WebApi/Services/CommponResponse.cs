using OuterScout.WebApi.Http.Response;

namespace OuterScout.WebApi.Services;

using static ResponseFabric;

internal static class CommonResponse
{
    public static IResponse NotInPlayableScene { get; } =
        ServiceUnavailable(new { Error = "not in playable scene" });

    public static IResponse SceneIsNotCreated { get; } =
        ServiceUnavailable(new { Error = "not available, create a scene first" });

    public static IResponse RecordingIsInProgress { get; } =
        ServiceUnavailable(new { Error = "not available during recording" });

    public static IResponse GameObjectNotFound(string name) =>
        NotFound(new { Error = $"gameObject '{name}' was not found" });

    public static IResponse GameObjectIsNotCustom(string name) =>
        NotFound(new { Error = $"gameObject '{name}' is not custom" });

    public static IResponse CameraComponentNotFound(string gameObjectName) =>
        NotFound(
            new { Error = $"camera component of gameObject '{gameObjectName}' was not found" }
        );
}

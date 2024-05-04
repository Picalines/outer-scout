using OuterScout.WebApi.Http.Response;

namespace OuterScout.WebApi.Services;

using static ResponseFabric;

internal static class CommonResponse
{
    public static IResponse InvalidRouteParameter(string paramName, string? detail = null) =>
        BadRequest(
            new Problem("invalidRouteParameter")
            {
                Title = "Invalid route parameter",
                Detail =
                    $"The route parameter '{paramName}' is invalid"
                    + (detail is { } ? $": {detail}" : "")
            }
        );

    public static IResponse InvalidQueryParameter(string paramName, string? detail = null) =>
        BadRequest(
            new Problem("invalidQueryParameter")
            {
                Title = "Invalid query parameter",
                Detail =
                    $"The query parameter '{paramName}' is invalid"
                    + (detail is { } ? $": {detail}" : "")
            }
        );

    public static IResponse InvalidBodyField(string bodyPath, string? detail = null) =>
        BadRequest(
            new Problem("invalidRequestBody")
            {
                Title = "Invalid field in request body",
                Detail =
                    $"The request body field '{bodyPath}' is invalid"
                    + (detail is { } ? $": {detail}" : "")
            }
        );

    public static IResponse NotInPlayableScene { get; } =
        ServiceUnavailable(
            new Problem("notInPlayableScene")
            {
                Title = "Not in playable scene",
                Detail = "The operation is only available on playable scenes",
            }
        );

    public static IResponse SceneIsNotCreated { get; } =
        ServiceUnavailable(
            new Problem("sceneIsNotCreated")
            {
                Title = "Scene is not created",
                Detail = "The operation is not available, create a scene first",
            }
        );

    public static IResponse RecordingIsInProgress { get; } =
        ServiceUnavailable(
            new Problem("recordingIsInProgress")
            {
                Title = "Recording is in progress",
                Detail = "The operation is not available during recording",
            }
        );

    public static IResponse GameObjectNotFound(string name) =>
        NotFound(
            new Problem("gameObjectNotFound")
            {
                Title = "GameObject not found",
                Detail = $"The GameObject '{name}' was not found",
            }
        );

    public static IResponse GameObjectIsNotCustom(string name) =>
        NotFound(
            new Problem("gameObjectIsNotCustom")
            {
                Title = "GameObject is not custom",
                Detail =
                    $"The operation requires that the GameObject '{name}' be created using the API",
            }
        );

    public static IResponse CameraComponentNotFound(string gameObjectName) =>
        NotFound(
            new Problem("cameraComponentNotFound")
            {
                Title = "Camera component not found",
                Detail = $"Camera component of the GameObject '{gameObjectName}' was not found",
            }
        );
}

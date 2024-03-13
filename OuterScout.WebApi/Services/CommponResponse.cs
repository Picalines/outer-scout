using OuterScout.WebApi.Http.Response;

namespace OuterScout.WebApi.Services;

using static ResponseFabric;

internal static class CommonResponse
{
    public static IResponse GameObjectNotFound(string name) =>
        NotFound(new { Error = $"gameObject '{name}' was not found" });

    public static IResponse CameraNotFound(string id) =>
        NotFound(new { Error = $"camera '{id}' was not found" });
}

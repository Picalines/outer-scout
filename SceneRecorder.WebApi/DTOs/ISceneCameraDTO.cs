using JsonSubTypes;
using Newtonsoft.Json;

namespace SceneRecorder.WebApi.DTOs;

[JsonConverter(typeof(JsonSubtypes), nameof(ISceneCameraDTO.Type))]
internal interface ISceneCameraDTO
{
    public string Id { get; }

    public string Type { get; }
}

internal sealed class PerspectiveSceneCameraDTO : ISceneCameraDTO
{
    public required string Id { get; init; }

    public string Type { get; } = "perspective";

    public required ResolutionDTO Resolution { get; init; }
}

internal sealed class EquirectangularSceneCameraDTO : ISceneCameraDTO
{
    public required string Id { get; init; }

    public string Type { get; } = "equirectangular";

    public required int Resolution { get; init; }
}

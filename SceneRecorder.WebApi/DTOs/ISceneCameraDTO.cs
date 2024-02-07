using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;

namespace SceneRecorder.WebApi.DTOs;

[JsonConverter(typeof(JsonSubtypes), nameof(ISceneCameraDTO.Type))]
internal interface ISceneCameraDTO
{
    public string Id { get; }

    public string Type { get; }

    public TransformDTO Transform { get; }
}

internal sealed class ResolutionDTO
{
    public required int Width { get; init; }

    public required int Height { get; init; }
}

internal sealed class PerspectiveSceneCameraDTO : ISceneCameraDTO
{
    public required string Id { get; init; }

    public string Type { get; } = "perspective";

    public required TransformDTO Transform { get; init; }

    public required Camera.GateFitMode GateFit { get; init; }

    public required ResolutionDTO Resolution { get; init; }

    public required CameraPerspectiveDTO Perspective { get; init; }
}

internal sealed class EquirectSceneCameraDTO : ISceneCameraDTO
{
    public required string Id { get; init; }

    public string Type { get; } = "equirectangular";

    public required TransformDTO Transform { get; init; }

    public required int Resolution { get; init; }
}

using JsonSubTypes;
using Newtonsoft.Json;
using OuterScout.Domain;
using UnityEngine;

namespace OuterScout.WebApi.DTOs;

[JsonConverter(typeof(JsonSubtypes), nameof(ISceneCameraDTO.Type))]
[JsonSubtypes.KnownSubType(typeof(PerspectiveSceneCameraDTO), "perspective")]
[JsonSubtypes.KnownSubType(typeof(EquirectSceneCameraDTO), "equirectangular")]
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

    public required string Type { get; init; } = "perspective";

    public required TransformDTO Transform { get; init; }

    public required Camera.GateFitMode GateFit { get; init; }

    public required ResolutionDTO Resolution { get; init; }

    public required CameraPerspective Perspective { get; init; }
}

internal sealed class EquirectSceneCameraDTO : ISceneCameraDTO
{
    public required string Id { get; init; }

    public required string Type { get; init; } = "equirectangular";

    public required TransformDTO Transform { get; init; }

    public required int FaceResolution { get; init; }
}

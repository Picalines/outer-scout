using OuterScout.Application.Extensions;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.Endpoints;

using static ResponseFabric;

internal sealed class PlayerEndpoint : IRouteMapper
{
    public static PlayerEndpoint Instance { get; } = new();

    private PlayerEndpoint() { }

    private sealed class WarpRequest
    {
        public required string GroundBody { get; init; }

        public required TransformDto Transform { get; init; }
    }

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            serverBuilder.MapGet("player/ground-body", GetPlayerGroundBody);

            serverBuilder.MapGet("player/sectors", GetPlayerSectors);

            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("player/warp", WarpToGroundBody);
            }
        }
    }

    private static IResponse GetPlayerGroundBody()
    {
        if (
            LocatorExtensions.GetLastGroundBody()
            is not { name: var name, transform: var transform }
        )
        {
            return NotFound(
                new Problem("groundBodyNotFound")
                {
                    Title = "The ground body not found",
                    Detail = "The ground body not found. Try to jump and land on the body"
                }
            );
        }

        return Ok(
            new
            {
                Name = name,
                Transform = new TransformDto()
                {
                    Position = transform.position,
                    Rotation = transform.rotation,
                    Scale = transform.lossyScale,
                }
            }
        );
    }

    private static IResponse GetPlayerSectors()
    {
        if (
            Locator.GetPlayerDetector().OrNull()?.GetComponent<SectorDetector>()
            is not { } sectorDetector
        )
        {
            return ServiceUnavailable();
        }

        return Ok(
            new
            {
                LastEntered = sectorDetector
                    ._sectorList.Select(sector => sector.transform)
                    .LastOrDefault()
                    ?.GetPath(),
                Sectors = sectorDetector
                    ._sectorList.Select(sector => new
                    {
                        Name = sector.name.ToString(),
                        Id = sector.GetIDString() is { Length: > 0 } id ? id : null,
                    })
                    .ToArray(),
            }
        );
    }

    private static IResponse WarpToGroundBody(
        [FromBody] WarpRequest request,
        GameObjectRepository gameObjects
    )
    {
        if (
            LocatorExtensions.GetPlayerSpawner() is not { } playerSpawner
            || Locator.GetPlayerBody().OrNull() is not { transform: var playerTransform }
        )
        {
            return ServiceUnavailable();
        }

        if (
            request.Transform
            is not {
                Parent: null,
                Position: { } spawnLocalPosition,
                Rotation: { } spawnLocalRotation,
                Scale: null
            }
        )
        {
            return CommonResponse.InvalidBodyField(
                "transform",
                "must not contain parent and scale, must contain position and rotation"
            );
        }

        if (
            gameObjects.FindOrNull(request.GroundBody) is not { transform: var spawnParent }
            || spawnParent.GetComponentOrNull<OWRigidbody>() is not { } groundBody
        )
        {
            return BadRequest(
                new Problem("invalidWarpTarget")
                {
                    Detail = $"'{request.GroundBody}' is not a valid ground body"
                }
            );
        }

        var spawnPosition = spawnParent.TransformPoint(spawnLocalPosition);
        var spawnRotation = spawnParent.rotation * spawnLocalRotation;

        var nearestSpawnPoint = spawnParent
            .GetComponentsInChildren<SpawnPoint>()
            .OrderBy(point => (point.transform.position - spawnPosition).magnitude)
            .FirstOrDefault();

        var spawnPoint = nearestSpawnPoint is not null
            ? UnityEngine.Object.Instantiate(nearestSpawnPoint)
            : new GameObject("", typeof(SpawnPoint)).GetComponent<SpawnPoint>();

        spawnPoint.name = $"{nameof(OuterScout)}.SpawnPoint";
        spawnPoint.transform.parent = spawnParent;
        spawnPoint._isShipSpawn = false;
        spawnPoint._attachedBody = groundBody;
        spawnPoint.transform.position = spawnPosition;
        spawnPoint.transform.rotation = spawnRotation;

        UnityEngine.Object.Destroy(spawnPoint, 10);
        UnpauseForSeconds(0.5f);
        AssignDebugDreamLantern();

        playerSpawner.DebugWarp(spawnPoint);

        return Ok();
    }

    // DebugWarp works in the Update loop
    private static void UnpauseForSeconds(float seconds)
    {
        var pauseCommandListener = Locator.GetPauseCommandListener().OrNull();
        var pauseMenuManager = pauseCommandListener?._pauseMenu;
        var pauseMenu = pauseMenuManager?._pauseMenu;

        if (pauseMenu?.IsMenuEnabled() is not true)
        {
            return;
        }

        pauseMenu.EnableMenu(false);
        pauseMenuManager?.Invoke(nameof(PauseMenuManager.TryOpenPauseMenu), seconds);
    }

    // DebugWarp throws when you attempt to warp to the DreamWorld without a lantern in hands
    private static void AssignDebugDreamLantern()
    {
        if (
            Locator.GetDreamWorldController().OrNull() is not { } dreamWorldController
            || dreamWorldController._debugPlayerLantern.OrNull() is not null
        )
        {
            return;
        }

        dreamWorldController._debugPlayerLantern = GameObject
            .FindObjectsOfType<DreamLanternItem>()
            .FirstOrDefault(lantern =>
                lantern._lanternType.HasFlag(DreamLanternType.Functioning) is true
            );
    }
}

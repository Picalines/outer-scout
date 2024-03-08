using OuterScout.Application.Extensions;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.DTOs;
using OuterScout.WebApi.Extensions;
using OuterScout.WebApi.Http;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Services;
using UnityEngine;

namespace OuterScout.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class PlayerRouteMapper : IRouteMapper
{
    public static PlayerRouteMapper Instance { get; } = new();

    private PlayerRouteMapper() { }

    private sealed class WarpRequest
    {
        public required string GroundBody { get; init; }

        public required TransformDTO Transform { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
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
            LocatorExtensions.GetCurrentGroundBody()
            is not { name: var name, transform: var transform }
        )
        {
            return ServiceUnavailable();
        }

        return Ok(
            new
            {
                Name = name,
                Transform = new TransformDTO()
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
                Current = sectorDetector.GetLastEnteredSector().transform.GetPath(),
                Sectors = sectorDetector
                    ._sectorList.Select(sector => sector.transform.GetPath())
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

        if (request.Transform is not { Parent: null, Position: { }, Rotation: { }, Scale: null })
        {
            return BadRequest("invalid warp transform");
        }

        if (
            gameObjects.FindOrNull(request.GroundBody) is not { transform: var groundBodyTransform }
            || groundBodyTransform.GetComponentOrNull<OWRigidbody>() is not { } groundBody
        )
        {
            return BadRequest($"'{request.GroundBody}' is not a valid ground body");
        }

        var localTransform = new GameObject().transform;
        localTransform.parent = groundBodyTransform;
        localTransform.ApplyKeepParent(request.Transform.ToLocalTransform(groundBodyTransform));

        const string spawnPointName = $"{nameof(OuterScout)}.SpawnPoint";

        var spawnPoint = groundBodyTransform
            .Find(spawnPointName)
            .OrNull()
            ?.GetComponent<SpawnPoint>();

        if (spawnPoint is null)
        {
            var nearestSpawnPoint = groundBodyTransform
                .GetComponentsInChildren<SpawnPoint>()
                .OrderBy(point => (point.transform.position - localTransform.position).magnitude)
                .FirstOrDefault();

            var newSpawnPointObject = nearestSpawnPoint is not null
                ? UnityEngine.Object.Instantiate(nearestSpawnPoint.gameObject)
                : new GameObject(spawnPointName, typeof(SpawnPoint));

            newSpawnPointObject.name = spawnPointName;
            newSpawnPointObject.transform.parent = groundBodyTransform;

            spawnPoint = newSpawnPointObject.GetComponent<SpawnPoint>();
        }

        spawnPoint._isShipSpawn = false;
        spawnPoint._attachedBody = groundBody;
        spawnPoint.transform.position = localTransform.position;
        spawnPoint.transform.rotation = localTransform.rotation;

        UnityEngine.Object.Destroy(localTransform.gameObject);

        playerSpawner.DebugWarp(spawnPoint);

        return Ok();
    }
}

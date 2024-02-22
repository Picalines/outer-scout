using SceneRecorder.Application.Extensions;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class PlayerRouteMapper : IRouteMapper
{
    public static PlayerRouteMapper Instance { get; } = new();

    private const string ReusedSpawnPointName = $"{nameof(SceneRecorder)}.SpawnPoint";

    private PlayerRouteMapper() { }

    private sealed class WarpRequest
    {
        public required TransformDTO Transform { get; init; }
    }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            serverBuilder.MapGet("player/sectors", GetPlayerSectors);

            using (serverBuilder.WithNotRecordingFilter())
            {
                serverBuilder.MapPost("player/warp", WarpToGroundBody);
            }
        }
    }

    private static IResponse GetPlayerSectors()
    {
        var sectorDetector = Locator.GetPlayerDetector().OrNull()?.GetComponent<SectorDetector>();

        if (sectorDetector is null)
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

    private static IResponse WarpToGroundBody([FromBody] WarpRequest request)
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
            is not { Parent: { } groundBodyName, Position: { }, Rotation: { }, Scale: null }
        )
        {
            return BadRequest("invalid warp transform");
        }

        if (
            GameObject.Find(groundBodyName).OrNull() is not { transform: var groundBodyTransform }
            || groundBodyTransform.GetComponent<OWRigidbody>().OrNull() is not { } groundBody
        )
        {
            return BadRequest($"'{groundBodyName}' is not a valid ground body");
        }

        var localTransform = new GameObject().transform;
        localTransform.parent = groundBodyTransform;
        localTransform.Apply(request.Transform.ToLocalTransform(groundBodyTransform));

        var spawnPoint = groundBodyTransform
            .Find(ReusedSpawnPointName)
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
                : new GameObject(ReusedSpawnPointName, typeof(SpawnPoint));

            newSpawnPointObject.name = ReusedSpawnPointName;
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

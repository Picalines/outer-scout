using SceneRecorder.Shared.Extensions;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.RouteMappers.DTOs;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class WarpRouteMapper : IRouteMapper
{
    private const string ModSpawnPointName = $"__{nameof(SceneRecorder)}_SpawnPoint";

    public static WarpRouteMapper Instance { get; } = new();

    private WarpRouteMapper() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IRouteMapper.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.MapPost(
            ":groundBodyName/warp",
            (string groundBodyName, WarpRequest request) =>
            {
                var playerSpawner = LocatorExtensions.GetPlayerSpawner();
                if (playerSpawner is null)
                {
                    return ServiceUnavailable();
                }

                var groundBodyTransform = GameObject.Find(groundBodyName).OrNull()?.transform;
                var groundBody = groundBodyTransform?.GetComponent<OWRigidbody>();
                if ((groundBodyTransform, groundBody) is not ({ }, { }))
                {
                    return BadRequest($"ground body \"{groundBodyName}\" not found");
                }

                var playerTransform = Locator.GetPlayerBody().transform;

                var localTransform = new GameObject().transform;
                localTransform.SetParent(groundBodyTransform, false);
                request.LocalTransform.ApplyLocal(localTransform);

                var spawnPoint = groundBodyTransform
                    .Find(ModSpawnPointName)
                    .OrNull()
                    ?.GetComponent<SpawnPoint>();

                if (spawnPoint is null)
                {
                    var referenceSpawnPoint = groundBodyTransform
                        .GetComponentsInChildren<SpawnPoint>()
                        .MinByOrDefault(
                            point => (point.transform.position - localTransform.position).magnitude
                        );

                    GameObject newSpawnPointGameObject;

                    if (referenceSpawnPoint is null)
                    {
                        newSpawnPointGameObject = new GameObject(ModSpawnPointName);
                        newSpawnPointGameObject.AddComponent<SpawnPoint>();
                    }
                    else
                    {
                        newSpawnPointGameObject = UnityEngine.Object.Instantiate(
                            referenceSpawnPoint.gameObject
                        );
                    }

                    newSpawnPointGameObject.transform.SetParent(groundBodyTransform);
                    newSpawnPointGameObject.name = ModSpawnPointName;

                    spawnPoint = newSpawnPointGameObject.GetComponent<SpawnPoint>();
                }

                spawnPoint._isShipSpawn = false;
                spawnPoint._attachedBody = groundBody;
                spawnPoint.transform.position = localTransform.position;
                spawnPoint.transform.rotation = localTransform.rotation;

                playerSpawner.DebugWarp(spawnPoint);

                UnityEngine.Object.Destroy(localTransform.gameObject);

                return Ok();
            }
        );
    }
}
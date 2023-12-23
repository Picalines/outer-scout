using Picalines.OuterWilds.SceneRecorder.Shared.Extensions;
using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Extensions;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class WarpRouteDefinition : IApiRouteDefinition
{
    private const string ModSpawnPointName = $"__{nameof(SceneRecorder)}_SpawnPoint";

    public static WarpRouteDefinition Instance { get; } = new();

    private WarpRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        using var precondition = serverBuilder.UseInPlayableScenePrecondition();

        serverBuilder.Map(
            HttpMethod.Post,
            "warp_to/:ground_body_name",
            (Request request, string ground_body_name) =>
            {
                var playerSpawner = LocatorExtensions.GetPlayerSpawner();
                if (playerSpawner is null)
                {
                    return ResponseFabric.ServiceUnavailable();
                }

                var groundBodyTransform = GameObject.Find(ground_body_name).OrNull()?.transform;
                var groundBody = groundBodyTransform?.GetComponent<OWRigidbody>();
                if ((groundBodyTransform, groundBody) is not ({ }, { }))
                {
                    return ResponseFabric.BadRequest(
                        $"ground body \"{ground_body_name}\" not found"
                    );
                }

                var localTransformModel = request.ParseContentJson<TransformModel>();
                var playerTransform = Locator.GetPlayerBody().transform;

                var localTransform = new GameObject().transform;
                localTransform.SetParent(groundBodyTransform, false);
                localTransformModel.ApplyToLocalTransform(localTransform);

                var spawnPoint = groundBodyTransform
                    .Find(ModSpawnPointName)
                    .OrNull()
                    ?.GetComponent<SpawnPoint>();

                if (spawnPoint is null)
                {
                    var referenceSpawnPoint = groundBodyTransform
                        .GetComponentsInChildren<SpawnPoint>()
                        .Select(
                            point =>
                                new
                                {
                                    point,
                                    distanceToPlayer = (
                                        point.transform.position - localTransform.position
                                    ).magnitude
                                }
                        )
                        .MinByOrDefault(pair => pair.distanceToPlayer)
                        ?.point;

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

                return ResponseFabric.Ok();
            }
        );
    }
}

using SceneRecorder.Application.Extensions;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.DTOs;
using SceneRecorder.WebApi.Extensions;
using SceneRecorder.WebApi.Http;
using SceneRecorder.WebApi.Http.Response;
using UnityEngine;

namespace SceneRecorder.WebApi.RouteMappers;

using static ResponseFabric;

internal sealed class GroundBodyRouteMapper : IRouteMapper
{
    public static GroundBodyRouteMapper Instance { get; } = new();

    private GroundBodyRouteMapper() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        using (serverBuilder.WithNotRecordingFilter())
        {
            serverBuilder.MapGet("player/ground-body", GetPlayerGroundBody);

            serverBuilder.MapGet("gameObjects/:name/mesh", GetGameObjectMesh);
        }
    }

    private static IResponse GetPlayerGroundBody()
    {
        return LocatorExtensions.GetCurrentGroundBody() switch
        {
            { name: var name, transform: var transform }
                => Ok(
                    new GameObjectDTO
                    {
                        Name = name,
                        Path = transform.GetPath(),
                        Transform = ToGlobalTransformDTO(transform)
                    }
                ),
            _ => NotFound(),
        };
    }

    private static IResponse GetGameObjectMesh(string name)
    {
        return GameObject.Find(name).OrNull() switch
        {
            { } gameObject => Ok(GetBodyMeshDTO(gameObject)),
            _ => NotFound(),
        };
    }

    private static BodyMeshDTO GetBodyMeshDTO(GameObject body)
    {
        var bodyTransform = body.transform;

        var renderedMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(body)
            .Where(pair => pair.Component.TryGetComponent<Renderer>(out _) is true);

        var noSectorMeshInfo = CreateEmptySectorDTO(bodyTransform.GetPath());
        var sectorMeshInfos = new Dictionary<Sector, SectorMeshDTO>();

        foreach (var (sector, meshFilter) in renderedMeshFilters)
        {
            var sectorMeshInfo = sector is null
                ? noSectorMeshInfo
                : GetOrCreate(sectorMeshInfos, sector, CreateEmptySectorDTO);

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var globalMeshTransform = ToGlobalTransformDTO(meshTransform);
            var localMeshTrasnform = ToLocalTransformDTO(bodyTransform, meshTransform);

            if (
                StreamingManager.s_tableLoaded
                && meshFilter.TryGetComponent<StreamingMeshHandle>(out var streamingHandle)
                && StreamingManager.s_streamingAssetBundleMap.TryGetValue(
                    streamingHandle.assetBundle,
                    out var assetBundle
                )
                && assetBundle is StreamingMeshAssetBundle { isLoaded: true } meshAssetBundle
            )
            {
                var streamedMeshes = (sectorMeshInfo.StreamedMeshes as List<MeshDTO>)!;
                streamedMeshes.Add(
                    new()
                    {
                        Path = meshAssetBundle._meshNamesByID[streamingHandle.meshIndex],
                        GlobalTransform = globalMeshTransform,
                        LocalTransform = localMeshTrasnform,
                    }
                );
            }
            else
            {
                var plainMeshes = (sectorMeshInfo.PlainMeshes as List<MeshDTO>)!;
                plainMeshes.Add(
                    new()
                    {
                        Path = meshGameObject.transform.GetPath(),
                        GlobalTransform = globalMeshTransform,
                        LocalTransform = localMeshTrasnform,
                    }
                );
            }
        }

        var sectorMeshInfosList = new List<SectorMeshDTO>();
        if (noSectorMeshInfo is not { PlainMeshes.Count: 0, StreamedMeshes.Count: 0 })
        {
            sectorMeshInfosList.Add(noSectorMeshInfo);
        }

        sectorMeshInfosList.AddRange(sectorMeshInfos.Values);

        return new BodyMeshDTO()
        {
            Body = new()
            {
                Name = body.name,
                Path = bodyTransform.GetPath(),
                Transform = ToGlobalTransformDTO(bodyTransform),
            },
            Sectors = sectorMeshInfosList,
        };
    }

    private static SectorMeshDTO CreateEmptySectorDTO(string path)
    {
        return new()
        {
            Path = path,
            PlainMeshes = new List<MeshDTO>(),
            StreamedMeshes = new List<MeshDTO>(),
        };
    }

    private static SectorMeshDTO CreateEmptySectorDTO(Sector sector)
    {
        return CreateEmptySectorDTO(sector.transform.GetPath());
    }

    private static TransformDTO ToGlobalTransformDTO(Transform transform) =>
        new()
        {
            Position = transform.position,
            Rotation = transform.rotation,
            Scale = transform.lossyScale,
        };

    private static TransformDTO ToLocalTransformDTO(
        Transform parentTransform,
        Transform childTransform
    ) =>
        new()
        {
            Parent = parentTransform.name,
            Position = parentTransform.InverseTransformPoint(childTransform.position),
            Rotation = parentTransform.InverseTransformRotation(childTransform.rotation),
            Scale = childTransform.lossyScale,
        };

    private static IEnumerable<(Sector? Sector, T Component)> GetComponentsInChildrenWithSector<T>(
        GameObject gameObject,
        Sector? parentSector = null
    )
        where T : Component
    {
        Sector? sector = gameObject.GetComponent<Sector>() ?? parentSector;

        foreach (T component in gameObject.GetComponents<T>())
        {
            yield return (sector, component);
        }

        foreach (Transform child in gameObject.transform)
        {
            foreach (
                var recursivePair in GetComponentsInChildrenWithSector<T>(child.gameObject, sector)
            )
            {
                yield return recursivePair;
            }
        }
    }

    private static V GetOrCreate<K, V>(IDictionary<K, V> dictionary, K key, Func<K, V> createValue)
    {
        if (dictionary.TryGetValue(key, out V value) is false)
        {
            value = createValue(key);
            dictionary.Add(key, value);
        }

        return value;
    }
}

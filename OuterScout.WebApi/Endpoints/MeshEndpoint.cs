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

internal sealed class MeshEndpoint : IRouteMapper
{
    public static MeshEndpoint Instance { get; } = new();

    private MeshEndpoint() { }

    public void MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            serverBuilder.MapGet("gameObjects/:name/mesh", GetGameObjectMesh);
        }
    }

    private static IResponse GetGameObjectMesh(
        [FromUrl] string name,
        GameObjectRepository gameObjects
    )
    {
        return gameObjects.FindOrNull(name) is { } gameObject
            ? Ok(GetBodyMeshDTO(gameObject))
            : CommonResponse.GameObjectNotFound(name);
    }

    private sealed class GameObjectMeshDTO
    {
        public required GameObjectDTO Body { get; init; }

        public required IReadOnlyList<SectorMeshDTO> Sectors { get; init; }
    }

    private sealed class GameObjectDTO
    {
        public required string Name { get; init; }

        public required string Path { get; init; }

        public required TransformDTO Transform { get; init; }
    }

    private sealed class SectorMeshDTO
    {
        public required string Path { get; init; }

        public required IReadOnlyList<MeshDTO> PlainMeshes { get; init; }

        public required IReadOnlyList<MeshDTO> StreamedMeshes { get; init; }
    }

    private sealed class MeshDTO
    {
        // GameObject path for "static" meshes, Asset path for streamed ones
        public required string Path { get; init; }

        public required TransformDTO GlobalTransform { get; init; }

        public required TransformDTO LocalTransform { get; init; }
    }

    private static GameObjectMeshDTO GetBodyMeshDTO(GameObject body)
    {
        var bodyTransform = body.transform;

        var renderedMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(body)
            .Where(pair => pair.Component.HasComponent<Renderer>() is true);

        var noSectorMeshInfo = CreateEmptySectorDTO(bodyTransform.GetPath());
        var sectorMeshInfos = new Dictionary<Sector, SectorMeshDTO>();

        foreach (var (sector, meshFilter) in renderedMeshFilters)
        {
            var sectorMeshInfo = sector is null
                ? noSectorMeshInfo
                : sectorMeshInfos.GetOrCreate(sector, () => CreateEmptySectorDTO(sector));

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var globalMeshTransform = ToGlobalTransformDTO(meshTransform);
            var localMeshTrasnform = ToLocalTransformDTO(bodyTransform, meshTransform);

            if (
                StreamingManager.s_tableLoaded
                && meshFilter.GetComponentOrNull<StreamingMeshHandle>() is { } streamingHandle
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

        return new GameObjectMeshDTO()
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
}

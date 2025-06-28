using OuterScout.Application.Extensions;
using OuterScout.Shared.Extensions;
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

    void IRouteMapper.MapRoutes(HttpServer.Builder serverBuilder)
    {
        using (serverBuilder.WithPlayableSceneFilter())
        {
            serverBuilder.MapGet("objects/:name/mesh", GetGameObjectMesh);
        }
    }

    private static IResponse GetGameObjectMesh(
        [FromUrl] string name,
        GameObjectRepository gameObjects,
        [FromUrl] string ignorePaths = ":",
        [FromUrl] string ignoreLayers = "",
        [FromUrl] bool caseSensitive = false
    )
    {
        return gameObjects.FindOrNull(name) is { } gameObject
            ? Ok(
                GetBodyMeshDto(
                    gameObject,
                    new FilterParameters()
                    {
                        CaseSensitive = caseSensitive,
                        IgnorePaths = new HashSet<string>(ignorePaths.Split(',')),
                        IgnoreLayers = new HashSet<string>(ignoreLayers.Split(',')),
                    }
                )
            )
            : CommonResponse.GameObjectNotFound(name);
    }

    private sealed class GameObjectMeshDto
    {
        public required GameObjectDto Body { get; init; }

        public required IReadOnlyList<SectorDto> Sectors { get; init; }
    }

    private sealed class GameObjectDto
    {
        public required string Name { get; init; }

        public required string Path { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private sealed class SectorDto
    {
        public required string Path { get; init; }

        public required IReadOnlyList<MeshAssetDto> PlainMeshes { get; init; }

        public required IReadOnlyList<MeshAssetDto> StreamedMeshes { get; init; }
    }

    private sealed class MeshAssetDto
    {
        // GameObject path for "static" meshes, Asset path for streamed ones
        public required string Path { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private sealed class FilterParameters
    {
        public required bool CaseSensitive { get; init; }

        public required IEnumerable<string> IgnorePaths { get; init; }

        public required IEnumerable<string> IgnoreLayers { get; init; }
    }

    private static GameObjectMeshDto GetBodyMeshDto(GameObject body, FilterParameters filter)
    {
        var stringComparison = filter.CaseSensitive
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var ignoredLayers = new HashSet<int>(filter.IgnoreLayers.Select(LayerMask.NameToLayer));

        var bodyTransform = body.transform;

        var sectoredMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(body)
            .Where(pair => pair.Component.HasComponent<Renderer>() is true)
            .Where(pair => ignoredLayers.Contains(pair.Component.gameObject.layer) is false);

        var noSectorMeshInfo = CreateEmptySectorDto(bodyTransform.GetPath());
        var sectorMeshInfos = new Dictionary<Sector, SectorDto>();

        foreach (var (sector, meshFilter) in sectoredMeshFilters)
        {
            var sectorMeshInfo = sector is null
                ? noSectorMeshInfo
                : sectorMeshInfos.GetOrCreate(sector, () => CreateEmptySectorDto(sector));

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            List<MeshAssetDto> meshList;
            string meshPath;

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
                meshList = (sectorMeshInfo.StreamedMeshes as List<MeshAssetDto>)!;
                meshPath = meshAssetBundle._meshNamesByID[streamingHandle.meshIndex];
            }
            else
            {
                meshList = (sectorMeshInfo.PlainMeshes as List<MeshAssetDto>)!;
                meshPath = meshGameObject.transform.GetPath();
            }

            if (
                filter.IgnorePaths.Any(part => meshPath.IndexOf(part, stringComparison) is >= 0)
                is false
            )
            {
                meshList.Add(
                    new MeshAssetDto()
                    {
                        Path = meshPath,
                        Transform = bodyTransform.InverseDto(meshTransform),
                    }
                );
            }
        }

        var sectorMeshInfosList = new List<SectorDto>();
        if (noSectorMeshInfo is not { PlainMeshes.Count: 0, StreamedMeshes.Count: 0 })
        {
            sectorMeshInfosList.Add(noSectorMeshInfo);
        }

        sectorMeshInfosList.AddRange(sectorMeshInfos.Values);

        return new GameObjectMeshDto()
        {
            Body = new()
            {
                Name = body.name,
                Path = bodyTransform.GetPath(),
                Transform = bodyTransform.GlobalDto(),
            },
            Sectors = sectorMeshInfosList,
        };
    }

    private static SectorDto CreateEmptySectorDto(string path)
    {
        return new()
        {
            Path = path,
            PlainMeshes = new List<MeshAssetDto>(),
            StreamedMeshes = new List<MeshAssetDto>(),
        };
    }

    private static SectorDto CreateEmptySectorDto(Sector sector)
    {
        return CreateEmptySectorDto(sector.transform.GetPath());
    }

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

﻿using OuterScout.Application.Extensions;
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
            ? Ok(GetBodyMeshDto(gameObject))
            : CommonResponse.GameObjectNotFound(name);
    }

    private sealed class GameObjectMeshDto
    {
        public required GameObjectDto Body { get; init; }

        public required IReadOnlyList<SectorMeshDto> Sectors { get; init; }
    }

    private sealed class GameObjectDto
    {
        public required string Name { get; init; }

        public required string Path { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private sealed class SectorMeshDto
    {
        public required string Path { get; init; }

        public required IReadOnlyList<MeshDto> PlainMeshes { get; init; }

        public required IReadOnlyList<MeshDto> StreamedMeshes { get; init; }
    }

    private sealed class MeshDto
    {
        // GameObject path for "static" meshes, Asset path for streamed ones
        public required string Path { get; init; }

        public required TransformDto Transform { get; init; }
    }

    private static GameObjectMeshDto GetBodyMeshDto(GameObject body)
    {
        var bodyTransform = body.transform;

        var renderedMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(body)
            .Where(pair => pair.Component.HasComponent<Renderer>() is true);

        var noSectorMeshInfo = CreateEmptySectorDto(bodyTransform.GetPath());
        var sectorMeshInfos = new Dictionary<Sector, SectorMeshDto>();

        foreach (var (sector, meshFilter) in renderedMeshFilters)
        {
            var sectorMeshInfo = sector is null
                ? noSectorMeshInfo
                : sectorMeshInfos.GetOrCreate(sector, () => CreateEmptySectorDto(sector));

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var localMeshTrasnform = bodyTransform.InverseDto(meshTransform);

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
                var streamedMeshes = (sectorMeshInfo.StreamedMeshes as List<MeshDto>)!;
                streamedMeshes.Add(
                    new()
                    {
                        Path = meshAssetBundle._meshNamesByID[streamingHandle.meshIndex],
                        Transform = localMeshTrasnform,
                    }
                );
            }
            else
            {
                var plainMeshes = (sectorMeshInfo.PlainMeshes as List<MeshDto>)!;
                plainMeshes.Add(
                    new()
                    {
                        Path = meshGameObject.transform.GetPath(),
                        Transform = localMeshTrasnform,
                    }
                );
            }
        }

        var sectorMeshInfosList = new List<SectorMeshDto>();
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

    private static SectorMeshDto CreateEmptySectorDto(string path)
    {
        return new()
        {
            Path = path,
            PlainMeshes = new List<MeshDto>(),
            StreamedMeshes = new List<MeshDto>(),
        };
    }

    private static SectorMeshDto CreateEmptySectorDto(Sector sector)
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

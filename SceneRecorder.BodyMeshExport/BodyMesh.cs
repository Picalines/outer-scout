﻿using SceneRecorder.Shared.Extensions;
using SceneRecorder.Shared.Models;
using UnityEngine;

namespace SceneRecorder.BodyMeshExport;

public static class BodyMesh
{
    public static BodyMeshDTO GetDTO(GameObject body)
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

            var globalMeshTransform = TransformDTO.FromGlobal(meshTransform);
            var localMeshTrasnform = TransformDTO.FromInverse(bodyTransform, meshTransform);

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
                Transform = TransformDTO.FromGlobal(bodyTransform),
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
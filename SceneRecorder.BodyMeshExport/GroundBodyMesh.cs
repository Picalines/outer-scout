using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Shared.Models;
using UnityEngine;

namespace SceneRecorder.BodyMeshExport;

public static class GroundBodyMesh
{
    public static GroundBodyMeshDTO GetDTO(GameObject groundBodyObject)
    {
        var renderedMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(groundBodyObject)
            .Where(pair => pair.Component.TryGetComponent<Renderer>(out _) is true);

        var noSectorMeshInfo = CreateEmptySectorMeshInfo(groundBodyObject.transform.GetPath());
        var sectorMeshInfos = new Dictionary<Sector, SectorMeshDTO>();

        foreach (var (sector, meshFilter) in renderedMeshFilters)
        {
            var sectorMeshInfo = sector is null
                ? noSectorMeshInfo
                : GetOrCreate(sectorMeshInfos, sector, CreateEmptySectorMeshInfoFromSector);

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var transformData = TransformDTO.FromGlobal(meshTransform);

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
                    new MeshDTO()
                    {
                        Path = meshAssetBundle._meshNamesByID[streamingHandle.meshIndex],
                        Transform = transformData,
                    }
                );
            }
            else
            {
                var plainMeshes = (sectorMeshInfo.PlainMeshes as List<MeshDTO>)!;
                plainMeshes.Add(
                    new MeshDTO()
                    {
                        Path = meshGameObject.transform.GetPath(),
                        Transform = transformData,
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

        return new GroundBodyMeshDTO()
        {
            BodyName = groundBodyObject.name,
            BodyTransform = TransformDTO.FromGlobal(groundBodyObject.transform),
            Sectors = sectorMeshInfosList,
        };

        static SectorMeshDTO CreateEmptySectorMeshInfo(string path)
        {
            return new SectorMeshDTO()
            {
                Path = path,
                PlainMeshes = new List<MeshDTO>(),
                StreamedMeshes = new List<MeshDTO>(),
            };
        }

        static SectorMeshDTO CreateEmptySectorMeshInfoFromSector(Sector sector)
        {
            return CreateEmptySectorMeshInfo(sector.transform.GetPath());
        }
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

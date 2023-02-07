using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public static class GroundBodyMeshExport
{
    public static GroundBodyMeshInfo CaptureMeshInfo(GameObject groundBodyObject)
    {
        var renderedMeshFilters = GetComponentsInChildrenWithSector<MeshFilter>(groundBodyObject)
            .Where(pair => pair.Component.TryGetComponent<Renderer>(out _) is true);

        var noSectorMeshInfo = CreateEmptySectorMeshInfo(GetTransformPath(groundBodyObject.transform));
        var sectorMeshInfos = new Dictionary<Sector, SectorMeshInfo>();

        foreach (var (sector, meshFilter) in renderedMeshFilters)
        {
            var sectorMeshInfo = sector is null ? noSectorMeshInfo : GetOrCreate(sectorMeshInfos, sector, CreateEmptySectorMeshInfoFromSector);

            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var transformData = TransformModel.FromGlobalTransform(meshTransform);

            if (StreamingManager.s_tableLoaded
                && meshFilter.TryGetComponent<StreamingMeshHandle>(out var streamingHandle)
                && StreamingManager.s_streamingAssetBundleMap.TryGetValue(streamingHandle.assetBundle, out var assetBundle)
                && assetBundle is StreamingMeshAssetBundle { isLoaded: true } meshAssetBundle)
            {
                var streamedMeshes = (sectorMeshInfo.StreamedMeshes as List<MeshInfo>)!;
                streamedMeshes.Add(new MeshInfo()
                {
                    Path = meshAssetBundle._meshNamesByID[streamingHandle.meshIndex],
                    Transform = transformData,
                });
            }
            else
            {
                var plainMeshes = (sectorMeshInfo.PlainMeshes as List<MeshInfo>)!;
                plainMeshes.Add(new MeshInfo()
                {
                    Path = GetTransformPath(meshGameObject.transform),
                    Transform = transformData,
                });
            }
        }

        var sectorMeshInfosList = new List<SectorMeshInfo>();
        if (noSectorMeshInfo is not { PlainMeshes.Count: 0, StreamedMeshes.Count: 0 })
        {
            sectorMeshInfosList.Add(noSectorMeshInfo);
        }
        sectorMeshInfosList.AddRange(sectorMeshInfos.Values);

        return new GroundBodyMeshInfo()
        {
            BodyName = groundBodyObject.name,
            BodyTransform = TransformModel.FromGlobalTransform(groundBodyObject.transform),
            Sectors = sectorMeshInfosList,
        };

        static SectorMeshInfo CreateEmptySectorMeshInfo(string path)
        {
            return new SectorMeshInfo()
            {
                Path = path,
                PlainMeshes = new List<MeshInfo>(),
                StreamedMeshes = new List<MeshInfo>(),
            };
        }

        static SectorMeshInfo CreateEmptySectorMeshInfoFromSector(Sector sector)
        {
            return CreateEmptySectorMeshInfo(GetTransformPath(sector.transform));
        }
    }

    private static IEnumerable<(Sector? Sector, T Component)> GetComponentsInChildrenWithSector<T>(GameObject gameObject, Sector? parentSector = null)
        where T : Component
    {
        Sector? sector = gameObject.GetComponent<Sector>() ?? parentSector;

        foreach (T component in gameObject.GetComponents<T>())
        {
            yield return (sector, component);
        }

        foreach (Transform child in gameObject.transform)
        {
            foreach (var recursivePair in GetComponentsInChildrenWithSector<T>(child.gameObject, sector))
            {
                yield return recursivePair;
            }
        }
    }

    private static string GetTransformPath(Transform current)
    {
        return current.parent == null
            ? current.name
            : GetTransformPath(current.parent) + "/" + current.name;
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

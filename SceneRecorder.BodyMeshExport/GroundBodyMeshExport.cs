using Picalines.OuterWilds.SceneRecorder.Shared.Models;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.BodyMeshExport;

public static class GroundBodyMeshExport
{
    public static GroundBodyMeshInfo CaptureMeshInfo(GameObject groundBodyObject)
    {
        var renderedMeshFilters = groundBodyObject.GetComponentsInChildren<MeshFilter>()
            .Where(meshFilter => meshFilter.TryGetComponent<Renderer>(out _) is true);

        var plainMeshes = new List<MeshInfo>();
        var streamedMeshes = new List<MeshInfo>();

        foreach (var meshFilter in renderedMeshFilters)
        {
            var (meshGameObject, meshTransform) = (meshFilter.gameObject, meshFilter.transform);

            var transformData = TransformModel.FromGlobalTransform(meshTransform);
            var meshGameObjectPath = GetTransformPath(meshGameObject.transform);

            if (!(StreamingManager.s_tableLoaded
                && meshFilter.TryGetComponent<StreamingMeshHandle>(out var streamingHandle)
                && StreamingManager.s_streamingAssetBundleMap.TryGetValue(streamingHandle.assetBundle, out var assetBundle)
                && assetBundle is StreamingMeshAssetBundle { isLoaded: true } meshAssetBundle))
            {
                plainMeshes.Add(new(meshGameObjectPath, transformData));
            }
            else
            {
                streamedMeshes.Add(new(meshAssetBundle._meshNamesByID[streamingHandle.meshIndex], transformData));
            }
        }

        return new GroundBodyMeshInfo(
            groundBodyObject.name,
            TransformModel.FromGlobalTransform(groundBodyObject.transform),
            plainMeshes,
            streamedMeshes);
    }

    private static string GetTransformPath(Transform current)
    {
        return current.parent == null
            ? current.name
            : GetTransformPath(current.parent) + "/" + current.name;
    }
}

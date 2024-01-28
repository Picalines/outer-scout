using UnityEngine;
using UnityEngine.PostProcessing;

namespace SceneRecorder.Recording.Extensions;

internal static class OWCameraExtensions
{
    public static OWCamera CopyTo(
        this OWCamera sourceOWCamera,
        GameObject cameraParent,
        bool copyPostProcessing = true
    )
    {
        cameraParent.SetActive(false);

        cameraParent.AddComponent<Camera>(); // controlled by OWCamera

        var newOWCamera = cameraParent.AddComponent<OWCamera>();
        newOWCamera.renderSkybox = sourceOWCamera.renderSkybox;
        newOWCamera.useGUILayout = sourceOWCamera.useGUILayout;
        newOWCamera.useViewmodels = sourceOWCamera.useViewmodels;
        newOWCamera.farCameraDistance = sourceOWCamera.farCameraDistance;
        newOWCamera.useFarCamera = sourceOWCamera.useFarCamera;

        if (copyPostProcessing && sourceOWCamera.postProcessing != null)
        {
            cameraParent.AddComponent<PostProcessingBehaviour>().profile = sourceOWCamera
                .postProcessing
                .profile;
        }

        cameraParent.SetActive(true); // OWCamera.Awake

        // after awake in order to set properties on UnityEngine.Camera
        newOWCamera.mainCamera.usePhysicalProperties = sourceOWCamera
            .mainCamera
            .usePhysicalProperties;

        newOWCamera.backgroundColor = sourceOWCamera.backgroundColor;
        newOWCamera.clearFlags = sourceOWCamera.clearFlags;
        newOWCamera.cullingMask = sourceOWCamera.cullingMask;
        newOWCamera.farClipPlane = sourceOWCamera.farClipPlane;
        newOWCamera.nearClipPlane = sourceOWCamera.nearClipPlane;
        newOWCamera.aspect = sourceOWCamera.aspect;
        newOWCamera.fieldOfView = sourceOWCamera.fieldOfView;

        return newOWCamera;
    }
}

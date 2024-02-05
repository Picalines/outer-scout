using SceneRecorder.Domain;
using SceneRecorder.Infrastructure.Validation;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace SceneRecorder.Application.Extensions;

public static class OWCameraExtensions
{
    public static CameraPerspective GetPerspective(this OWCamera owCamera)
    {
        owCamera.Throw().If(owCamera.mainCamera.usePhysicalProperties is false);

        var camera = owCamera.mainCamera;

        return new()
        {
            SensorSize = camera.sensorSize,
            FocalLength = camera.focalLength,
            LensShift = camera.lensShift,
            NearClipPlane = camera.nearClipPlane,
            FarClipPlane = camera.farClipPlane,
        };
    }

    public static void ApplyPerspective(this OWCamera owCamera, CameraPerspective cameraInfo)
    {
        owCamera.Throw().If(owCamera.mainCamera.usePhysicalProperties is false);

        var camera = owCamera.mainCamera;

        camera.focalLength = cameraInfo.FocalLength;
        camera.sensorSize = cameraInfo.SensorSize;
        camera.lensShift = cameraInfo.LensShift;

        owCamera.nearClipPlane = cameraInfo.NearClipPlane;
        owCamera.farClipPlane = cameraInfo.FarClipPlane;
    }

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

        newOWCamera.mainCamera.gateFit = sourceOWCamera.mainCamera.gateFit;

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

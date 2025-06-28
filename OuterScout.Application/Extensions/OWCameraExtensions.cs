using OuterScout.Domain;
using OuterScout.Shared.Extensions;
using OuterScout.Shared.Validation;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace OuterScout.Application.Extensions;

public static class OWCameraExtensions
{
    public static CameraPerspective GetPerspective(this OWCamera owCamera)
    {
        owCamera.mainCamera.usePhysicalProperties.Throw().IfFalse();

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
        cameraParent.Throw().If(cameraParent.HasComponent<Camera>());

        cameraParent.SetActive(false);

        var newCamera = cameraParent.AddComponent<Camera>();

        var newOWCamera = cameraParent.AddComponent<OWCamera>();
        newOWCamera.renderSkybox = sourceOWCamera.renderSkybox;
        newOWCamera.useGUILayout = sourceOWCamera.useGUILayout;
        newOWCamera.useViewmodels = sourceOWCamera.useViewmodels;
        newOWCamera.farCameraDistance = sourceOWCamera.farCameraDistance;
        newOWCamera.useFarCamera = sourceOWCamera.useFarCamera;
        newOWCamera.cullingMask = sourceOWCamera.cullingMask;
        newOWCamera.mainCamera.layerCullDistances = sourceOWCamera.mainCamera.layerCullDistances;
        newOWCamera.mainCamera.layerCullSpherical = sourceOWCamera.mainCamera.layerCullSpherical;

        if (copyPostProcessing)
        {
            if (
                sourceOWCamera.GetComponentOrNull<FlashbackScreenGrabImageEffect>() is
                { _downsampleShader: var downSampleShader }
            )
            {
                cameraParent.AddComponent<FlashbackScreenGrabImageEffect>()._downsampleShader =
                    downSampleShader;
            }

            if (
                sourceOWCamera.GetComponentOrNull<PlanetaryFogImageEffect>() is
                { fogShader: var fogShader }
            )
            {
                cameraParent.AddComponent<PlanetaryFogImageEffect>().fogShader = fogShader;
            }

            if (
                sourceOWCamera.GetComponentOrNull<PostProcessingBehaviour>() is
                { profile: var postProfile }
            )
            {
                cameraParent.AddComponent<PostProcessingBehaviour>().profile = postProfile;
            }
        }

        var cameraParentTransform = cameraParent.transform;

        var position = cameraParentTransform.position;
        var rotation = cameraParentTransform.rotation;
        newCamera.CopyFrom(sourceOWCamera.mainCamera);
        cameraParentTransform.position = position;
        cameraParentTransform.rotation = rotation;

        cameraParent.SetActive(true);

        return newOWCamera;
    }
}

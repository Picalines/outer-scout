﻿using Picalines.OuterWilds.SceneRecorder.Models;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.RouteDefinitions;

internal sealed class FreeCameraRouteDefinition : IApiRouteDefinition
{
    public static FreeCameraRouteDefinition Instance { get; } = new();

    private FreeCameraRouteDefinition() { }

    public void MapRoutes(HttpServerBuilder serverBuilder, IApiRouteDefinition.IContext context)
    {
        serverBuilder.MapGet("free_camera/transform/local", request =>
        {
            return LocatorExtensions.IsInSolarSystemScene()
                ? ResponseFabric.Ok(TransformModel.FromLocalTransform(FindFreeCamera().transform))
                : ResponseFabric.ServiceUnavailable();
        });

        serverBuilder.MapPatch("free_camera/transform/local", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var transformModel = request.ParseContentJson<TransformModel>();
            transformModel.ApplyToLocalTransform(FindFreeCamera().transform);

            return ResponseFabric.Ok();
        });

        serverBuilder.MapGet("free_camera/info", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var freeCam = FindFreeCamera().GetComponent<OWCamera>();

            return ResponseFabric.Ok(new
            {
                fov = freeCam.fieldOfView,
                near_clip_plane = freeCam.nearClipPlane,
                far_clip_plane = freeCam.farClipPlane,
                resolution_x = freeCam.pixelWidth,
                resolution_y = freeCam.pixelHeight,
            });
        });
    }

    private static GameObject FindFreeCamera()
    {
        return GameObject.Find("FREECAM");
    }
}
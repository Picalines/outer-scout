using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.Models;
using Picalines.OuterWilds.SceneRecorder.Recording.Recorders;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi;

[RequireComponent(typeof(OutputRecorder))]
public sealed class WebApiServer : MonoBehaviour
{
    private OutputRecorder _OutputRecorder = null!;

    private HttpServer? _HttpServer = null;

    private void Start()
    {
        _OutputRecorder = GetComponent<OutputRecorder>();
    }

    public void Configure(IModConfig modConfig)
    {
        OnDestroy();

        var listenUrl = $"http://localhost:{modConfig.GetSettingsValue<int>("web_api_port")}/";
        var httpServerBuilder = new HttpServerBuilder(listenUrl);

        MapRoutes(httpServerBuilder);

        if (_HttpServer?.Listening is true)
        {
            _HttpServer.StopListening();
        }

        _HttpServer = httpServerBuilder.Build(gameObject);
        _HttpServer.StartListening();
    }

    private void OnDestroy()
    {
        _HttpServer?.StopListening();
        _HttpServer = null!;
    }

    private void MapRoutes(HttpServerBuilder serverBuilder)
    {
        serverBuilder.MapGet("", request =>
        {
            return ResponseFabric.Ok(new { Message = $"Welcome to {nameof(SceneRecorder)} API!" });
        });

        serverBuilder.MapGet("scene/settings", request =>
        {
            return _OutputRecorder switch
            {
                { SceneSettings: { } sceneSettings } => ResponseFabric.Ok(sceneSettings),
                _ => ResponseFabric.ServiceUnavailable(),
            };
        });

        serverBuilder.MapGet("player/transform", request =>
        {
            return LocatorExtensions.IsInSolarSystemScene()
                ? ResponseFabric.Ok(TransformModel.FromGlobalTransform(Locator.GetPlayerBody().transform))
                : ResponseFabric.ServiceUnavailable();
        });

        serverBuilder.MapGet("player/ground_body", request =>
        {
            if (LocatorExtensions.IsInSolarSystemScene() is false)
            {
                return ResponseFabric.ServiceUnavailable();
            }

            var groundBody = Locator.GetPlayerController().GetLastGroundBodySafe();

            return ResponseFabric.Ok(new
            {
                groundBody.name,
                transform = TransformModel.FromGlobalTransform(groundBody.transform),
            });
        });

        serverBuilder.MapGet("recorder/is_able_to_record", request =>
        {
            return ResponseFabric.Ok(_OutputRecorder.IsAbleToRecord);
        });

        serverBuilder.MapGet("recorder/frames_recorded", request =>
        {
            return _OutputRecorder switch
            {
                { IsAbleToRecord: false } => ResponseFabric.ServiceUnavailable(),
                { IsAbleToRecord: true } => ResponseFabric.Ok(_OutputRecorder.FramesRecorded),
            };
        });

        serverBuilder.MapGet("recorder/enabled", request =>
        {
            return ResponseFabric.Ok(_OutputRecorder.enabled);
        });

        serverBuilder.MapPut("recorder?{enabled:bool}", request =>
        {
            var shouldRecord = request.GetQueryParameter<bool>("enabled");

            if ((shouldRecord, _OutputRecorder.IsAbleToRecord) is (true, false))
            {
                return ResponseFabric.ServiceUnavailable();
            }

            if (shouldRecord == _OutputRecorder.IsRecording)
            {
                return ResponseFabric.NotModified();
            }

            _OutputRecorder.enabled = shouldRecord;

            return ResponseFabric.Ok();
        });
    }
}

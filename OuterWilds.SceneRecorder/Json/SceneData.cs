using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Recorders;
using System.Collections.Generic;
using UnityEngine;

using static Picalines.OuterWilds.SceneRecorder.Recorders.TransformRecorder;

namespace Picalines.OuterWilds.SceneRecorder.Json;

internal sealed class SceneData
{
    public sealed class GameObjectData
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("transform")]
        public TransformData Transform { get; private set; }

        public GameObjectData(string name, TransformData transform)
        {
            Name = name;
            Transform = transform;
        }
    }

    public sealed class CameraData
    {
        [JsonProperty("fov")]
        public float FieldOfView { get; private set; }

        [JsonProperty("near_clip_plane")]
        public float NearClipPlane { get; private set; }

        [JsonProperty("far_clip_plane")]
        public float FarClipPlane { get; private set; }

        [JsonProperty("initial_transform")]
        public TransformData InitialTransform { get; private set; }

        public CameraData(float fieldOfView, float nearClipPlane, float farClipPlane, TransformData initialTransform)
        {
            FieldOfView = fieldOfView;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            InitialTransform = initialTransform;
        }
    }

    [JsonProperty("recorded_frames")]
    public int RecordedFrames { get; private set; }

    [JsonProperty("recorder_settings")]
    public SceneRecorderSettings RecorderSettings { get; private set; }

    [JsonProperty("player")]
    public GameObjectData Player { get; private set; }

    [JsonProperty("ground_body")]
    public GameObjectData GroundBody { get; private set; }

    [JsonProperty("player_camera")]
    public CameraData PlayerCamera { get; private set; }

    [JsonProperty("free_camera")]
    public CameraData FreeCamera { get; private set; }

    [JsonProperty("depth_camera")]
    public CameraData DepthCamera { get; private set; }

    [JsonProperty("free_camera_transforms")]
    public IReadOnlyList<TransformData> FreeCameraTransforms { get; private set; }

    private SceneData(
        int frames,
        SceneRecorderSettings recorderSettings,
        GameObjectData player,
        GameObjectData body,
        CameraData playerCamera,
        CameraData backgroundCamera,
        CameraData depthCamera,
        IReadOnlyList<TransformData> freeCameraTransforms)
    {
        RecordedFrames = frames;
        RecorderSettings = recorderSettings;
        Player = player;
        GroundBody = body;
        PlayerCamera = playerCamera;
        FreeCamera = backgroundCamera;
        DepthCamera = depthCamera;
        FreeCameraTransforms = freeCameraTransforms;
    }

    public static SceneData Capture(SceneRecorderSettings recorderSettings, int recordedFrames, IReadOnlyList<TransformData> freeCameraTransforms)
    {
        var player = Locator.GetPlayerBody();
        var playerCamera = Locator.GetPlayerCamera();
        var freeCamera = GameObject.Find("FREECAM").GetComponent<OWCamera>();
        var depthCamera = freeCamera.gameObject.transform.Find(DepthRecorder.CameraGameObjectName).GetComponent<OWCamera>();

        var playerGroundBody = (player.GetComponent<PlayerCharacterController>().GetLastGroundBody()
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetOWRigidbody())
            .gameObject;

        var playerData = CaptureGameObjectData(player.gameObject);

        var bodyData = CaptureGameObjectData(playerGroundBody);

        var playerCameraData = CaptureCameraData(playerCamera);

        var freeCameraData = CaptureCameraData(freeCamera);

        var depthCameraData = CaptureCameraData(depthCamera);

        return new SceneData(recordedFrames, recorderSettings, playerData, bodyData, playerCameraData, freeCameraData, depthCameraData, freeCameraTransforms);
    }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            Converters = { new TransformDataConverter() },
        });
    }

    private static GameObjectData CaptureGameObjectData(GameObject gameObject)
    {
        return new GameObjectData(
            name: gameObject.name,
            transform: CaptureTransformData(gameObject.transform));
    }

    private static CameraData CaptureCameraData(OWCamera owCamera)
    {
        return new CameraData(
            fieldOfView: owCamera.fieldOfView,
            nearClipPlane: owCamera.nearClipPlane,
            farClipPlane: owCamera.farClipPlane,
            initialTransform: CaptureTransformData(owCamera.transform));
    }

    private static TransformData CaptureTransformData(Transform transform)
    {
        return new TransformData(transform.position, transform.rotation, transform.localScale);
    }
}

using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Recorders;
using Picalines.OuterWilds.SceneRecorder.Utils;
using Picalines.OuterWildsSceneRecorder;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder;

internal sealed class SceneData
{
    public sealed class TransformData
    {
        [JsonProperty("position"), JsonConverter(typeof(Vector3JsonConverter))]
        public Vector3 Position { get; private set; }

        [JsonProperty("rotation"), JsonConverter(typeof(QuaternionJsonConverter))]
        public Quaternion Rotation { get; private set; }

        [JsonProperty("scale"), JsonConverter(typeof(Vector3JsonConverter))]
        public Vector3 Scale { get; private set; }

        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }
    }

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

        [JsonProperty("transform")]
        public TransformData Transform { get; private set; }

        public CameraData(float fieldOfView, float nearClipPlane, float farClipPlane, TransformData transform)
        {
            FieldOfView = fieldOfView;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            Transform = transform;
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

    private SceneData(
        int frames,
        SceneRecorderSettings recorderSettings,
        GameObjectData player,
        GameObjectData body,
        CameraData playerCamera,
        CameraData backgroundCamera,
        CameraData depthCamera)
    {
        RecordedFrames = frames;
        RecorderSettings = recorderSettings;
        Player = player;
        GroundBody = body;
        PlayerCamera = playerCamera;
        FreeCamera = backgroundCamera;
        DepthCamera = depthCamera;
    }

    public static SceneData Capture(int recordedFrames, SceneRecorderSettings recorderSettings)
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

        return new SceneData(recordedFrames, recorderSettings, playerData, bodyData, playerCameraData, freeCameraData, depthCameraData);
    }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
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
            transform: CaptureTransformData(owCamera.transform));
    }

    private static TransformData CaptureTransformData(Transform transform)
    {
        return new TransformData(transform.position, transform.rotation, transform.localScale);
    }
}

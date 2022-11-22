using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Recorders;
using Picalines.OuterWilds.SceneRecorder.Utils;
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

        public TransformData(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public TransformData(Transform transform, Vector3 localPositionOffset = default)
            : this(transform.TransformPoint(localPositionOffset), transform.rotation)
        {
        }
    }

    public sealed class BodyData
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("transform")]
        public TransformData Transform { get; private set; }

        public BodyData(string name, TransformData transform)
        {
            Name = name;
            Transform = transform;
        }
    }

    public sealed class PlayerData
    {
        [JsonProperty("transform")]
        public TransformData Transform { get; private set; }

        public PlayerData(TransformData transform)
        {
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

    [JsonProperty("frames")]
    public int Frames { get; private set; }

    [JsonProperty("body")]
    public BodyData Body { get; private set; }

    [JsonProperty("player")]
    public PlayerData Player { get; private set; }

    [JsonProperty("player_camera")]
    public CameraData PlayerCamera { get; private set; }

    [JsonProperty("background_camera")]
    public CameraData BackgroundCamera { get; private set; }

    [JsonProperty("depth_camera")]
    public CameraData DepthCamera { get; private set; }

    private SceneData(
        int frames,
        BodyData body,
        PlayerData player,
        CameraData playerCamera,
        CameraData backgroundCamera,
        CameraData depthCamera)
    {
        Frames = frames;
        Body = body;
        Player = player;
        PlayerCamera = playerCamera;
        BackgroundCamera = backgroundCamera;
        DepthCamera = depthCamera;
    }

    public static SceneData Capture(int frames)
    {
        var player = Locator.GetPlayerBody();
        var playerCamera = Locator.GetPlayerCamera();
        var freeCamera = GameObject.Find("FREECAM").GetComponent<OWCamera>();
        var depthCamera = freeCamera.gameObject.transform.Find(DepthRecorder.CameraGameObjectName).GetComponent<OWCamera>();

        var playerGroundBody = (player.GetComponent<PlayerCharacterController>().GetLastGroundBody()
            ?? Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetOWRigidbody())
            .gameObject;

        var bodyData = new BodyData(
            name: playerGroundBody.name,
            transform: new(playerGroundBody.transform));

        var playerData = new PlayerData(
            transform: new(player.transform, localPositionOffset: Vector3.down));

        var playerCameraData = CaptureCameraData(playerCamera);

        var freeCameraData = CaptureCameraData(freeCamera);

        var depthCameraData = CaptureCameraData(depthCamera);

        return new SceneData(frames, bodyData, playerData, playerCameraData, freeCameraData, depthCameraData);

        static CameraData CaptureCameraData(OWCamera owCamera)
        {
            return new CameraData(
                fieldOfView: owCamera.fieldOfView,
                nearClipPlane: owCamera.nearClipPlane,
                farClipPlane: owCamera.farClipPlane,
                transform: new(owCamera.transform));
        }
    }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }
}

using Newtonsoft.Json;
using Picalines.OuterWilds.SceneRecorder.Recorders;
using Picalines.OuterWilds.SceneRecorder.Utils;
using Picalines.OuterWildsSceneRecorder;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public TransformData(Transform transform, Vector3 localPositionOffset = default)
            : this(transform.TransformPoint(localPositionOffset), transform.rotation, transform.localScale)
        {
        }
    }

    public class GameObjectData
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

    public sealed class SectorObjectData : GameObjectData
    {
        [JsonProperty("mesh_asset_path")]
        public string MeshAssetPath { get; private set; }

        public SectorObjectData(string name, TransformData transform, string meshAssetPath) : base(name, transform)
        {
            MeshAssetPath = meshAssetPath;
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

    [JsonProperty("recorded_frames")]
    public int RecordedFrames { get; private set; }

    [JsonProperty("recorder_settings")]
    public SceneRecorderSettings RecorderSettings { get; private set; }

    [JsonProperty("player")]
    public PlayerData Player { get; private set; }

    [JsonProperty("ground_body")]
    public GameObjectData GroundBody { get; private set; }

    [JsonProperty("sector_objects")]
    public IReadOnlyList<SectorObjectData> SectorObjects { get; private set; }

    [JsonProperty("player_camera")]
    public CameraData PlayerCamera { get; private set; }

    [JsonProperty("background_camera")]
    public CameraData BackgroundCamera { get; private set; }

    [JsonProperty("depth_camera")]
    public CameraData DepthCamera { get; private set; }

    private SceneData(
        int frames,
        SceneRecorderSettings recorderSettings,
        PlayerData player,
        GameObjectData body,
        IReadOnlyList<SectorObjectData> sectorObjects,
        CameraData playerCamera,
        CameraData backgroundCamera,
        CameraData depthCamera)
    {
        RecordedFrames = frames;
        RecorderSettings = recorderSettings;
        Player = player;
        GroundBody = body;
        SectorObjects = sectorObjects;
        PlayerCamera = playerCamera;
        BackgroundCamera = backgroundCamera;
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

        var playerData = new PlayerData(
            transform: new(player.transform, localPositionOffset: Vector3.down));

        var bodyData = CaptureGameObjectData(playerGroundBody);

        var sectorObjects = CaptureSectorObjects(Locator.GetPlayerSectorDetector());

        var playerCameraData = CaptureCameraData(playerCamera);

        var freeCameraData = CaptureCameraData(freeCamera);

        var depthCameraData = CaptureCameraData(depthCamera);

        return new SceneData(recordedFrames, recorderSettings, playerData, bodyData, sectorObjects, playerCameraData, freeCameraData, depthCameraData);
    }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    private static SectorObjectData[] CaptureSectorObjects(PlayerSectorDetector playerSectorDetector)
    {
        if (StreamingManager.s_tableLoaded is false)
        {
            return Array.Empty<SectorObjectData>();
        }

        return playerSectorDetector._sectorList switch
        {
            { Count: 0 } => Array.Empty<SectorObjectData>(),
            { Count: 1 } singleSector => CaptureRenderersOf(singleSector[0].gameObject).ToArray(),
            { } sectors => CaptureRenderersOf(sectors[1].gameObject).ToArray(),
        };

        static IEnumerable<SectorObjectData> CaptureRenderersOf(GameObject sector)
        {
            return sector.GetComponentsInChildren<StreamingMeshHandle>()
                .Select(handle => new SectorObjectData(
                    handle.gameObject.name,
                    new(handle.gameObject.transform),
                    MeshAssetPath(StreamingManager.s_streamingAssetBundleTable, handle)));
        }

        static string MeshAssetPath(StreamingAssetBundleTable streamingAssetBundleTable, StreamingMeshHandle handle)
        {
            return streamingAssetBundleTable.assetBundles
                .First(bundle => bundle.assetBundleName == handle.assetBundle)
                .meshNamesByID[handle.meshIndex];
        }
    }

    private static GameObjectData CaptureGameObjectData(GameObject gameObject)
    {
        return new GameObjectData(
            name: gameObject.name,
            transform: new(gameObject.transform));
    }

    private static CameraData CaptureCameraData(OWCamera owCamera)
    {
        return new CameraData(
            fieldOfView: owCamera.fieldOfView,
            nearClipPlane: owCamera.nearClipPlane,
            farClipPlane: owCamera.farClipPlane,
            transform: new(owCamera.transform));
    }
}

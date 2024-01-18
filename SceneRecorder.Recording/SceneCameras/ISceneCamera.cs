using SceneRecorder.Recording.Recorders;
using SceneRecorder.Shared.DTOs;
using UnityEngine;

namespace SceneRecorder.Recording.SceneCameras;

public interface ISceneCamera
{
    public string Id { get; }

    public Transform Transform { get; }

    public CameraInfoDTO CameraInfo { get; set; }

    public IRecorder CreateColorRecorder();

    public IRecorder CreateDepthRecorder();
}

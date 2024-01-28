using UnityEngine;

namespace SceneRecorder.Application.SceneCameras;

public interface ISceneCamera
{
    public string Id { get; }

    public Transform Transform { get; }

    public RenderTexture? ColorTexture { get; }

    public RenderTexture? DepthTexture { get; }
}

using UnityEngine;

namespace SceneRecorder.Application.SceneCameras;

public interface ISceneCamera : IDisposable
{
    public Transform Transform { get; }

    public RenderTexture? ColorTexture { get; }

    public RenderTexture? DepthTexture { get; }
}

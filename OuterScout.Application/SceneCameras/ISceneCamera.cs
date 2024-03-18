using UnityEngine;

namespace OuterScout.Application.SceneCameras;

public interface ISceneCamera : IDisposable
{
    public RenderTexture? ColorTexture { get; }

    public RenderTexture? DepthTexture { get; }
}

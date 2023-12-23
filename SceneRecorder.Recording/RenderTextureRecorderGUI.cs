using SceneRecorder.Recording.Recorders.Abstract;
using UnityEngine;

namespace SceneRecorder.Recording;

[RequireComponent(typeof(RenderTextureRecorder))]
internal sealed class RenderTextureRecorderGUI : MonoBehaviour
{
    private RenderTextureRecorder _TextureRecorder = null!;

    private void Awake()
    {
        _TextureRecorder = GetComponent<RenderTextureRecorder>();
    }

    private void OnGUI()
    {
        if (_TextureRecorder is not { IsRecording: true, SourceRenderTexture: { } texture })
        {
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
    }
}

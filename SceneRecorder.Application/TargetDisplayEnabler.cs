using UnityEngine;

namespace SceneRecorder.Application;

[RequireComponent(typeof(Camera))]
internal sealed class TargetDisplayEnabler : MonoBehaviour
{
    private const int InvalidDisplay = -1;

    private Camera _Camera = null!;

    private int _TargetDisplay = InvalidDisplay;

    private void Start()
    {
        _Camera = GetComponent<Camera>();

        SaveDisplay();
    }

    public void SaveDisplay()
    {
        if (_Camera.targetDisplay is not InvalidDisplay and var targetDisplay)
        {
            _TargetDisplay = targetDisplay;
        }
    }

    public bool RenderingEnabled
    {
        get => _Camera.targetDisplay is not InvalidDisplay;
        set => _Camera.targetDisplay = value ? _TargetDisplay : InvalidDisplay;
    }
}

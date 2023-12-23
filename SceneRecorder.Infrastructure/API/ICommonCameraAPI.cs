using UnityEngine;
using UnityEngine.Events;

namespace SceneRecorder.Infrastructure.API;

public interface ICommonCameraAPI
{
    void RegisterCustomCamera(OWCamera OWCamera);

    (OWCamera, Camera) CreateCustomCamera(string name);

    (OWCamera, Camera) CreateCustomCamera(string name, Action<OWCamera> postInitMethod);

    void ExitCamera(OWCamera OWCamera);

    void EnterCamera(OWCamera OWCamera);

    UnityEvent<PlayerTool> EquipTool();
    UnityEvent<PlayerTool> UnequipTool();
}

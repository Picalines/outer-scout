namespace SceneRecorder.Application.Recording;

public interface IRecorder : IDisposable
{
    public void Capture();
}

namespace SceneRecorder.Recording.Recorders;

public interface IRecorder : IDisposable
{
    public void Capture();
}

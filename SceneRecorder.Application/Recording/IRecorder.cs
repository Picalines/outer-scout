namespace SceneRecorder.Application.Recording;

public interface IRecorder : IDisposable
{
    public void Capture();

    public interface IBuilder
    {
        public IRecorder StartRecording();
    }
}

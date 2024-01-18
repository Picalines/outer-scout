namespace SceneRecorder.Recording.Recorders;

public interface IRecorder
{
    public void StartRecording();

    public void RecordData();

    public void StopRecording();
}

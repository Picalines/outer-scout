using System.Web;
using SceneRecorder.Shared.Extensions;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders;

public sealed class TransformRecorder : IRecorder
{
    public required Transform Transform { get; init; }

    public required Transform Parent { get; init; }

    public required string TargetFile { get; init; }

    private StreamWriter _streamWriter = null!;

    private bool _prependComma = false;

    public void StartRecording()
    {
        _streamWriter = new StreamWriter(TargetFile);

        var jsonParentName = HttpUtility.JavaScriptStringEncode(Parent.name, true);

        _streamWriter.Write(
            $$"""
            {
                "parent": {{jsonParentName}},
                "transforms": [
            """
        );

        _prependComma = false;
    }

    public void RecordData()
    {
        var (px, py, pz) = Parent.InverseTransformPoint(Transform.position);
        var (rx, ry, rz, rw) = Parent.InverseTransformRotation(Transform.rotation);
        var (sx, sy, sz) = Transform.localScale;

        ReadOnlySpan<float> array = [px, py, pz, rx, ry, rz, rw, sx, sy, sz];

        if (_prependComma)
        {
            _streamWriter.Write(',');
        }

        _streamWriter.Write('[');
        _streamWriter.Write(array[0]);

        for (int i = 1; i < array.Length; i++)
        {
            _streamWriter.Write(',');
            _streamWriter.Write(array[i]);
        }

        _streamWriter.Write(']');
        _prependComma = true;
    }

    public void StopRecording()
    {
        _streamWriter.WriteLine(']');
        _streamWriter.Write('}');

        _streamWriter.Dispose();
        _streamWriter = null!;
    }
}

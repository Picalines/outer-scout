using SceneRecorder.Shared.Extensions;
using UnityEngine;

namespace SceneRecorder.Recording.Recorders;

public sealed class TransformRecorder : IRecorder
{
    private const char CsvColumnSeparator = ';';

    public required Transform Transform { get; init; }

    public required Transform Parent { get; init; }

    public required string TargetFile { get; init; }

    private StreamWriter _streamWriter = null!;

    public void StartRecording()
    {
        _streamWriter = new StreamWriter(TargetFile);

        _streamWriter.WriteLine($"# parent={Parent.name}");

        // header
        WriteCsvRow<string>(["px", "py", "pz", "rx", "ry", "rz", "rw", "sx", "sy", "sz"]);
    }

    public void RecordData()
    {
        var (px, py, pz) = Parent.InverseTransformPoint(Transform.position);
        var (rx, ry, rz, rw) = Parent.InverseTransformRotation(Transform.rotation);
        var (sx, sy, sz) = Transform.localScale;

        _streamWriter.WriteLine();
        WriteCsvRow<float>([px, py, pz, rx, ry, rz, rw, sx, sy, sz]);
    }

    public void StopRecording()
    {
        _streamWriter.Dispose();
        _streamWriter = null!;
    }

    private void WriteCsvRow<T>(ReadOnlySpan<T> row)
    {
        if (row.Length is 0)
        {
            return;
        }

        _streamWriter.Write(row[0]);

        for (int i = 1; i < row.Length; i++)
        {
            _streamWriter.Write(CsvColumnSeparator);
            _streamWriter.Write(row[i]);
        }
    }
}

using UnityEngine;

namespace SceneRecorder.Shared.Models.JsonConverters;

public sealed class Vector2Converter : VectorJsonConverter<Vector2>
{
    protected override int NumberOfAxes => 2;

    protected override double GetAxis(in Vector2 vector, int axisIndex)
    {
        return vector[axisIndex];
    }

    protected override void SetAxis(ref Vector2 vector, int axisIndex, double axisValue)
    {
        vector[axisIndex] = (float)axisValue;
    }
}

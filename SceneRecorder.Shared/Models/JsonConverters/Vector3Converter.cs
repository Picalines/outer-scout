using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public sealed class Vector3Converter : VectorJsonConverter<Vector3>
{
    protected override int NumberOfAxes => 3;

    protected override double GetAxis(in Vector3 vector, int axisIndex)
    {
        return vector[axisIndex];
    }

    protected override void SetAxis(ref Vector3 vector, int axisIndex, double axisValue)
    {
        vector[axisIndex] = (float)axisValue;
    }
}

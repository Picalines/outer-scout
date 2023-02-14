using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.Shared.Models.JsonConverters;

public sealed class QuaternionConverter : VectorJsonConverter<Quaternion>
{
    protected override int NumberOfAxes => 4;

    protected override double GetAxis(in Quaternion quaternion, int axisIndex)
    {
        return quaternion[axisIndex];
    }

    protected override void SetAxis(ref Quaternion quaternion, int axisIndex, double axisValue)
    {
        quaternion[axisIndex] = (float)axisValue;
    }
}

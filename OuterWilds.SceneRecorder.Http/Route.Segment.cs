namespace Picalines.OuterWilds.SceneRecorder.Http;

internal sealed partial record Route
{
    public abstract record Segment;

    private sealed record PlainSegment(string Value) : Segment;

    private enum ParameterSegmentType
    {
        Path,
        Query,
    }

    private abstract record ParameterSegment(string ParameterName, ParameterSegmentType Type) : Segment
    {
        public abstract bool TryParseValue(string parameterUrlValue, out object? parameterValue);
    }

    private sealed record BoolParameterSegment(string ParameterName, ParameterSegmentType Type) : ParameterSegment(ParameterName, Type)
    {
        public override bool TryParseValue(string parameterUrlValue, out object? parameterValue)
        {
            parameterValue = parameterUrlValue switch
            {
                "true" => true,
                "false" => false,
                _ => null,
            };
            return parameterValue is not null;
        }
    }

    private sealed record IntParameterSegment(string ParameterName, ParameterSegmentType Type) : ParameterSegment(ParameterName, Type)
    {
        public override bool TryParseValue(string parameterUrlValue, out object? parameterValue)
        {
            if (int.TryParse(parameterUrlValue, out int intParameterValue))
            {
                parameterValue = intParameterValue;
                return true;
            }

            parameterValue = null;
            return false;
        }
    }

    private sealed record FloatParameterSegment(string ParameterName, ParameterSegmentType Type) : ParameterSegment(ParameterName, Type)
    {
        public override bool TryParseValue(string parameterUrlValue, out object? parameterValue)
        {
            if (float.TryParse(parameterUrlValue, out float floatParameterValue))
            {
                parameterValue = floatParameterValue;
                return true;
            }

            parameterValue = null;
            return false;
        }
    }

    private sealed record StringParameterSegment(string ParameterName, ParameterSegmentType Type) : ParameterSegment(ParameterName, Type)
    {
        public override bool TryParseValue(string parameterUrlValue, out object? parameterValue)
        {
            parameterValue = parameterUrlValue;
            return true;
        }
    }
}

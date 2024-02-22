using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;

namespace SceneRecorder.WebApi.Http.Routing;

internal sealed partial class Route
{
    public sealed class Builder
    {
        private HttpMethod _httpMethod = HttpMethod.Get;

        private readonly List<Segment> _segments = [];

        private readonly Dictionary<string, int> _parameterIndexes = [];

        public Route Build()
        {
            IEnumerable<Segment> segments =
                _segments.Count > 0 ? _segments : [new Segment(SegmentType.Constant, "")];

            return new Route(_httpMethod, segments.ToArray(), _parameterIndexes.ToDictionary());
        }

        public Builder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public Builder WithConstantSegment(string value)
        {
            _segments.Add(new Segment(SegmentType.Constant, value));
            return this;
        }

        public Builder WithParameterSegment(string parameterName)
        {
            parameterName.Throw().If(_parameterIndexes.ContainsKey(parameterName));

            _parameterIndexes[parameterName] = _segments.Count;
            _segments.Add(new Segment(SegmentType.Parameter, parameterName));
            return this;
        }
    }
}

using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Http.Routing;

namespace OuterScout.WebApi.Http;

public sealed partial class HttpServer
{
    private sealed class UrlParameterBinder : IParameterBinder
    {
        private readonly Route _route;

        private readonly Request _request;

        public UrlParameterBinder(Route route, Request request)
        {
            _route = route;
            _request = request;
        }

        public bool CanBind(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<FromUrlAttribute>(false) is not null;
        }

        public object? Bind(ParameterInfo parameter)
        {
            var typeConverter = TypeDescriptor.GetConverter(parameter.ParameterType);

            if (_route.ParameterIndexes.TryGetValue(parameter.Name, out var pathIndex))
            {
                return typeConverter.ConvertFromString(_request.Path[pathIndex]);
            }

            if (_request.QueryParameters.TryGetValue(parameter.Name, out var queryValue) is false)
            {
                throw new ResponseException(
                    ResponseFabric.BadRequest(
                        $"missing query parameter '{parameter.Name}' ({parameter.ParameterType.Name})"
                    )
                );
            }

            return typeConverter.ConvertFromString(queryValue);
        }
    }

    private sealed class RequestBodyBinder : IParameterBinder
    {
        private readonly Request _request;

        private readonly JsonSerializer _jsonSerializer;

        public RequestBodyBinder(Request request, JsonSerializer jsonSerializer)
        {
            _request = request;
            _jsonSerializer = jsonSerializer;
        }

        public bool CanBind(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<FromBodyAttribute>() is not null;
        }

        public object? Bind(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(string))
            {
                return _request.BodyReader.ReadToEnd();
            }

            using var jsonReader = new JsonTextReader(_request.BodyReader);
            return _jsonSerializer.Deserialize(jsonReader, parameter.ParameterType);
        }
    }
}

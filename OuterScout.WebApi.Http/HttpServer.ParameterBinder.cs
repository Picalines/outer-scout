using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using OuterScout.Infrastructure.DependencyInjection;
using OuterScout.Infrastructure.Extensions;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Http.Routing;

namespace OuterScout.WebApi.Http;

public sealed partial class HttpServer
{
    private sealed class UrlParameterBinder : IParameterBinder
    {
        public required Route Route { private get; init; }

        public required Request Request { private get; init; }

        public bool CanBind(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<FromUrlAttribute>(false) is not null;
        }

        public object? Bind(ParameterInfo parameter)
        {
            var typeConverter = TypeDescriptor.GetConverter(parameter.ParameterType);

            if (Route.ParameterIndexes.TryGetValue(parameter.Name, out var pathIndex))
            {
                return typeConverter.ConvertFromString(Request.Path[pathIndex]);
            }

            if (Request.QueryParameters.TryGetValue(parameter.Name, out var queryValue))
            {
                return typeConverter.ConvertFromString(queryValue);
            }

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            throw new ResponseException(
                ResponseFabric.BadRequest(
                    $"missing query parameter '{parameter.Name}' ({parameter.ParameterType.Name})"
                )
            );
        }
    }

    private sealed class RequestBodyBinder : IParameterBinder
    {
        public required Request Request { private get; init; }

        public required JsonSerializer JsonSerializer { private get; init; }

        public bool CanBind(ParameterInfo parameter)
        {
            return parameter.GetCustomAttribute<FromBodyAttribute>() is not null;
        }

        public object? Bind(ParameterInfo parameter)
        {
            if (parameter.ParameterType == typeof(string))
            {
                return Request.BodyReader.ReadToEnd();
            }

            return JsonSerializer.Deserialize(Request.BodyReader, parameter.ParameterType) switch
            {
                { } parsedBody => parsedBody,
                null when parameter.HasDefaultValue => parameter.DefaultValue,
                null when parameter.IsNullable() => null,
                _
                    => throw new ResponseException(
                        ResponseFabric.BadRequest($"invalid json request body")
                    )
            };
        }
    }
}

﻿using Newtonsoft.Json;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public string Url { get; }

    public string Concent { get; }

    private readonly Dictionary<string, object?> _RouteParameters = new();

    private readonly Dictionary<string, object?> _QueryParameters = new();

    internal Request(HttpMethod httpMethod, string url, string concent)
    {
        HttpMethod = httpMethod;
        Url = url;
        Concent = concent;
    }

    internal void AddRouteParameter(string name, object? value)
    {
        _RouteParameters[name] = value;
    }

    internal void AddQueryParameter(string name, object? value)
    {
        _QueryParameters[name] = value;
    }

    public T GetRouteParameter<T>(string name)
    {
        if (_RouteParameters.TryGetValue(name, out var parameterBoxValue) is false
            || parameterBoxValue is not T parameterValue)
        {
            throw new InvalidOperationException();
        }

        return parameterValue;
    }

    public T GetQueryParameter<T>(string name)
    {
        if (_QueryParameters.TryGetValue(name, out var parameterBoxValue) is false
            || parameterBoxValue is not T parameterValue)
        {
            throw new InvalidOperationException();
        }

        return parameterValue;
    }

    public T ParseContentJson<T>()
    {
        return JsonConvert.DeserializeObject<T>(Concent)!;
    }
}
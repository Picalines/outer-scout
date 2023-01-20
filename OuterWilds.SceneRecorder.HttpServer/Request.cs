﻿namespace OuterWilds.SceneRecorder.HttpServer;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public string Url { get; }

    private readonly Dictionary<string, object?> _RouteParameters = new();

    private readonly Dictionary<string, object?> _QueryParameters = new();

    internal Request(HttpMethod httpMethod, string url)
    {
        HttpMethod = httpMethod;
        Url = url;
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
}

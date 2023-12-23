﻿using System.Web;
using Newtonsoft.Json;

namespace SceneRecorder.WebApi.Http;

public sealed class Request
{
    public HttpMethod HttpMethod { get; }

    public Uri Uri { get; }

    public string Body { get; }

    internal Dictionary<string, string> MutableRouteParameters { get; } = new();

    internal Request(HttpMethod httpMethod, Uri uri, string body)
    {
        HttpMethod = httpMethod;
        Uri = uri;
        Body = body;

        var queryDictionary = new Dictionary<string, string>();

        var query = HttpUtility.ParseQueryString(uri.Query);
        var queryPairs = query.AllKeys.SelectMany(
            query.GetValues,
            (key, value) => new { key, value }
        );

        foreach (var pair in queryPairs)
        {
            queryDictionary[pair.key] = pair.value;
        }

        QueryParameters = queryDictionary;
    }

    public IReadOnlyDictionary<string, string> RouteParameters => MutableRouteParameters;

    public IReadOnlyDictionary<string, string> QueryParameters { get; }

    public T JsonBody<T>()
    {
        return JsonConvert.DeserializeObject<T>(Body)!;
    }
}

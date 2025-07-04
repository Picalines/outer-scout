﻿using System.Net;
using Newtonsoft.Json;
using OuterScout.Shared.DependencyInjection;
using OuterScout.Shared.Extensions;
using OuterScout.Shared.Validation;
using OuterScout.WebApi.Http.Components;
using OuterScout.WebApi.Http.Response;
using OuterScout.WebApi.Http.Routing;
using OWML.Common;

namespace OuterScout.WebApi.Http;

using RequestHandler = Func<IServiceContainer, IResponse>;

public sealed partial class HttpServer : IDisposable
{
    private const string RequestScope = "request";

    private readonly string _baseUrl;

    private readonly ServiceContainer _services;
    private readonly Router<RequestHandler> _router;
    private readonly HttpListener _httpListener;
    private readonly IModConsole? _logger;

    private CancellationTokenSource? _cancellationTokenSource = null;
    private TaskCompletionSource<object?>? _stoppedListening = null;

    private Route? _currentRoute = null;
    private Request? _currentRequest = null;

    private bool _disposed = false;

    private HttpServer(string baseUrl, ServiceContainer services, Router<RequestHandler> router)
    {
        baseUrl.Throw().If(baseUrl.EndsWith("/") is false);

        _baseUrl = baseUrl;
        _services = services;
        _router = router;
        _logger = services.ResolveOrNull<IModConsole>();

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(_baseUrl);

        Log($"started listening at {_httpListener.Prefixes.Single()}", MessageType.Info);

        Task.Run(ListenAsync);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _cancellationTokenSource?.Cancel();
        _stoppedListening?.Task.Wait();

        Log("stopped listening", MessageType.Info);

        _services.Dispose();
    }

    private async Task ListenAsync()
    {
        if (_disposed)
        {
            return;
        }

        _stoppedListening = new();

        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        _httpListener.Start();

        var unityThreadExecutor = UnityThreadExecutor.Create();

        while (_disposed is false)
        {
            HttpListenerContext context;
            try
            {
                context = await _httpListener.GetContextAsync().AsCancellable(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            var uri = context.Request.Url;
            var httpMethod = new HttpMethod(context.Request.HttpMethod);

            var bodyStream = new MemoryStream();
            context.Request.InputStream.CopyTo(bodyStream);
            context.Request.InputStream.Close();
            bodyStream.Position = 0;

            var bodyReader = new StreamReader(bodyStream, context.Request.ContentEncoding);

            bodyReader.BaseStream.Position = 0;

            var request = new Request.Builder()
                .WithHttpMethod(httpMethod)
                .WithBodyReader(bodyReader)
                .WithPathAndQuery(uri)
                .Build();

            if (_router.Match(request) is (var route, var requestHandler))
            {
                unityThreadExecutor.EnqueueTask(() =>
                    HandleRequest(context, route, request, requestHandler)
                );
            }
            else
            {
                bodyReader.Dispose();

                SendSyncResponse(context, ResponseFabric.NotFound());

                var unmatchedPathAndQuery = context.Request.Url.PathAndQuery;

                unityThreadExecutor.EnqueueTask(() =>
                    Log($"route not found: {unmatchedPathAndQuery}", MessageType.Warning)
                );
            }
        }

        UnityEngine.Object.Destroy(unityThreadExecutor);

        _httpListener.Stop();
        _stoppedListening.SetResult(null);

        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = null;
    }

    private void HandleRequest(
        HttpListenerContext context,
        Route route,
        Request request,
        RequestHandler handler
    )
    {
        Log($"handling {route}", MessageType.Info);

        IResponse response;

        _currentRoute = route;
        _currentRequest = request;

        using (request.BodyReader)
        using (var scope = _services.StartScope(RequestScope))
        {
            response = handler(scope);
        }

        SendSyncResponse(context, response);
    }

    private void SendSyncResponse(HttpListenerContext context, IResponse response)
    {
        SetGenericHeaders(context, response);

        using var httpResponse = context.Response;

        if (response is EmptyResponse)
        {
            httpResponse.ContentLength64 = 0;
        }
        else if (response is StringResponse { Content: var content })
        {
            using var bodyWriter = new StreamWriter(httpResponse.OutputStream);
            bodyWriter.Write(content);
        }
        else if (response is StreamResponse { Stream: var stream })
        {
            stream.CopyTo(httpResponse.OutputStream);
            stream.Close();
            httpResponse.Close();
        }
        else if (response is JsonResponse { Value: var value })
        {
            using var bodyWriter = new StreamWriter(httpResponse.OutputStream);
            var jsonSerializer = _services.Resolve<JsonSerializer>();
            jsonSerializer.Serialize(bodyWriter, value);
        }
        else
        {
            throw new NotImplementedException(
                $"{response.GetType()} is not supported by {nameof(HttpServer)}"
            );
        }
    }

    private static void SetGenericHeaders(HttpListenerContext context, IResponse response)
    {
        var httpResponse = context.Response;

        httpResponse.StatusCode = (int)response.StatusCode;
        httpResponse.ContentType = response.ContentType;
        if (response.ContentType.Contains("charset") is false)
        {
            httpResponse.ContentType += "; charset=utf-8";
        }
    }

    private void Log(string message, MessageType messageType)
    {
        _logger?.WriteLine($"{nameof(OuterScout)} API: {message}", messageType);
    }
}

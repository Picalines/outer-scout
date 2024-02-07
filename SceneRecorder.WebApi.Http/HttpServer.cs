﻿using System.Collections;
using System.Net;
using OWML.Common;
using SceneRecorder.Infrastructure.Components;
using SceneRecorder.Infrastructure.DependencyInjection;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.Infrastructure.Validation;
using SceneRecorder.WebApi.Http.Components;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;

namespace SceneRecorder.WebApi.Http;

public sealed partial class HttpServer : IDisposable
{
    private readonly string _baseUrl;
    private readonly ServiceContainer _services;
    private readonly Router _router;
    private readonly HttpListener _httpListener;

    private CancellationTokenSource? _cancellationTokenSource = null;
    private TaskCompletionSource<object?>? _stoppedListening;

    private bool _disposed = false;

    private HttpServer(string baseUrl, ServiceContainer services, Router router)
    {
        baseUrl.Throw().If(baseUrl.EndsWith("/") is false);

        _baseUrl = baseUrl;
        _services = services;
        _router = router;

        _httpListener = new();
        _httpListener.Prefixes.Add(_baseUrl);

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

        Log($"stopped listening", MessageType.Info);
    }

    private async Task ListenAsync()
    {
        if (_disposed)
        {
            return;
        }

        Log($"started listening at {_httpListener.Prefixes.Single()}", MessageType.Info);

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

            Log($"received {httpMethod} request at '{uri}'", MessageType.Info);

            var bodyReader = new StreamReader(
                context.Request.InputStream,
                context.Request.ContentEncoding
            );

            var request = new Request.Builder()
                .WithHttpMethod(httpMethod)
                .WithBodyReader(bodyReader)
                .WithPathAndQuery(uri)
                .Build();

            if (_router.Match(request) is (var route, var requestHandler))
            {
                unityThreadExecutor.EnqueueTask(
                    () => HandleRequest(context, route, request, requestHandler)
                );

                Log($"route '{route}' is queued for handling", MessageType.Info);
            }
            else
            {
                bodyReader.Dispose();

                ((SyncResponse)ResponseFabric.NotFound()).Send(context.Response);

                Log($"route for '{context.Request.Url}' not found", MessageType.Warning);
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
        IRequestHandler handler
    )
    {
        IResponse response;

        using (request.BodyReader)
        using (_services.RegisterInstance(request))
        {
            Log($"handling route '{route}'", MessageType.Info);

            response = handler.Handle(request);
        }

        var isInternalError = response.StatusCode is HttpStatusCode.InternalServerError;

        if (isInternalError)
        {
            var content = response is SyncResponse { Content: var syncContent }
                ? syncContent
                : "<cannot read async content>";

            Log($"internal error: {content}", MessageType.Error);
        }

        switch (response)
        {
            case SyncResponse syncResponse:
                syncResponse.Send(context.Response);

                Log(
                    $"sent response {response.StatusCode} to {request.HttpMethod} request at '{route}'",
                    isInternalError ? MessageType.Error : MessageType.Info
                );
                break;

            case CoroutineResponse coroutineResponse:
                GlobalCoroutine.Start(
                    CoroutineRequestHandler(
                        request.HttpMethod,
                        route,
                        context.Response,
                        coroutineResponse
                    )
                );
                break;

            default:
                throw new NotSupportedException(
                    $"{response.GetType().FullName} response type is not supported"
                );
        }
    }

    private IEnumerator CoroutineRequestHandler(
        HttpMethod httpMethod,
        Route route,
        HttpListenerResponse listenerResponse,
        CoroutineResponse coroutineResponse
    )
    {
        var coroutine = coroutineResponse.Coroutine;

        using (var contentWriter = new StreamWriter(listenerResponse.OutputStream))
        {
            while (true)
            {
                try
                {
                    if (coroutine.MoveNext() is false)
                    {
                        break;
                    }
                }
                catch (Exception exception)
                {
                    Log(
                        $"unhandled exception in async {httpMethod} request at '{route}'",
                        MessageType.Error
                    );

                    Log(
                        $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}",
                        MessageType.Error
                    );
                    break;
                }

                switch (coroutine.Current)
                {
                    case string contentChunk:
                        contentWriter.Write(contentChunk);
                        contentWriter.Flush();
                        break;

                    case var coroutineControl:
                        yield return coroutineControl;
                        break;
                }
            }
        }

        listenerResponse.Close();

        Log(
            $"sent response {coroutineResponse.StatusCode} to {httpMethod} request at '{route}'",
            coroutineResponse.StatusCode is HttpStatusCode.InternalServerError
                ? MessageType.Error
                : MessageType.Info
        );
    }

    private void Log(string message, MessageType messageType)
    {
        var modConsole = _services.Resolve<IModConsole>();

        modConsole?.WriteLine($"{nameof(SceneRecorder)} API: {message}", messageType);
    }
}

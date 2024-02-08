using System.Collections;
using System.Net;
using Newtonsoft.Json;
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

                SendSyncResponse(context, ResponseFabric.NotFound());

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

        if (response is CoroutineResponse coroutineResponse)
        {
            GlobalCoroutine.Start(
                HandleCoroutineResponse(context, request.HttpMethod, route, coroutineResponse)
            );
        }
        else
        {
            SendSyncResponse(context, response);
        }
    }

    private void SendSyncResponse(HttpListenerContext context, IResponse response)
    {
        SetGenericHeaders(context, response);

        var httpResponse = context.Response;

        if (response is EmptyResponse)
        {
            httpResponse.ContentLength64 = 0;
            httpResponse.Close();
            return;
        }

        if (response is StringResponse { Content: var content })
        {
            using (var bodyWriter = new StreamWriter(httpResponse.OutputStream))
            {
                bodyWriter.Write(content);
            }
            httpResponse.Close();
            return;
        }

        if (response is JsonResponse { Value: var value })
        {
            using (var bodyWriter = new StreamWriter(httpResponse.OutputStream))
            {
                var jsonSerializer = _services.Resolve<JsonSerializer>();
                jsonSerializer.ThrowIfNull();
                jsonSerializer.Serialize(bodyWriter, value);
            }
            httpResponse.Close();
            return;
        }

        throw new NotImplementedException();
    }

    private IEnumerator HandleCoroutineResponse(
        HttpListenerContext context,
        HttpMethod httpMethod,
        Route route,
        CoroutineResponse coroutineResponse
    )
    {
        SetGenericHeaders(context, coroutineResponse);

        var httpResponse = context.Response;
        var coroutine = coroutineResponse.Coroutine;

        using (var contentWriter = new StreamWriter(httpResponse.OutputStream))
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

        httpResponse.Close();

        Log(
            $"sent response {coroutineResponse.StatusCode} to {httpMethod} request at '{route}'",
            coroutineResponse.StatusCode is HttpStatusCode.InternalServerError
                ? MessageType.Error
                : MessageType.Info
        );
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
        var modConsole = _services.Resolve<IModConsole>();

        modConsole?.WriteLine($"{nameof(SceneRecorder)} API: {message}", messageType);
    }
}

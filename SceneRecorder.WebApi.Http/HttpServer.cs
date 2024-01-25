using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using OWML.Common;
using SceneRecorder.Infrastructure.Extensions;
using SceneRecorder.WebApi.Http.Response;
using SceneRecorder.WebApi.Http.Routing;
using UnityEngine;

namespace SceneRecorder.WebApi.Http;

public class HttpServer : MonoBehaviour
{
    public IModConsole? ModConsole { get; set; } = null;

    public bool Listening { get; private set; } = false;

    private string _BaseUrl = null!;

    private HttpListener _HttpListener = null!;

    private Router _Router = null!;

    private CancellationTokenSource? _CancellationTokenSource = null;

    private TaskCompletionSource<object?>? _StoppedListening;

    private readonly ConcurrentQueue<Action> _UnityThreadActionQueue = new();

    internal void Configure(string baseUrl, IReadOnlyList<RequestHandler> requestHandlers)
    {
        if (Listening)
        {
            throw new InvalidOperationException(
                $"{nameof(Configure)} method called before {nameof(StopListening)}"
            );
        }

        if (baseUrl.EndsWith("/") is false)
        {
            throw new ArgumentException("must end with /", nameof(baseUrl));
        }

        _Router = new(requestHandlers);

        _BaseUrl = baseUrl;

        _HttpListener = new();

        _HttpListener.Prefixes.Add(_BaseUrl);
    }

    public void StartListening()
    {
        if (Listening)
        {
            throw new InvalidOperationException(
                $"{nameof(StartListening)} method called before {nameof(StopListening)}"
            );
        }

        Listening = true;

        Log($"started listening at {_HttpListener.Prefixes.Single()}", MessageType.Info);

        Task.Run(Listen);
    }

    public void StopListening()
    {
        if (Listening is false)
        {
            throw new InvalidOperationException(
                $"{nameof(StopListening)} method called before {nameof(StartListening)}"
            );
        }

        Listening = false;
        _CancellationTokenSource!.Cancel();
        _StoppedListening!.Task.Wait();

        Log($"stopped listening", MessageType.Info);
    }

    public async Task StopListeningAsync()
    {
        StopListening();

        await _StoppedListening!.Task;
    }

    private void OnDestroy()
    {
        StopListening();
    }

    private async Task Listen()
    {
        _StoppedListening = new();
        _HttpListener.Start();

        using var cancellationTokenSource = new CancellationTokenSource();
        _CancellationTokenSource = cancellationTokenSource;
        var cancellationToken = _CancellationTokenSource.Token;

        while (Listening)
        {
            HttpListenerContext context;
            try
            {
                context = await _HttpListener.GetContextAsync().AsCancellable(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }

            string requestContent;
            using (
                var requestContentReader = new StreamReader(
                    context.Request.InputStream,
                    context.Request.ContentEncoding
                )
            )
            {
                requestContent = requestContentReader.ReadToEnd();
            }

            var httpMethod = new HttpMethod(context.Request.HttpMethod);

            Log($"received {httpMethod} request at '{context.Request.Url}'", MessageType.Info);

            Request request;

            try
            {
                request = new Request(httpMethod, context.Request.Url, requestContent);
            }
            catch (JsonSerializationException exception)
            {
                var response = ResponseFabric.BadRequest(new { exception.Message });
                ((SyncResponse)response).Send(context.Response);

                Log(
                    $"bad request at '{context.Request.Url}': {exception.Message}",
                    MessageType.Warning
                );

                continue;
            }

            var requestHandler = _Router.Match(request);

            if (requestHandler is not null)
            {
                _UnityThreadActionQueue.Enqueue(
                    () => HandleRequest(context, request, requestHandler)
                );
            }
            else
            {
                ((SyncResponse)ResponseFabric.NotFound()).Send(context.Response);

                Log($"route '{context.Request.Url}' not found", MessageType.Warning);
            }
        }

        _UnityThreadActionQueue.Clear();
        StopAllCoroutines();

        _HttpListener.Stop();
        _StoppedListening.SetResult(null);
    }

    private void HandleRequest(HttpListenerContext context, Request request, RequestHandler handler)
    {
        var response = handler.Handle(request);

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
                    $"sent response {response.StatusCode} to {request.HttpMethod} request at '{handler.Route}'",
                    isInternalError ? MessageType.Error : MessageType.Info
                );
                break;

            case CoroutineResponse coroutineResponse:
                StartCoroutine(
                    CoroutineRequestHandler(
                        request.HttpMethod,
                        handler.Route,
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
        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: {message}", messageType);
    }

    private void Update()
    {
        if (_UnityThreadActionQueue.TryDequeue(out var action))
        {
            action();
        }
    }
}

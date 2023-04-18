using OWML.Common;
using Picalines.OuterWilds.SceneRecorder.WebApi.Http.Extensions;
using System.Collections.Concurrent;
using System.Net;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

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
            throw new InvalidOperationException($"{nameof(Configure)} method called before {nameof(StopListening)}");
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
            throw new InvalidOperationException($"{nameof(StartListening)} method called before {nameof(StopListening)}");
        }

        Listening = true;

        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: started listening at {_HttpListener.Prefixes.Single()}", MessageType.Info);

        Task.Run(Listen);
    }

    public void StopListening()
    {
        if (Listening is false)
        {
            throw new InvalidOperationException($"{nameof(StopListening)} method called before {nameof(StartListening)}");
        }

        Listening = false;
        _CancellationTokenSource!.Cancel();
        _StoppedListening!.Task.Wait();

        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: stopped listening", MessageType.Info);
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
            using (var requestContentReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestContent = requestContentReader.ReadToEnd();
            }

            var httpMethod = new HttpMethod(context.Request.HttpMethod);
            var request = new Request(httpMethod, context.Request.Url, requestContent);

            var requestHandler = _Router.Match(request);

            if (requestHandler is not null)
            {
                _UnityThreadActionQueue.Enqueue(() => HandleRequest(context, request, requestHandler));
            }
            else
            {
                ResponseFabric.NotFound().Send(context.Response);
            }
        }

        _UnityThreadActionQueue.Clear();

        _HttpListener.Stop();
        _StoppedListening.SetResult(null);
    }

    private void HandleRequest(HttpListenerContext context, Request request, RequestHandler handler)
    {
        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: handling {request.HttpMethod} request at '{request.Uri}'", MessageType.Info);

        var response = handler.Handle(request);

        var isInternalError = response.StatusCode is HttpStatusCode.InternalServerError;

        if (isInternalError)
        {
            ModConsole?.WriteLine($"{nameof(SceneRecorder)} API internal error: {response.Content}", MessageType.Error);
        }

        response.Send(context.Response);

        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: sent response {response.StatusCode} to {request.HttpMethod} request at '{handler.Route}'",
            isInternalError ? MessageType.Error : MessageType.Info);
    }

    private void Update()
    {
        if (_UnityThreadActionQueue.TryDequeue(out var action))
        {
            action();
        }
    }
}

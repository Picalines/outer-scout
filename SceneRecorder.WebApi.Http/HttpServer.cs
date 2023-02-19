using OWML.Common;
using System.Collections.Concurrent;
using System.Net;
using System.Web;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

public class HttpServer : MonoBehaviour
{
    public IModConsole? ModConsole { get; set; } = null;

    public bool Listening { get; private set; } = false;

    private string _BaseUrl = null!;

    private HttpListener _HttpListener = null!;

    private IReadOnlyList<RequestHandler> _RequestHandlers = null!;

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

        _BaseUrl = baseUrl;

        _HttpListener = new();
        _RequestHandlers = requestHandlers;

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

        while (Listening)
        {
            var context = await _HttpListener.GetContextAsync();

            if (Enum.TryParse(context.Request.HttpMethod, out HttpMethod httpMethod) is false)
            {
                continue;
            }

            string requestContent;
            using (var requestContentReader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                requestContent = requestContentReader.ReadToEnd();
            }

            var request = new Request(
                httpMethod,
                HttpUtility.UrlDecode(context.Request.Url.ToString()).Substring(_BaseUrl.Length),
                requestContent);

            bool handled = false;
            foreach (var handler in _RequestHandlers)
            {
                if (handler.Route.MatchRequest(request))
                {
                    _UnityThreadActionQueue.Enqueue(() => HandleRequest(context, request, handler));
                    handled = true;
                    break;
                }
            }

            if (handled is false)
            {
                ResponseFabric.NotFound().Send(context.Response);
            }
        }

        _HttpListener.Stop();
        _StoppedListening.SetResult(null);
    }

    private void HandleRequest(HttpListenerContext context, Request request, RequestHandler handler)
    {
        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: handling {request.HttpMethod} request at '{request.Url}'", MessageType.Info);

        var response = handler.Handle(request);

        var isInternalError = response.StatusCode is HttpStatusCode.InternalServerError;

        if (isInternalError)
        {
            ModConsole?.WriteLine($"{nameof(SceneRecorder)} API internal error: {response.Content}", MessageType.Error);
        }

        response.Send(context.Response);

        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: sent response {response.StatusCode} to {request.HttpMethod} request at '{request.Url}'",
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

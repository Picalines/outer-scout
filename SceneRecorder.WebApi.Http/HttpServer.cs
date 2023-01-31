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
            throw new InvalidOperationException();
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
            throw new InvalidOperationException();
        }

        Listening = true;

        Task.Run(Listen);
    }

    public void StopListening()
    {
        if (Listening is false)
        {
            throw new InvalidOperationException();
        }

        Listening = false;
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
                    _UnityThreadActionQueue.Enqueue(() =>
                    {
                        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: handling {request.HttpMethod} request at '{request.Url}'", MessageType.Info);
                        var response = handler.Handle(request);
                        ModConsole?.WriteLine($"{nameof(SceneRecorder)} API: sending response {response.StatusCode} to {request.HttpMethod} request at '{request.Url}'", MessageType.Info);
                        response.ToHttpListenerResponse(context.Response);
                    });
                    handled = true;
                    break;
                }
            }

            if (handled is false)
            {
                ResponseFabric.NotFound().ToHttpListenerResponse(context.Response);
            }
        }

        _HttpListener.Stop();
        _StoppedListening.SetResult(null);
    }

    private void Update()
    {
        if (_UnityThreadActionQueue.TryDequeue(out var action))
        {
            action();
        }
    }
}

using System.Collections.Concurrent;
using System.Net;
using UnityEngine;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal sealed class HttpServer : MonoBehaviour
{
    public bool Listening { get; private set; } = false;

    private string _BaseUrl = null!;

    private HttpListener _HttpListener = null!;

    private IReadOnlyList<RequestHandler> _RequestHandlers = null!;

    private TaskCompletionSource<object?>? _StoppedListening;

    private readonly ConcurrentQueue<Action> _UnityThreadActionQueue = new();

    public void Configure(string baseUrl, IReadOnlyList<RequestHandler> requestHandlers)
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

            var request = new Request(httpMethod, context.Request.Url.ToString().Substring(_BaseUrl.Length));

            bool handled = false;
            foreach (var handler in _RequestHandlers)
            {
                if (handler.Route.MatchRequest(request))
                {
                    _UnityThreadActionQueue.Enqueue(() =>
                    {
                        var response = handler.Handle(request);
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

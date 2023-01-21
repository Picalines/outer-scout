using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.Http;

public sealed class HttpServer
{
    public bool Listening { get; private set; } = false;

    private readonly string _BaseUrl;

    private readonly HttpListener _HttpListener;

    private readonly IReadOnlyList<IRequestHandler> _RequestHandlers;

    private TaskCompletionSource<object?>? _StoppedListening;

    internal HttpServer(string baseUrl, IReadOnlyList<IRequestHandler> requestHandlers)
    {
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
                if (handler.Route.TrySetRequestParameters(request))
                {
                    handler.BuildResponse(request, context.Response);
                    handled = true;
                    break;
                }
            }

            if (handled is false)
            {
                var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.OutputStream.Close();
            }
        }

        _HttpListener.Stop();
        _StoppedListening.SetResult(null);
    }
}

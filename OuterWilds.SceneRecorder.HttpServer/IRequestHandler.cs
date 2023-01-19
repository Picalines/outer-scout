using System.Net;

namespace OuterWilds.SceneRecorder.HttpServer;

internal interface IRequestHandler
{
    public Route Route { get; }

    public void BuildResponse(Request request, HttpListenerResponse listenerResponse);
}

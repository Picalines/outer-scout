using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.Http;

internal interface IRequestHandler
{
    public Route Route { get; }

    public void BuildResponse(Request request, HttpListenerResponse listenerResponse);
}

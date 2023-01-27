﻿namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal abstract class RequestHandler
{
    public Route Route { get; }

    public RequestHandler(Route route)
    {
        Route = route;
    }

    protected abstract Response HandleInternal(Request request);

    public Response Handle(Request request)
    {
        try
        {
            return HandleInternal(request);
        }
        catch
        {
            return ResponseFabric.InternalServerError();
        }
    }
}
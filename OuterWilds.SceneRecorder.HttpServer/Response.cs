using System.Net;

namespace OuterWilds.SceneRecorder.HttpServer;

public sealed class Response<T>
{
    public HttpStatusCode StatusCode { get; }

    public T Value { get; }

    public Response(HttpStatusCode statusCode, T value)
    {
        StatusCode = statusCode;
        Value = value;
    }
}

public static class Response
{
    public static Response<T> Continue<T>(T value) => new(HttpStatusCode.Continue, value);
    public static Response<T> SwitchingProtocols<T>(T value) => new(HttpStatusCode.SwitchingProtocols, value);
    public static Response<T> Ok<T>(T value) => new(HttpStatusCode.OK, value);
    public static Response<T> Created<T>(T value) => new(HttpStatusCode.Created, value);
    public static Response<T> Accepted<T>(T value) => new(HttpStatusCode.Accepted, value);
    public static Response<T> NonAuthoritativeInformation<T>(T value) => new(HttpStatusCode.NonAuthoritativeInformation, value);
    public static Response<T> NoContent<T>(T value) => new(HttpStatusCode.NoContent, value);
    public static Response<T> ResetContent<T>(T value) => new(HttpStatusCode.ResetContent, value);
    public static Response<T> PartialContent<T>(T value) => new(HttpStatusCode.PartialContent, value);
    public static Response<T> MultipleChoices<T>(T value) => new(HttpStatusCode.MultipleChoices, value);
    public static Response<T> Ambiguous<T>(T value) => new(HttpStatusCode.Ambiguous, value);
    public static Response<T> MovedPermanently<T>(T value) => new(HttpStatusCode.MovedPermanently, value);
    public static Response<T> Moved<T>(T value) => new(HttpStatusCode.Moved, value);
    public static Response<T> Found<T>(T value) => new(HttpStatusCode.Found, value);
    public static Response<T> Redirect<T>(T value) => new(HttpStatusCode.Redirect, value);
    public static Response<T> SeeOther<T>(T value) => new(HttpStatusCode.SeeOther, value);
    public static Response<T> RedirectMethod<T>(T value) => new(HttpStatusCode.RedirectMethod, value);
    public static Response<T> NotModified<T>(T value) => new(HttpStatusCode.NotModified, value);
    public static Response<T> UseProxy<T>(T value) => new(HttpStatusCode.UseProxy, value);
    public static Response<T> Unused<T>(T value) => new(HttpStatusCode.Unused, value);
    public static Response<T> TemporaryRedirect<T>(T value) => new(HttpStatusCode.TemporaryRedirect, value);
    public static Response<T> RedirectKeepVerb<T>(T value) => new(HttpStatusCode.RedirectKeepVerb, value);
    public static Response<T> BadRequest<T>(T value) => new(HttpStatusCode.BadRequest, value);
    public static Response<T> Unauthorized<T>(T value) => new(HttpStatusCode.Unauthorized, value);
    public static Response<T> PaymentRequired<T>(T value) => new(HttpStatusCode.PaymentRequired, value);
    public static Response<T> Forbidden<T>(T value) => new(HttpStatusCode.Forbidden, value);
    public static Response<T> NotFound<T>(T value) => new(HttpStatusCode.NotFound, value);
    public static Response<T> MethodNotAllowed<T>(T value) => new(HttpStatusCode.MethodNotAllowed, value);
    public static Response<T> NotAcceptable<T>(T value) => new(HttpStatusCode.NotAcceptable, value);
    public static Response<T> ProxyAuthenticationRequired<T>(T value) => new(HttpStatusCode.ProxyAuthenticationRequired, value);
    public static Response<T> RequestTimeout<T>(T value) => new(HttpStatusCode.RequestTimeout, value);
    public static Response<T> Conflict<T>(T value) => new(HttpStatusCode.Conflict, value);
    public static Response<T> Gone<T>(T value) => new(HttpStatusCode.Gone, value);
    public static Response<T> LengthRequired<T>(T value) => new(HttpStatusCode.LengthRequired, value);
    public static Response<T> PreconditionFailed<T>(T value) => new(HttpStatusCode.PreconditionFailed, value);
    public static Response<T> RequestEntityTooLarge<T>(T value) => new(HttpStatusCode.RequestEntityTooLarge, value);
    public static Response<T> RequestUriTooLong<T>(T value) => new(HttpStatusCode.RequestUriTooLong, value);
    public static Response<T> UnsupportedMediaType<T>(T value) => new(HttpStatusCode.UnsupportedMediaType, value);
    public static Response<T> RequestedRangeNotSatisfiable<T>(T value) => new(HttpStatusCode.RequestedRangeNotSatisfiable, value);
    public static Response<T> ExpectationFailed<T>(T value) => new(HttpStatusCode.ExpectationFailed, value);
    public static Response<T> UpgradeRequired<T>(T value) => new(HttpStatusCode.UpgradeRequired, value);
    public static Response<T> InternalServerError<T>(T value) => new(HttpStatusCode.InternalServerError, value);
    public static Response<T> NotImplemented<T>(T value) => new(HttpStatusCode.NotImplemented, value);
    public static Response<T> BadGateway<T>(T value) => new(HttpStatusCode.BadGateway, value);
    public static Response<T> ServiceUnavailable<T>(T value) => new(HttpStatusCode.ServiceUnavailable, value);
    public static Response<T> GatewayTimeout<T>(T value) => new(HttpStatusCode.GatewayTimeout, value);
    public static Response<T> HttpVersionNotSupported<T>(T value) => new(HttpStatusCode.HttpVersionNotSupported, value);
}

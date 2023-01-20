using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.Http;

public sealed class Response<T>
{
    public HttpStatusCode StatusCode { get; }

    private readonly T _Value;

    private readonly bool _HasValue;

    internal Response(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
        _Value = default!;
        _HasValue = false;
    }

    internal Response(HttpStatusCode statusCode, T value)
        : this(statusCode)
    {
        _Value = value;
        _HasValue = true;
    }

    public bool HasValue
    {
        get => _HasValue;
    }

    public T Value
    {
        get => _HasValue
            ? _Value
            : throw new InvalidOperationException();
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

    public static Response<T> Continue<T>() => new(HttpStatusCode.Continue);
    public static Response<T> SwitchingProtocols<T>() => new(HttpStatusCode.SwitchingProtocols);
    public static Response<T> Ok<T>() => new(HttpStatusCode.OK);
    public static Response<T> Created<T>() => new(HttpStatusCode.Created);
    public static Response<T> Accepted<T>() => new(HttpStatusCode.Accepted);
    public static Response<T> NonAuthoritativeInformation<T>() => new(HttpStatusCode.NonAuthoritativeInformation);
    public static Response<T> NoContent<T>() => new(HttpStatusCode.NoContent);
    public static Response<T> ResetContent<T>() => new(HttpStatusCode.ResetContent);
    public static Response<T> PartialContent<T>() => new(HttpStatusCode.PartialContent);
    public static Response<T> MultipleChoices<T>() => new(HttpStatusCode.MultipleChoices);
    public static Response<T> Ambiguous<T>() => new(HttpStatusCode.Ambiguous);
    public static Response<T> MovedPermanently<T>() => new(HttpStatusCode.MovedPermanently);
    public static Response<T> Moved<T>() => new(HttpStatusCode.Moved);
    public static Response<T> Found<T>() => new(HttpStatusCode.Found);
    public static Response<T> Redirect<T>() => new(HttpStatusCode.Redirect);
    public static Response<T> SeeOther<T>() => new(HttpStatusCode.SeeOther);
    public static Response<T> RedirectMethod<T>() => new(HttpStatusCode.RedirectMethod);
    public static Response<T> NotModified<T>() => new(HttpStatusCode.NotModified);
    public static Response<T> UseProxy<T>() => new(HttpStatusCode.UseProxy);
    public static Response<T> Unused<T>() => new(HttpStatusCode.Unused);
    public static Response<T> TemporaryRedirect<T>() => new(HttpStatusCode.TemporaryRedirect);
    public static Response<T> RedirectKeepVerb<T>() => new(HttpStatusCode.RedirectKeepVerb);
    public static Response<T> BadRequest<T>() => new(HttpStatusCode.BadRequest);
    public static Response<T> Unauthorized<T>() => new(HttpStatusCode.Unauthorized);
    public static Response<T> PaymentRequired<T>() => new(HttpStatusCode.PaymentRequired);
    public static Response<T> Forbidden<T>() => new(HttpStatusCode.Forbidden);
    public static Response<T> NotFound<T>() => new(HttpStatusCode.NotFound);
    public static Response<T> MethodNotAllowed<T>() => new(HttpStatusCode.MethodNotAllowed);
    public static Response<T> NotAcceptable<T>() => new(HttpStatusCode.NotAcceptable);
    public static Response<T> ProxyAuthenticationRequired<T>() => new(HttpStatusCode.ProxyAuthenticationRequired);
    public static Response<T> RequestTimeout<T>() => new(HttpStatusCode.RequestTimeout);
    public static Response<T> Conflict<T>() => new(HttpStatusCode.Conflict);
    public static Response<T> Gone<T>() => new(HttpStatusCode.Gone);
    public static Response<T> LengthRequired<T>() => new(HttpStatusCode.LengthRequired);
    public static Response<T> PreconditionFailed<T>() => new(HttpStatusCode.PreconditionFailed);
    public static Response<T> RequestEntityTooLarge<T>() => new(HttpStatusCode.RequestEntityTooLarge);
    public static Response<T> RequestUriTooLong<T>() => new(HttpStatusCode.RequestUriTooLong);
    public static Response<T> UnsupportedMediaType<T>() => new(HttpStatusCode.UnsupportedMediaType);
    public static Response<T> RequestedRangeNotSatisfiable<T>() => new(HttpStatusCode.RequestedRangeNotSatisfiable);
    public static Response<T> ExpectationFailed<T>() => new(HttpStatusCode.ExpectationFailed);
    public static Response<T> UpgradeRequired<T>() => new(HttpStatusCode.UpgradeRequired);
    public static Response<T> InternalServerError<T>() => new(HttpStatusCode.InternalServerError);
    public static Response<T> NotImplemented<T>() => new(HttpStatusCode.NotImplemented);
    public static Response<T> BadGateway<T>() => new(HttpStatusCode.BadGateway);
    public static Response<T> ServiceUnavailable<T>() => new(HttpStatusCode.ServiceUnavailable);
    public static Response<T> GatewayTimeout<T>() => new(HttpStatusCode.GatewayTimeout);
    public static Response<T> HttpVersionNotSupported<T>() => new(HttpStatusCode.HttpVersionNotSupported);
}

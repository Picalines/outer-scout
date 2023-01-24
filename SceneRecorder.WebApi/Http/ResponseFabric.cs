using System.Net;

namespace Picalines.OuterWilds.SceneRecorder.WebApi.Http;

internal static class ResponseFabric
{
    public static Response Continue() => Response.Empty(HttpStatusCode.Continue);
    public static Response SwitchingProtocols() => Response.Empty(HttpStatusCode.SwitchingProtocols);
    public static Response Ok() => Response.Empty(HttpStatusCode.OK);
    public static Response Created() => Response.Empty(HttpStatusCode.Created);
    public static Response Accepted() => Response.Empty(HttpStatusCode.Accepted);
    public static Response NonAuthoritativeInformation() => Response.Empty(HttpStatusCode.NonAuthoritativeInformation);
    public static Response NoContent() => Response.Empty(HttpStatusCode.NoContent);
    public static Response ResetContent() => Response.Empty(HttpStatusCode.ResetContent);
    public static Response PartialContent() => Response.Empty(HttpStatusCode.PartialContent);
    public static Response MultipleChoices() => Response.Empty(HttpStatusCode.MultipleChoices);
    public static Response Ambiguous() => Response.Empty(HttpStatusCode.Ambiguous);
    public static Response MovedPermanently() => Response.Empty(HttpStatusCode.MovedPermanently);
    public static Response Moved() => Response.Empty(HttpStatusCode.Moved);
    public static Response Found() => Response.Empty(HttpStatusCode.Found);
    public static Response Redirect() => Response.Empty(HttpStatusCode.Redirect);
    public static Response SeeOther() => Response.Empty(HttpStatusCode.SeeOther);
    public static Response RedirectMethod() => Response.Empty(HttpStatusCode.RedirectMethod);
    public static Response NotModified() => Response.Empty(HttpStatusCode.NotModified);
    public static Response UseProxy() => Response.Empty(HttpStatusCode.UseProxy);
    public static Response Unused() => Response.Empty(HttpStatusCode.Unused);
    public static Response TemporaryRedirect() => Response.Empty(HttpStatusCode.TemporaryRedirect);
    public static Response RedirectKeepVerb() => Response.Empty(HttpStatusCode.RedirectKeepVerb);
    public static Response BadRequest() => Response.Empty(HttpStatusCode.BadRequest);
    public static Response Unauthorized() => Response.Empty(HttpStatusCode.Unauthorized);
    public static Response PaymentRequired() => Response.Empty(HttpStatusCode.PaymentRequired);
    public static Response Forbidden() => Response.Empty(HttpStatusCode.Forbidden);
    public static Response NotFound() => Response.Empty(HttpStatusCode.NotFound);
    public static Response MethodNotAllowed() => Response.Empty(HttpStatusCode.MethodNotAllowed);
    public static Response NotAcceptable() => Response.Empty(HttpStatusCode.NotAcceptable);
    public static Response ProxyAuthenticationRequired() => Response.Empty(HttpStatusCode.ProxyAuthenticationRequired);
    public static Response RequestTimeout() => Response.Empty(HttpStatusCode.RequestTimeout);
    public static Response Conflict() => Response.Empty(HttpStatusCode.Conflict);
    public static Response Gone() => Response.Empty(HttpStatusCode.Gone);
    public static Response LengthRequired() => Response.Empty(HttpStatusCode.LengthRequired);
    public static Response PreconditionFailed() => Response.Empty(HttpStatusCode.PreconditionFailed);
    public static Response RequestEntityTooLarge() => Response.Empty(HttpStatusCode.RequestEntityTooLarge);
    public static Response RequestUriTooLong() => Response.Empty(HttpStatusCode.RequestUriTooLong);
    public static Response UnsupportedMediaType() => Response.Empty(HttpStatusCode.UnsupportedMediaType);
    public static Response RequestedRangeNotSatisfiable() => Response.Empty(HttpStatusCode.RequestedRangeNotSatisfiable);
    public static Response ExpectationFailed() => Response.Empty(HttpStatusCode.ExpectationFailed);
    public static Response UpgradeRequired() => Response.Empty(HttpStatusCode.UpgradeRequired);
    public static Response InternalServerError() => Response.Empty(HttpStatusCode.InternalServerError);
    public static Response NotImplemented() => Response.Empty(HttpStatusCode.NotImplemented);
    public static Response BadGateway() => Response.Empty(HttpStatusCode.BadGateway);
    public static Response ServiceUnavailable() => Response.Empty(HttpStatusCode.ServiceUnavailable);
    public static Response GatewayTimeout() => Response.Empty(HttpStatusCode.GatewayTimeout);
    public static Response HttpVersionNotSupported() => Response.Empty(HttpStatusCode.HttpVersionNotSupported);

    public static Response Continue(string message) => Response.FromString(HttpStatusCode.Continue, message);
    public static Response SwitchingProtocols(string message) => Response.FromString(HttpStatusCode.SwitchingProtocols, message);
    public static Response Ok(string message) => Response.FromString(HttpStatusCode.OK, message);
    public static Response Created(string message) => Response.FromString(HttpStatusCode.Created, message);
    public static Response Accepted(string message) => Response.FromString(HttpStatusCode.Accepted, message);
    public static Response NonAuthoritativeInformation(string message) => Response.FromString(HttpStatusCode.NonAuthoritativeInformation, message);
    public static Response NoContent(string message) => Response.FromString(HttpStatusCode.NoContent, message);
    public static Response ResetContent(string message) => Response.FromString(HttpStatusCode.ResetContent, message);
    public static Response PartialContent(string message) => Response.FromString(HttpStatusCode.PartialContent, message);
    public static Response MultipleChoices(string message) => Response.FromString(HttpStatusCode.MultipleChoices, message);
    public static Response Ambiguous(string message) => Response.FromString(HttpStatusCode.Ambiguous, message);
    public static Response MovedPermanently(string message) => Response.FromString(HttpStatusCode.MovedPermanently, message);
    public static Response Moved(string message) => Response.FromString(HttpStatusCode.Moved, message);
    public static Response Found(string message) => Response.FromString(HttpStatusCode.Found, message);
    public static Response Redirect(string message) => Response.FromString(HttpStatusCode.Redirect, message);
    public static Response SeeOther(string message) => Response.FromString(HttpStatusCode.SeeOther, message);
    public static Response RedirectMethod(string message) => Response.FromString(HttpStatusCode.RedirectMethod, message);
    public static Response NotModified(string message) => Response.FromString(HttpStatusCode.NotModified, message);
    public static Response UseProxy(string message) => Response.FromString(HttpStatusCode.UseProxy, message);
    public static Response Unused(string message) => Response.FromString(HttpStatusCode.Unused, message);
    public static Response TemporaryRedirect(string message) => Response.FromString(HttpStatusCode.TemporaryRedirect, message);
    public static Response RedirectKeepVerb(string message) => Response.FromString(HttpStatusCode.RedirectKeepVerb, message);
    public static Response BadRequest(string message) => Response.FromString(HttpStatusCode.BadRequest, message);
    public static Response Unauthorized(string message) => Response.FromString(HttpStatusCode.Unauthorized, message);
    public static Response PaymentRequired(string message) => Response.FromString(HttpStatusCode.PaymentRequired, message);
    public static Response Forbidden(string message) => Response.FromString(HttpStatusCode.Forbidden, message);
    public static Response NotFound(string message) => Response.FromString(HttpStatusCode.NotFound, message);
    public static Response MethodNotAllowed(string message) => Response.FromString(HttpStatusCode.MethodNotAllowed, message);
    public static Response NotAcceptable(string message) => Response.FromString(HttpStatusCode.NotAcceptable, message);
    public static Response ProxyAuthenticationRequired(string message) => Response.FromString(HttpStatusCode.ProxyAuthenticationRequired, message);
    public static Response RequestTimeout(string message) => Response.FromString(HttpStatusCode.RequestTimeout, message);
    public static Response Conflict(string message) => Response.FromString(HttpStatusCode.Conflict, message);
    public static Response Gone(string message) => Response.FromString(HttpStatusCode.Gone, message);
    public static Response LengthRequired(string message) => Response.FromString(HttpStatusCode.LengthRequired, message);
    public static Response PreconditionFailed(string message) => Response.FromString(HttpStatusCode.PreconditionFailed, message);
    public static Response RequestEntityTooLarge(string message) => Response.FromString(HttpStatusCode.RequestEntityTooLarge, message);
    public static Response RequestUriTooLong(string message) => Response.FromString(HttpStatusCode.RequestUriTooLong, message);
    public static Response UnsupportedMediaType(string message) => Response.FromString(HttpStatusCode.UnsupportedMediaType, message);
    public static Response RequestedRangeNotSatisfiable(string message) => Response.FromString(HttpStatusCode.RequestedRangeNotSatisfiable, message);
    public static Response ExpectationFailed(string message) => Response.FromString(HttpStatusCode.ExpectationFailed, message);
    public static Response UpgradeRequired(string message) => Response.FromString(HttpStatusCode.UpgradeRequired, message);
    public static Response InternalServerError(string message) => Response.FromString(HttpStatusCode.InternalServerError, message);
    public static Response NotImplemented(string message) => Response.FromString(HttpStatusCode.NotImplemented, message);
    public static Response BadGateway(string message) => Response.FromString(HttpStatusCode.BadGateway, message);
    public static Response ServiceUnavailable(string message) => Response.FromString(HttpStatusCode.ServiceUnavailable, message);
    public static Response GatewayTimeout(string message) => Response.FromString(HttpStatusCode.GatewayTimeout, message);
    public static Response HttpVersionNotSupported(string message) => Response.FromString(HttpStatusCode.HttpVersionNotSupported, message);

    private static Response FromValue<T>(HttpStatusCode httpStatusCode, T value)
    {
        return value switch
        {
            string stringValue => Response.FromString(httpStatusCode, stringValue),
            _ => Response.FromJson(httpStatusCode, value),
        };
    }

    public static Response Continue<T>(T value) => FromValue(HttpStatusCode.Continue, value);
    public static Response SwitchingProtocols<T>(T value) => FromValue(HttpStatusCode.SwitchingProtocols, value);
    public static Response Ok<T>(T value) => FromValue(HttpStatusCode.OK, value);
    public static Response Created<T>(T value) => FromValue(HttpStatusCode.Created, value);
    public static Response Accepted<T>(T value) => FromValue(HttpStatusCode.Accepted, value);
    public static Response NonAuthoritativeInformation<T>(T value) => FromValue(HttpStatusCode.NonAuthoritativeInformation, value);
    public static Response NoContent<T>(T value) => FromValue(HttpStatusCode.NoContent, value);
    public static Response ResetContent<T>(T value) => FromValue(HttpStatusCode.ResetContent, value);
    public static Response PartialContent<T>(T value) => FromValue(HttpStatusCode.PartialContent, value);
    public static Response MultipleChoices<T>(T value) => FromValue(HttpStatusCode.MultipleChoices, value);
    public static Response Ambiguous<T>(T value) => FromValue(HttpStatusCode.Ambiguous, value);
    public static Response MovedPermanently<T>(T value) => FromValue(HttpStatusCode.MovedPermanently, value);
    public static Response Moved<T>(T value) => FromValue(HttpStatusCode.Moved, value);
    public static Response Found<T>(T value) => FromValue(HttpStatusCode.Found, value);
    public static Response Redirect<T>(T value) => FromValue(HttpStatusCode.Redirect, value);
    public static Response SeeOther<T>(T value) => FromValue(HttpStatusCode.SeeOther, value);
    public static Response RedirectMethod<T>(T value) => FromValue(HttpStatusCode.RedirectMethod, value);
    public static Response NotModified<T>(T value) => FromValue(HttpStatusCode.NotModified, value);
    public static Response UseProxy<T>(T value) => FromValue(HttpStatusCode.UseProxy, value);
    public static Response Unused<T>(T value) => FromValue(HttpStatusCode.Unused, value);
    public static Response TemporaryRedirect<T>(T value) => FromValue(HttpStatusCode.TemporaryRedirect, value);
    public static Response RedirectKeepVerb<T>(T value) => FromValue(HttpStatusCode.RedirectKeepVerb, value);
    public static Response BadRequest<T>(T value) => FromValue(HttpStatusCode.BadRequest, value);
    public static Response Unauthorized<T>(T value) => FromValue(HttpStatusCode.Unauthorized, value);
    public static Response PaymentRequired<T>(T value) => FromValue(HttpStatusCode.PaymentRequired, value);
    public static Response Forbidden<T>(T value) => FromValue(HttpStatusCode.Forbidden, value);
    public static Response NotFound<T>(T value) => FromValue(HttpStatusCode.NotFound, value);
    public static Response MethodNotAllowed<T>(T value) => FromValue(HttpStatusCode.MethodNotAllowed, value);
    public static Response NotAcceptable<T>(T value) => FromValue(HttpStatusCode.NotAcceptable, value);
    public static Response ProxyAuthenticationRequired<T>(T value) => FromValue(HttpStatusCode.ProxyAuthenticationRequired, value);
    public static Response RequestTimeout<T>(T value) => FromValue(HttpStatusCode.RequestTimeout, value);
    public static Response Conflict<T>(T value) => FromValue(HttpStatusCode.Conflict, value);
    public static Response Gone<T>(T value) => FromValue(HttpStatusCode.Gone, value);
    public static Response LengthRequired<T>(T value) => FromValue(HttpStatusCode.LengthRequired, value);
    public static Response PreconditionFailed<T>(T value) => FromValue(HttpStatusCode.PreconditionFailed, value);
    public static Response RequestEntityTooLarge<T>(T value) => FromValue(HttpStatusCode.RequestEntityTooLarge, value);
    public static Response RequestUriTooLong<T>(T value) => FromValue(HttpStatusCode.RequestUriTooLong, value);
    public static Response UnsupportedMediaType<T>(T value) => FromValue(HttpStatusCode.UnsupportedMediaType, value);
    public static Response RequestedRangeNotSatisfiable<T>(T value) => FromValue(HttpStatusCode.RequestedRangeNotSatisfiable, value);
    public static Response ExpectationFailed<T>(T value) => FromValue(HttpStatusCode.ExpectationFailed, value);
    public static Response UpgradeRequired<T>(T value) => FromValue(HttpStatusCode.UpgradeRequired, value);
    public static Response InternalServerError<T>(T value) => FromValue(HttpStatusCode.InternalServerError, value);
    public static Response NotImplemented<T>(T value) => FromValue(HttpStatusCode.NotImplemented, value);
    public static Response BadGateway<T>(T value) => FromValue(HttpStatusCode.BadGateway, value);
    public static Response ServiceUnavailable<T>(T value) => FromValue(HttpStatusCode.ServiceUnavailable, value);
    public static Response GatewayTimeout<T>(T value) => FromValue(HttpStatusCode.GatewayTimeout, value);
    public static Response HttpVersionNotSupported<T>(T value) => FromValue(HttpStatusCode.HttpVersionNotSupported, value);
}

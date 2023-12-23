using System.Collections;
using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

using static SyncResponse;

public static class ResponseFabric
{
    private static SyncResponse FromValue<T>(HttpStatusCode httpStatusCode, T value)
    {
        return value switch
        {
            string stringValue => FromString(httpStatusCode, stringValue),
            _ => FromJson(httpStatusCode, value),
        };
    }

    public static IResponse Continue() => Empty(HttpStatusCode.Continue);

    public static IResponse SwitchingProtocols() => Empty(HttpStatusCode.SwitchingProtocols);

    public static IResponse Ok() => Empty(HttpStatusCode.OK);

    public static IResponse Created() => Empty(HttpStatusCode.Created);

    public static IResponse Accepted() => Empty(HttpStatusCode.Accepted);

    public static IResponse NonAuthoritativeInformation() =>
        Empty(HttpStatusCode.NonAuthoritativeInformation);

    public static IResponse NoContent() => Empty(HttpStatusCode.NoContent);

    public static IResponse ResetContent() => Empty(HttpStatusCode.ResetContent);

    public static IResponse PartialContent() => Empty(HttpStatusCode.PartialContent);

    public static IResponse MultipleChoices() => Empty(HttpStatusCode.MultipleChoices);

    public static IResponse Ambiguous() => Empty(HttpStatusCode.Ambiguous);

    public static IResponse MovedPermanently() => Empty(HttpStatusCode.MovedPermanently);

    public static IResponse Moved() => Empty(HttpStatusCode.Moved);

    public static IResponse Found() => Empty(HttpStatusCode.Found);

    public static IResponse Redirect() => Empty(HttpStatusCode.Redirect);

    public static IResponse SeeOther() => Empty(HttpStatusCode.SeeOther);

    public static IResponse RedirectMethod() => Empty(HttpStatusCode.RedirectMethod);

    public static IResponse NotModified() => Empty(HttpStatusCode.NotModified);

    public static IResponse UseProxy() => Empty(HttpStatusCode.UseProxy);

    public static IResponse Unused() => Empty(HttpStatusCode.Unused);

    public static IResponse TemporaryRedirect() => Empty(HttpStatusCode.TemporaryRedirect);

    public static IResponse RedirectKeepVerb() => Empty(HttpStatusCode.RedirectKeepVerb);

    public static IResponse BadRequest() => Empty(HttpStatusCode.BadRequest);

    public static IResponse Unauthorized() => Empty(HttpStatusCode.Unauthorized);

    public static IResponse PaymentRequired() => Empty(HttpStatusCode.PaymentRequired);

    public static IResponse Forbidden() => Empty(HttpStatusCode.Forbidden);

    public static IResponse NotFound() => Empty(HttpStatusCode.NotFound);

    public static IResponse MethodNotAllowed() => Empty(HttpStatusCode.MethodNotAllowed);

    public static IResponse NotAcceptable() => Empty(HttpStatusCode.NotAcceptable);

    public static IResponse ProxyAuthenticationRequired() =>
        Empty(HttpStatusCode.ProxyAuthenticationRequired);

    public static IResponse RequestTimeout() => Empty(HttpStatusCode.RequestTimeout);

    public static IResponse Conflict() => Empty(HttpStatusCode.Conflict);

    public static IResponse Gone() => Empty(HttpStatusCode.Gone);

    public static IResponse LengthRequired() => Empty(HttpStatusCode.LengthRequired);

    public static IResponse PreconditionFailed() => Empty(HttpStatusCode.PreconditionFailed);

    public static IResponse RequestEntityTooLarge() => Empty(HttpStatusCode.RequestEntityTooLarge);

    public static IResponse RequestUriTooLong() => Empty(HttpStatusCode.RequestUriTooLong);

    public static IResponse UnsupportedMediaType() => Empty(HttpStatusCode.UnsupportedMediaType);

    public static IResponse RequestedRangeNotSatisfiable() =>
        Empty(HttpStatusCode.RequestedRangeNotSatisfiable);

    public static IResponse ExpectationFailed() => Empty(HttpStatusCode.ExpectationFailed);

    public static IResponse UpgradeRequired() => Empty(HttpStatusCode.UpgradeRequired);

    public static IResponse InternalServerError() => Empty(HttpStatusCode.InternalServerError);

    public static IResponse NotImplemented() => Empty(HttpStatusCode.NotImplemented);

    public static IResponse BadGateway() => Empty(HttpStatusCode.BadGateway);

    public static IResponse ServiceUnavailable() => Empty(HttpStatusCode.ServiceUnavailable);

    public static IResponse GatewayTimeout() => Empty(HttpStatusCode.GatewayTimeout);

    public static IResponse HttpVersionNotSupported() =>
        Empty(HttpStatusCode.HttpVersionNotSupported);

    public static IResponse Continue(string message) =>
        FromString(HttpStatusCode.Continue, message);

    public static IResponse SwitchingProtocols(string message) =>
        FromString(HttpStatusCode.SwitchingProtocols, message);

    public static IResponse Ok(string message) => FromString(HttpStatusCode.OK, message);

    public static IResponse Created(string message) => FromString(HttpStatusCode.Created, message);

    public static IResponse Accepted(string message) =>
        FromString(HttpStatusCode.Accepted, message);

    public static IResponse NonAuthoritativeInformation(string message) =>
        FromString(HttpStatusCode.NonAuthoritativeInformation, message);

    public static IResponse NoContent(string message) =>
        FromString(HttpStatusCode.NoContent, message);

    public static IResponse ResetContent(string message) =>
        FromString(HttpStatusCode.ResetContent, message);

    public static IResponse PartialContent(string message) =>
        FromString(HttpStatusCode.PartialContent, message);

    public static IResponse MultipleChoices(string message) =>
        FromString(HttpStatusCode.MultipleChoices, message);

    public static IResponse Ambiguous(string message) =>
        FromString(HttpStatusCode.Ambiguous, message);

    public static IResponse MovedPermanently(string message) =>
        FromString(HttpStatusCode.MovedPermanently, message);

    public static IResponse Moved(string message) => FromString(HttpStatusCode.Moved, message);

    public static IResponse Found(string message) => FromString(HttpStatusCode.Found, message);

    public static IResponse Redirect(string message) =>
        FromString(HttpStatusCode.Redirect, message);

    public static IResponse SeeOther(string message) =>
        FromString(HttpStatusCode.SeeOther, message);

    public static IResponse RedirectMethod(string message) =>
        FromString(HttpStatusCode.RedirectMethod, message);

    public static IResponse NotModified(string message) =>
        FromString(HttpStatusCode.NotModified, message);

    public static IResponse UseProxy(string message) =>
        FromString(HttpStatusCode.UseProxy, message);

    public static IResponse Unused(string message) => FromString(HttpStatusCode.Unused, message);

    public static IResponse TemporaryRedirect(string message) =>
        FromString(HttpStatusCode.TemporaryRedirect, message);

    public static IResponse RedirectKeepVerb(string message) =>
        FromString(HttpStatusCode.RedirectKeepVerb, message);

    public static IResponse BadRequest(string message) =>
        FromString(HttpStatusCode.BadRequest, message);

    public static IResponse Unauthorized(string message) =>
        FromString(HttpStatusCode.Unauthorized, message);

    public static IResponse PaymentRequired(string message) =>
        FromString(HttpStatusCode.PaymentRequired, message);

    public static IResponse Forbidden(string message) =>
        FromString(HttpStatusCode.Forbidden, message);

    public static IResponse NotFound(string message) =>
        FromString(HttpStatusCode.NotFound, message);

    public static IResponse MethodNotAllowed(string message) =>
        FromString(HttpStatusCode.MethodNotAllowed, message);

    public static IResponse NotAcceptable(string message) =>
        FromString(HttpStatusCode.NotAcceptable, message);

    public static IResponse ProxyAuthenticationRequired(string message) =>
        FromString(HttpStatusCode.ProxyAuthenticationRequired, message);

    public static IResponse RequestTimeout(string message) =>
        FromString(HttpStatusCode.RequestTimeout, message);

    public static IResponse Conflict(string message) =>
        FromString(HttpStatusCode.Conflict, message);

    public static IResponse Gone(string message) => FromString(HttpStatusCode.Gone, message);

    public static IResponse LengthRequired(string message) =>
        FromString(HttpStatusCode.LengthRequired, message);

    public static IResponse PreconditionFailed(string message) =>
        FromString(HttpStatusCode.PreconditionFailed, message);

    public static IResponse RequestEntityTooLarge(string message) =>
        FromString(HttpStatusCode.RequestEntityTooLarge, message);

    public static IResponse RequestUriTooLong(string message) =>
        FromString(HttpStatusCode.RequestUriTooLong, message);

    public static IResponse UnsupportedMediaType(string message) =>
        FromString(HttpStatusCode.UnsupportedMediaType, message);

    public static IResponse RequestedRangeNotSatisfiable(string message) =>
        FromString(HttpStatusCode.RequestedRangeNotSatisfiable, message);

    public static IResponse ExpectationFailed(string message) =>
        FromString(HttpStatusCode.ExpectationFailed, message);

    public static IResponse UpgradeRequired(string message) =>
        FromString(HttpStatusCode.UpgradeRequired, message);

    public static IResponse InternalServerError(string message) =>
        FromString(HttpStatusCode.InternalServerError, message);

    public static IResponse NotImplemented(string message) =>
        FromString(HttpStatusCode.NotImplemented, message);

    public static IResponse BadGateway(string message) =>
        FromString(HttpStatusCode.BadGateway, message);

    public static IResponse ServiceUnavailable(string message) =>
        FromString(HttpStatusCode.ServiceUnavailable, message);

    public static IResponse GatewayTimeout(string message) =>
        FromString(HttpStatusCode.GatewayTimeout, message);

    public static IResponse HttpVersionNotSupported(string message) =>
        FromString(HttpStatusCode.HttpVersionNotSupported, message);

    public static IResponse Continue<T>(T value) => FromValue(HttpStatusCode.Continue, value);

    public static IResponse SwitchingProtocols<T>(T value) =>
        FromValue(HttpStatusCode.SwitchingProtocols, value);

    public static IResponse Ok<T>(T value) => FromValue(HttpStatusCode.OK, value);

    public static IResponse Created<T>(T value) => FromValue(HttpStatusCode.Created, value);

    public static IResponse Accepted<T>(T value) => FromValue(HttpStatusCode.Accepted, value);

    public static IResponse NonAuthoritativeInformation<T>(T value) =>
        FromValue(HttpStatusCode.NonAuthoritativeInformation, value);

    public static IResponse NoContent<T>(T value) => FromValue(HttpStatusCode.NoContent, value);

    public static IResponse ResetContent<T>(T value) =>
        FromValue(HttpStatusCode.ResetContent, value);

    public static IResponse PartialContent<T>(T value) =>
        FromValue(HttpStatusCode.PartialContent, value);

    public static IResponse MultipleChoices<T>(T value) =>
        FromValue(HttpStatusCode.MultipleChoices, value);

    public static IResponse Ambiguous<T>(T value) => FromValue(HttpStatusCode.Ambiguous, value);

    public static IResponse MovedPermanently<T>(T value) =>
        FromValue(HttpStatusCode.MovedPermanently, value);

    public static IResponse Moved<T>(T value) => FromValue(HttpStatusCode.Moved, value);

    public static IResponse Found<T>(T value) => FromValue(HttpStatusCode.Found, value);

    public static IResponse Redirect<T>(T value) => FromValue(HttpStatusCode.Redirect, value);

    public static IResponse SeeOther<T>(T value) => FromValue(HttpStatusCode.SeeOther, value);

    public static IResponse RedirectMethod<T>(T value) =>
        FromValue(HttpStatusCode.RedirectMethod, value);

    public static IResponse NotModified<T>(T value) => FromValue(HttpStatusCode.NotModified, value);

    public static IResponse UseProxy<T>(T value) => FromValue(HttpStatusCode.UseProxy, value);

    public static IResponse Unused<T>(T value) => FromValue(HttpStatusCode.Unused, value);

    public static IResponse TemporaryRedirect<T>(T value) =>
        FromValue(HttpStatusCode.TemporaryRedirect, value);

    public static IResponse RedirectKeepVerb<T>(T value) =>
        FromValue(HttpStatusCode.RedirectKeepVerb, value);

    public static IResponse BadRequest<T>(T value) => FromValue(HttpStatusCode.BadRequest, value);

    public static IResponse Unauthorized<T>(T value) =>
        FromValue(HttpStatusCode.Unauthorized, value);

    public static IResponse PaymentRequired<T>(T value) =>
        FromValue(HttpStatusCode.PaymentRequired, value);

    public static IResponse Forbidden<T>(T value) => FromValue(HttpStatusCode.Forbidden, value);

    public static IResponse NotFound<T>(T value) => FromValue(HttpStatusCode.NotFound, value);

    public static IResponse MethodNotAllowed<T>(T value) =>
        FromValue(HttpStatusCode.MethodNotAllowed, value);

    public static IResponse NotAcceptable<T>(T value) =>
        FromValue(HttpStatusCode.NotAcceptable, value);

    public static IResponse ProxyAuthenticationRequired<T>(T value) =>
        FromValue(HttpStatusCode.ProxyAuthenticationRequired, value);

    public static IResponse RequestTimeout<T>(T value) =>
        FromValue(HttpStatusCode.RequestTimeout, value);

    public static IResponse Conflict<T>(T value) => FromValue(HttpStatusCode.Conflict, value);

    public static IResponse Gone<T>(T value) => FromValue(HttpStatusCode.Gone, value);

    public static IResponse LengthRequired<T>(T value) =>
        FromValue(HttpStatusCode.LengthRequired, value);

    public static IResponse PreconditionFailed<T>(T value) =>
        FromValue(HttpStatusCode.PreconditionFailed, value);

    public static IResponse RequestEntityTooLarge<T>(T value) =>
        FromValue(HttpStatusCode.RequestEntityTooLarge, value);

    public static IResponse RequestUriTooLong<T>(T value) =>
        FromValue(HttpStatusCode.RequestUriTooLong, value);

    public static IResponse UnsupportedMediaType<T>(T value) =>
        FromValue(HttpStatusCode.UnsupportedMediaType, value);

    public static IResponse RequestedRangeNotSatisfiable<T>(T value) =>
        FromValue(HttpStatusCode.RequestedRangeNotSatisfiable, value);

    public static IResponse ExpectationFailed<T>(T value) =>
        FromValue(HttpStatusCode.ExpectationFailed, value);

    public static IResponse UpgradeRequired<T>(T value) =>
        FromValue(HttpStatusCode.UpgradeRequired, value);

    public static IResponse InternalServerError<T>(T value) =>
        FromValue(HttpStatusCode.InternalServerError, value);

    public static IResponse NotImplemented<T>(T value) =>
        FromValue(HttpStatusCode.NotImplemented, value);

    public static IResponse BadGateway<T>(T value) => FromValue(HttpStatusCode.BadGateway, value);

    public static IResponse ServiceUnavailable<T>(T value) =>
        FromValue(HttpStatusCode.ServiceUnavailable, value);

    public static IResponse GatewayTimeout<T>(T value) =>
        FromValue(HttpStatusCode.GatewayTimeout, value);

    public static IResponse HttpVersionNotSupported<T>(T value) =>
        FromValue(HttpStatusCode.HttpVersionNotSupported, value);

    private static CoroutineResponse PlainTextCoroutine(
        HttpStatusCode httpStatusCode,
        IEnumerator coroutine
    )
    {
        return new CoroutineResponse(httpStatusCode, "text/plain", coroutine);
    }

    public static IResponse Continue(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Continue, coroutine);

    public static IResponse SwitchingProtocols(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.SwitchingProtocols, coroutine);

    public static IResponse Ok(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.OK, coroutine);

    public static IResponse Created(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Created, coroutine);

    public static IResponse Accepted(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Accepted, coroutine);

    public static IResponse NonAuthoritativeInformation(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NonAuthoritativeInformation, coroutine);

    public static IResponse NoContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NoContent, coroutine);

    public static IResponse ResetContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ResetContent, coroutine);

    public static IResponse PartialContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PartialContent, coroutine);

    public static IResponse MultipleChoices(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MultipleChoices, coroutine);

    public static IResponse Ambiguous(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Ambiguous, coroutine);

    public static IResponse MovedPermanently(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MovedPermanently, coroutine);

    public static IResponse Moved(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Moved, coroutine);

    public static IResponse Found(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Found, coroutine);

    public static IResponse Redirect(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Redirect, coroutine);

    public static IResponse SeeOther(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.SeeOther, coroutine);

    public static IResponse RedirectMethod(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RedirectMethod, coroutine);

    public static IResponse NotModified(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotModified, coroutine);

    public static IResponse UseProxy(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UseProxy, coroutine);

    public static IResponse Unused(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Unused, coroutine);

    public static IResponse TemporaryRedirect(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.TemporaryRedirect, coroutine);

    public static IResponse RedirectKeepVerb(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RedirectKeepVerb, coroutine);

    public static IResponse BadRequest(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.BadRequest, coroutine);

    public static IResponse Unauthorized(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Unauthorized, coroutine);

    public static IResponse PaymentRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PaymentRequired, coroutine);

    public static IResponse Forbidden(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Forbidden, coroutine);

    public static IResponse NotFound(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotFound, coroutine);

    public static IResponse MethodNotAllowed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MethodNotAllowed, coroutine);

    public static IResponse NotAcceptable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotAcceptable, coroutine);

    public static IResponse ProxyAuthenticationRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ProxyAuthenticationRequired, coroutine);

    public static IResponse RequestTimeout(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestTimeout, coroutine);

    public static IResponse Conflict(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Conflict, coroutine);

    public static IResponse Gone(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Gone, coroutine);

    public static IResponse LengthRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.LengthRequired, coroutine);

    public static IResponse PreconditionFailed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PreconditionFailed, coroutine);

    public static IResponse RequestEntityTooLarge(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestEntityTooLarge, coroutine);

    public static IResponse RequestUriTooLong(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestUriTooLong, coroutine);

    public static IResponse UnsupportedMediaType(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UnsupportedMediaType, coroutine);

    public static IResponse RequestedRangeNotSatisfiable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestedRangeNotSatisfiable, coroutine);

    public static IResponse ExpectationFailed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ExpectationFailed, coroutine);

    public static IResponse UpgradeRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UpgradeRequired, coroutine);

    public static IResponse InternalServerError(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.InternalServerError, coroutine);

    public static IResponse NotImplemented(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotImplemented, coroutine);

    public static IResponse BadGateway(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.BadGateway, coroutine);

    public static IResponse ServiceUnavailable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ServiceUnavailable, coroutine);

    public static IResponse GatewayTimeout(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.GatewayTimeout, coroutine);

    public static IResponse HttpVersionNotSupported(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.HttpVersionNotSupported, coroutine);
}

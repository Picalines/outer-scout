﻿using System.Collections;
using System.Net;

namespace SceneRecorder.WebApi.Http;

public static class ResponseFabric
{
    public static SyncResponse Continue() => SyncResponse.Empty(HttpStatusCode.Continue);

    public static SyncResponse SwitchingProtocols() =>
        SyncResponse.Empty(HttpStatusCode.SwitchingProtocols);

    public static SyncResponse Ok() => SyncResponse.Empty(HttpStatusCode.OK);

    public static SyncResponse Created() => SyncResponse.Empty(HttpStatusCode.Created);

    public static SyncResponse Accepted() => SyncResponse.Empty(HttpStatusCode.Accepted);

    public static SyncResponse NonAuthoritativeInformation() =>
        SyncResponse.Empty(HttpStatusCode.NonAuthoritativeInformation);

    public static SyncResponse NoContent() => SyncResponse.Empty(HttpStatusCode.NoContent);

    public static SyncResponse ResetContent() => SyncResponse.Empty(HttpStatusCode.ResetContent);

    public static SyncResponse PartialContent() =>
        SyncResponse.Empty(HttpStatusCode.PartialContent);

    public static SyncResponse MultipleChoices() =>
        SyncResponse.Empty(HttpStatusCode.MultipleChoices);

    public static SyncResponse Ambiguous() => SyncResponse.Empty(HttpStatusCode.Ambiguous);

    public static SyncResponse MovedPermanently() =>
        SyncResponse.Empty(HttpStatusCode.MovedPermanently);

    public static SyncResponse Moved() => SyncResponse.Empty(HttpStatusCode.Moved);

    public static SyncResponse Found() => SyncResponse.Empty(HttpStatusCode.Found);

    public static SyncResponse Redirect() => SyncResponse.Empty(HttpStatusCode.Redirect);

    public static SyncResponse SeeOther() => SyncResponse.Empty(HttpStatusCode.SeeOther);

    public static SyncResponse RedirectMethod() =>
        SyncResponse.Empty(HttpStatusCode.RedirectMethod);

    public static SyncResponse NotModified() => SyncResponse.Empty(HttpStatusCode.NotModified);

    public static SyncResponse UseProxy() => SyncResponse.Empty(HttpStatusCode.UseProxy);

    public static SyncResponse Unused() => SyncResponse.Empty(HttpStatusCode.Unused);

    public static SyncResponse TemporaryRedirect() =>
        SyncResponse.Empty(HttpStatusCode.TemporaryRedirect);

    public static SyncResponse RedirectKeepVerb() =>
        SyncResponse.Empty(HttpStatusCode.RedirectKeepVerb);

    public static SyncResponse BadRequest() => SyncResponse.Empty(HttpStatusCode.BadRequest);

    public static SyncResponse Unauthorized() => SyncResponse.Empty(HttpStatusCode.Unauthorized);

    public static SyncResponse PaymentRequired() =>
        SyncResponse.Empty(HttpStatusCode.PaymentRequired);

    public static SyncResponse Forbidden() => SyncResponse.Empty(HttpStatusCode.Forbidden);

    public static SyncResponse NotFound() => SyncResponse.Empty(HttpStatusCode.NotFound);

    public static SyncResponse MethodNotAllowed() =>
        SyncResponse.Empty(HttpStatusCode.MethodNotAllowed);

    public static SyncResponse NotAcceptable() => SyncResponse.Empty(HttpStatusCode.NotAcceptable);

    public static SyncResponse ProxyAuthenticationRequired() =>
        SyncResponse.Empty(HttpStatusCode.ProxyAuthenticationRequired);

    public static SyncResponse RequestTimeout() =>
        SyncResponse.Empty(HttpStatusCode.RequestTimeout);

    public static SyncResponse Conflict() => SyncResponse.Empty(HttpStatusCode.Conflict);

    public static SyncResponse Gone() => SyncResponse.Empty(HttpStatusCode.Gone);

    public static SyncResponse LengthRequired() =>
        SyncResponse.Empty(HttpStatusCode.LengthRequired);

    public static SyncResponse PreconditionFailed() =>
        SyncResponse.Empty(HttpStatusCode.PreconditionFailed);

    public static SyncResponse RequestEntityTooLarge() =>
        SyncResponse.Empty(HttpStatusCode.RequestEntityTooLarge);

    public static SyncResponse RequestUriTooLong() =>
        SyncResponse.Empty(HttpStatusCode.RequestUriTooLong);

    public static SyncResponse UnsupportedMediaType() =>
        SyncResponse.Empty(HttpStatusCode.UnsupportedMediaType);

    public static SyncResponse RequestedRangeNotSatisfiable() =>
        SyncResponse.Empty(HttpStatusCode.RequestedRangeNotSatisfiable);

    public static SyncResponse ExpectationFailed() =>
        SyncResponse.Empty(HttpStatusCode.ExpectationFailed);

    public static SyncResponse UpgradeRequired() =>
        SyncResponse.Empty(HttpStatusCode.UpgradeRequired);

    public static SyncResponse InternalServerError() =>
        SyncResponse.Empty(HttpStatusCode.InternalServerError);

    public static SyncResponse NotImplemented() =>
        SyncResponse.Empty(HttpStatusCode.NotImplemented);

    public static SyncResponse BadGateway() => SyncResponse.Empty(HttpStatusCode.BadGateway);

    public static SyncResponse ServiceUnavailable() =>
        SyncResponse.Empty(HttpStatusCode.ServiceUnavailable);

    public static SyncResponse GatewayTimeout() =>
        SyncResponse.Empty(HttpStatusCode.GatewayTimeout);

    public static SyncResponse HttpVersionNotSupported() =>
        SyncResponse.Empty(HttpStatusCode.HttpVersionNotSupported);

    public static SyncResponse Continue(string message) =>
        SyncResponse.FromString(HttpStatusCode.Continue, message);

    public static SyncResponse SwitchingProtocols(string message) =>
        SyncResponse.FromString(HttpStatusCode.SwitchingProtocols, message);

    public static SyncResponse Ok(string message) =>
        SyncResponse.FromString(HttpStatusCode.OK, message);

    public static SyncResponse Created(string message) =>
        SyncResponse.FromString(HttpStatusCode.Created, message);

    public static SyncResponse Accepted(string message) =>
        SyncResponse.FromString(HttpStatusCode.Accepted, message);

    public static SyncResponse NonAuthoritativeInformation(string message) =>
        SyncResponse.FromString(HttpStatusCode.NonAuthoritativeInformation, message);

    public static SyncResponse NoContent(string message) =>
        SyncResponse.FromString(HttpStatusCode.NoContent, message);

    public static SyncResponse ResetContent(string message) =>
        SyncResponse.FromString(HttpStatusCode.ResetContent, message);

    public static SyncResponse PartialContent(string message) =>
        SyncResponse.FromString(HttpStatusCode.PartialContent, message);

    public static SyncResponse MultipleChoices(string message) =>
        SyncResponse.FromString(HttpStatusCode.MultipleChoices, message);

    public static SyncResponse Ambiguous(string message) =>
        SyncResponse.FromString(HttpStatusCode.Ambiguous, message);

    public static SyncResponse MovedPermanently(string message) =>
        SyncResponse.FromString(HttpStatusCode.MovedPermanently, message);

    public static SyncResponse Moved(string message) =>
        SyncResponse.FromString(HttpStatusCode.Moved, message);

    public static SyncResponse Found(string message) =>
        SyncResponse.FromString(HttpStatusCode.Found, message);

    public static SyncResponse Redirect(string message) =>
        SyncResponse.FromString(HttpStatusCode.Redirect, message);

    public static SyncResponse SeeOther(string message) =>
        SyncResponse.FromString(HttpStatusCode.SeeOther, message);

    public static SyncResponse RedirectMethod(string message) =>
        SyncResponse.FromString(HttpStatusCode.RedirectMethod, message);

    public static SyncResponse NotModified(string message) =>
        SyncResponse.FromString(HttpStatusCode.NotModified, message);

    public static SyncResponse UseProxy(string message) =>
        SyncResponse.FromString(HttpStatusCode.UseProxy, message);

    public static SyncResponse Unused(string message) =>
        SyncResponse.FromString(HttpStatusCode.Unused, message);

    public static SyncResponse TemporaryRedirect(string message) =>
        SyncResponse.FromString(HttpStatusCode.TemporaryRedirect, message);

    public static SyncResponse RedirectKeepVerb(string message) =>
        SyncResponse.FromString(HttpStatusCode.RedirectKeepVerb, message);

    public static SyncResponse BadRequest(string message) =>
        SyncResponse.FromString(HttpStatusCode.BadRequest, message);

    public static SyncResponse Unauthorized(string message) =>
        SyncResponse.FromString(HttpStatusCode.Unauthorized, message);

    public static SyncResponse PaymentRequired(string message) =>
        SyncResponse.FromString(HttpStatusCode.PaymentRequired, message);

    public static SyncResponse Forbidden(string message) =>
        SyncResponse.FromString(HttpStatusCode.Forbidden, message);

    public static SyncResponse NotFound(string message) =>
        SyncResponse.FromString(HttpStatusCode.NotFound, message);

    public static SyncResponse MethodNotAllowed(string message) =>
        SyncResponse.FromString(HttpStatusCode.MethodNotAllowed, message);

    public static SyncResponse NotAcceptable(string message) =>
        SyncResponse.FromString(HttpStatusCode.NotAcceptable, message);

    public static SyncResponse ProxyAuthenticationRequired(string message) =>
        SyncResponse.FromString(HttpStatusCode.ProxyAuthenticationRequired, message);

    public static SyncResponse RequestTimeout(string message) =>
        SyncResponse.FromString(HttpStatusCode.RequestTimeout, message);

    public static SyncResponse Conflict(string message) =>
        SyncResponse.FromString(HttpStatusCode.Conflict, message);

    public static SyncResponse Gone(string message) =>
        SyncResponse.FromString(HttpStatusCode.Gone, message);

    public static SyncResponse LengthRequired(string message) =>
        SyncResponse.FromString(HttpStatusCode.LengthRequired, message);

    public static SyncResponse PreconditionFailed(string message) =>
        SyncResponse.FromString(HttpStatusCode.PreconditionFailed, message);

    public static SyncResponse RequestEntityTooLarge(string message) =>
        SyncResponse.FromString(HttpStatusCode.RequestEntityTooLarge, message);

    public static SyncResponse RequestUriTooLong(string message) =>
        SyncResponse.FromString(HttpStatusCode.RequestUriTooLong, message);

    public static SyncResponse UnsupportedMediaType(string message) =>
        SyncResponse.FromString(HttpStatusCode.UnsupportedMediaType, message);

    public static SyncResponse RequestedRangeNotSatisfiable(string message) =>
        SyncResponse.FromString(HttpStatusCode.RequestedRangeNotSatisfiable, message);

    public static SyncResponse ExpectationFailed(string message) =>
        SyncResponse.FromString(HttpStatusCode.ExpectationFailed, message);

    public static SyncResponse UpgradeRequired(string message) =>
        SyncResponse.FromString(HttpStatusCode.UpgradeRequired, message);

    public static SyncResponse InternalServerError(string message) =>
        SyncResponse.FromString(HttpStatusCode.InternalServerError, message);

    public static SyncResponse NotImplemented(string message) =>
        SyncResponse.FromString(HttpStatusCode.NotImplemented, message);

    public static SyncResponse BadGateway(string message) =>
        SyncResponse.FromString(HttpStatusCode.BadGateway, message);

    public static SyncResponse ServiceUnavailable(string message) =>
        SyncResponse.FromString(HttpStatusCode.ServiceUnavailable, message);

    public static SyncResponse GatewayTimeout(string message) =>
        SyncResponse.FromString(HttpStatusCode.GatewayTimeout, message);

    public static SyncResponse HttpVersionNotSupported(string message) =>
        SyncResponse.FromString(HttpStatusCode.HttpVersionNotSupported, message);

    private static SyncResponse FromValue<T>(HttpStatusCode httpStatusCode, T value)
    {
        return value switch
        {
            string stringValue => SyncResponse.FromString(httpStatusCode, stringValue),
            _ => SyncResponse.FromJson(httpStatusCode, value),
        };
    }

    public static SyncResponse Continue<T>(T value) => FromValue(HttpStatusCode.Continue, value);

    public static SyncResponse SwitchingProtocols<T>(T value) =>
        FromValue(HttpStatusCode.SwitchingProtocols, value);

    public static SyncResponse Ok<T>(T value) => FromValue(HttpStatusCode.OK, value);

    public static SyncResponse Created<T>(T value) => FromValue(HttpStatusCode.Created, value);

    public static SyncResponse Accepted<T>(T value) => FromValue(HttpStatusCode.Accepted, value);

    public static SyncResponse NonAuthoritativeInformation<T>(T value) =>
        FromValue(HttpStatusCode.NonAuthoritativeInformation, value);

    public static SyncResponse NoContent<T>(T value) => FromValue(HttpStatusCode.NoContent, value);

    public static SyncResponse ResetContent<T>(T value) =>
        FromValue(HttpStatusCode.ResetContent, value);

    public static SyncResponse PartialContent<T>(T value) =>
        FromValue(HttpStatusCode.PartialContent, value);

    public static SyncResponse MultipleChoices<T>(T value) =>
        FromValue(HttpStatusCode.MultipleChoices, value);

    public static SyncResponse Ambiguous<T>(T value) => FromValue(HttpStatusCode.Ambiguous, value);

    public static SyncResponse MovedPermanently<T>(T value) =>
        FromValue(HttpStatusCode.MovedPermanently, value);

    public static SyncResponse Moved<T>(T value) => FromValue(HttpStatusCode.Moved, value);

    public static SyncResponse Found<T>(T value) => FromValue(HttpStatusCode.Found, value);

    public static SyncResponse Redirect<T>(T value) => FromValue(HttpStatusCode.Redirect, value);

    public static SyncResponse SeeOther<T>(T value) => FromValue(HttpStatusCode.SeeOther, value);

    public static SyncResponse RedirectMethod<T>(T value) =>
        FromValue(HttpStatusCode.RedirectMethod, value);

    public static SyncResponse NotModified<T>(T value) =>
        FromValue(HttpStatusCode.NotModified, value);

    public static SyncResponse UseProxy<T>(T value) => FromValue(HttpStatusCode.UseProxy, value);

    public static SyncResponse Unused<T>(T value) => FromValue(HttpStatusCode.Unused, value);

    public static SyncResponse TemporaryRedirect<T>(T value) =>
        FromValue(HttpStatusCode.TemporaryRedirect, value);

    public static SyncResponse RedirectKeepVerb<T>(T value) =>
        FromValue(HttpStatusCode.RedirectKeepVerb, value);

    public static SyncResponse BadRequest<T>(T value) =>
        FromValue(HttpStatusCode.BadRequest, value);

    public static SyncResponse Unauthorized<T>(T value) =>
        FromValue(HttpStatusCode.Unauthorized, value);

    public static SyncResponse PaymentRequired<T>(T value) =>
        FromValue(HttpStatusCode.PaymentRequired, value);

    public static SyncResponse Forbidden<T>(T value) => FromValue(HttpStatusCode.Forbidden, value);

    public static SyncResponse NotFound<T>(T value) => FromValue(HttpStatusCode.NotFound, value);

    public static SyncResponse MethodNotAllowed<T>(T value) =>
        FromValue(HttpStatusCode.MethodNotAllowed, value);

    public static SyncResponse NotAcceptable<T>(T value) =>
        FromValue(HttpStatusCode.NotAcceptable, value);

    public static SyncResponse ProxyAuthenticationRequired<T>(T value) =>
        FromValue(HttpStatusCode.ProxyAuthenticationRequired, value);

    public static SyncResponse RequestTimeout<T>(T value) =>
        FromValue(HttpStatusCode.RequestTimeout, value);

    public static SyncResponse Conflict<T>(T value) => FromValue(HttpStatusCode.Conflict, value);

    public static SyncResponse Gone<T>(T value) => FromValue(HttpStatusCode.Gone, value);

    public static SyncResponse LengthRequired<T>(T value) =>
        FromValue(HttpStatusCode.LengthRequired, value);

    public static SyncResponse PreconditionFailed<T>(T value) =>
        FromValue(HttpStatusCode.PreconditionFailed, value);

    public static SyncResponse RequestEntityTooLarge<T>(T value) =>
        FromValue(HttpStatusCode.RequestEntityTooLarge, value);

    public static SyncResponse RequestUriTooLong<T>(T value) =>
        FromValue(HttpStatusCode.RequestUriTooLong, value);

    public static SyncResponse UnsupportedMediaType<T>(T value) =>
        FromValue(HttpStatusCode.UnsupportedMediaType, value);

    public static SyncResponse RequestedRangeNotSatisfiable<T>(T value) =>
        FromValue(HttpStatusCode.RequestedRangeNotSatisfiable, value);

    public static SyncResponse ExpectationFailed<T>(T value) =>
        FromValue(HttpStatusCode.ExpectationFailed, value);

    public static SyncResponse UpgradeRequired<T>(T value) =>
        FromValue(HttpStatusCode.UpgradeRequired, value);

    public static SyncResponse InternalServerError<T>(T value) =>
        FromValue(HttpStatusCode.InternalServerError, value);

    public static SyncResponse NotImplemented<T>(T value) =>
        FromValue(HttpStatusCode.NotImplemented, value);

    public static SyncResponse BadGateway<T>(T value) =>
        FromValue(HttpStatusCode.BadGateway, value);

    public static SyncResponse ServiceUnavailable<T>(T value) =>
        FromValue(HttpStatusCode.ServiceUnavailable, value);

    public static SyncResponse GatewayTimeout<T>(T value) =>
        FromValue(HttpStatusCode.GatewayTimeout, value);

    public static SyncResponse HttpVersionNotSupported<T>(T value) =>
        FromValue(HttpStatusCode.HttpVersionNotSupported, value);

    private static CoroutineResponse PlainTextCoroutine(
        HttpStatusCode httpStatusCode,
        IEnumerator coroutine
    )
    {
        return new CoroutineResponse(httpStatusCode, "text/plain", coroutine);
    }

    public static CoroutineResponse Continue(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Continue, coroutine);

    public static CoroutineResponse SwitchingProtocols(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.SwitchingProtocols, coroutine);

    public static CoroutineResponse Ok(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.OK, coroutine);

    public static CoroutineResponse Created(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Created, coroutine);

    public static CoroutineResponse Accepted(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Accepted, coroutine);

    public static CoroutineResponse NonAuthoritativeInformation(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NonAuthoritativeInformation, coroutine);

    public static CoroutineResponse NoContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NoContent, coroutine);

    public static CoroutineResponse ResetContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ResetContent, coroutine);

    public static CoroutineResponse PartialContent(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PartialContent, coroutine);

    public static CoroutineResponse MultipleChoices(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MultipleChoices, coroutine);

    public static CoroutineResponse Ambiguous(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Ambiguous, coroutine);

    public static CoroutineResponse MovedPermanently(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MovedPermanently, coroutine);

    public static CoroutineResponse Moved(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Moved, coroutine);

    public static CoroutineResponse Found(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Found, coroutine);

    public static CoroutineResponse Redirect(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Redirect, coroutine);

    public static CoroutineResponse SeeOther(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.SeeOther, coroutine);

    public static CoroutineResponse RedirectMethod(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RedirectMethod, coroutine);

    public static CoroutineResponse NotModified(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotModified, coroutine);

    public static CoroutineResponse UseProxy(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UseProxy, coroutine);

    public static CoroutineResponse Unused(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Unused, coroutine);

    public static CoroutineResponse TemporaryRedirect(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.TemporaryRedirect, coroutine);

    public static CoroutineResponse RedirectKeepVerb(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RedirectKeepVerb, coroutine);

    public static CoroutineResponse BadRequest(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.BadRequest, coroutine);

    public static CoroutineResponse Unauthorized(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Unauthorized, coroutine);

    public static CoroutineResponse PaymentRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PaymentRequired, coroutine);

    public static CoroutineResponse Forbidden(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Forbidden, coroutine);

    public static CoroutineResponse NotFound(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotFound, coroutine);

    public static CoroutineResponse MethodNotAllowed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.MethodNotAllowed, coroutine);

    public static CoroutineResponse NotAcceptable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotAcceptable, coroutine);

    public static CoroutineResponse ProxyAuthenticationRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ProxyAuthenticationRequired, coroutine);

    public static CoroutineResponse RequestTimeout(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestTimeout, coroutine);

    public static CoroutineResponse Conflict(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Conflict, coroutine);

    public static CoroutineResponse Gone(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.Gone, coroutine);

    public static CoroutineResponse LengthRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.LengthRequired, coroutine);

    public static CoroutineResponse PreconditionFailed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.PreconditionFailed, coroutine);

    public static CoroutineResponse RequestEntityTooLarge(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestEntityTooLarge, coroutine);

    public static CoroutineResponse RequestUriTooLong(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestUriTooLong, coroutine);

    public static CoroutineResponse UnsupportedMediaType(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UnsupportedMediaType, coroutine);

    public static CoroutineResponse RequestedRangeNotSatisfiable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.RequestedRangeNotSatisfiable, coroutine);

    public static CoroutineResponse ExpectationFailed(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ExpectationFailed, coroutine);

    public static CoroutineResponse UpgradeRequired(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.UpgradeRequired, coroutine);

    public static CoroutineResponse InternalServerError(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.InternalServerError, coroutine);

    public static CoroutineResponse NotImplemented(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.NotImplemented, coroutine);

    public static CoroutineResponse BadGateway(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.BadGateway, coroutine);

    public static CoroutineResponse ServiceUnavailable(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.ServiceUnavailable, coroutine);

    public static CoroutineResponse GatewayTimeout(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.GatewayTimeout, coroutine);

    public static CoroutineResponse HttpVersionNotSupported(IEnumerator coroutine) =>
        PlainTextCoroutine(HttpStatusCode.HttpVersionNotSupported, coroutine);
}

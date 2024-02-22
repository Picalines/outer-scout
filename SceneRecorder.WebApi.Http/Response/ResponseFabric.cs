using System.Collections;
using System.Net;

namespace SceneRecorder.WebApi.Http.Response;

public static class ResponseFabric
{
    private static EmptyResponse Empty(HttpStatusCode statusCode)
    {
        return EmptyResponse.WithStatusCode(statusCode);
    }

    private static StringResponse FromString(HttpStatusCode statusCode, string @string)
    {
        return new StringResponse(statusCode, @string);
    }

    private static JsonResponse FromJson<T>(HttpStatusCode httpStatusCode, T value)
    {
        return new JsonResponse(httpStatusCode, value);
    }

    private static CoroutineResponse FromCoroutine(
        HttpStatusCode httpStatusCode,
        IEnumerator coroutine
    )
    {
        return new CoroutineResponse(httpStatusCode, "text/plain", coroutine);
    }

    // csharpier-ignore-start

    public static EmptyResponse Continue() => Empty(HttpStatusCode.Continue);

    public static EmptyResponse SwitchingProtocols() => Empty(HttpStatusCode.SwitchingProtocols);

    public static EmptyResponse Ok() => Empty(HttpStatusCode.OK);

    public static EmptyResponse Created() => Empty(HttpStatusCode.Created);

    public static EmptyResponse Accepted() => Empty(HttpStatusCode.Accepted);

    public static EmptyResponse NonAuthoritativeInformation() => Empty(HttpStatusCode.NonAuthoritativeInformation);

    public static EmptyResponse NoContent() => Empty(HttpStatusCode.NoContent);

    public static EmptyResponse ResetContent() => Empty(HttpStatusCode.ResetContent);

    public static EmptyResponse PartialContent() => Empty(HttpStatusCode.PartialContent);

    public static EmptyResponse MultipleChoices() => Empty(HttpStatusCode.MultipleChoices);

    public static EmptyResponse Ambiguous() => Empty(HttpStatusCode.Ambiguous);

    public static EmptyResponse MovedPermanently() => Empty(HttpStatusCode.MovedPermanently);

    public static EmptyResponse Moved() => Empty(HttpStatusCode.Moved);

    public static EmptyResponse Found() => Empty(HttpStatusCode.Found);

    public static EmptyResponse Redirect() => Empty(HttpStatusCode.Redirect);

    public static EmptyResponse SeeOther() => Empty(HttpStatusCode.SeeOther);

    public static EmptyResponse RedirectMethod() => Empty(HttpStatusCode.RedirectMethod);

    public static EmptyResponse NotModified() => Empty(HttpStatusCode.NotModified);

    public static EmptyResponse UseProxy() => Empty(HttpStatusCode.UseProxy);

    public static EmptyResponse Unused() => Empty(HttpStatusCode.Unused);

    public static EmptyResponse TemporaryRedirect() => Empty(HttpStatusCode.TemporaryRedirect);

    public static EmptyResponse RedirectKeepVerb() => Empty(HttpStatusCode.RedirectKeepVerb);

    public static EmptyResponse BadRequest() => Empty(HttpStatusCode.BadRequest);

    public static EmptyResponse Unauthorized() => Empty(HttpStatusCode.Unauthorized);

    public static EmptyResponse PaymentRequired() => Empty(HttpStatusCode.PaymentRequired);

    public static EmptyResponse Forbidden() => Empty(HttpStatusCode.Forbidden);

    public static EmptyResponse NotFound() => Empty(HttpStatusCode.NotFound);

    public static EmptyResponse MethodNotAllowed() => Empty(HttpStatusCode.MethodNotAllowed);

    public static EmptyResponse NotAcceptable() => Empty(HttpStatusCode.NotAcceptable);

    public static EmptyResponse ProxyAuthenticationRequired() => Empty(HttpStatusCode.ProxyAuthenticationRequired);

    public static EmptyResponse RequestTimeout() => Empty(HttpStatusCode.RequestTimeout);

    public static EmptyResponse Conflict() => Empty(HttpStatusCode.Conflict);

    public static EmptyResponse Gone() => Empty(HttpStatusCode.Gone);

    public static EmptyResponse LengthRequired() => Empty(HttpStatusCode.LengthRequired);

    public static EmptyResponse PreconditionFailed() => Empty(HttpStatusCode.PreconditionFailed);

    public static EmptyResponse RequestEntityTooLarge() => Empty(HttpStatusCode.RequestEntityTooLarge);

    public static EmptyResponse RequestUriTooLong() => Empty(HttpStatusCode.RequestUriTooLong);

    public static EmptyResponse UnsupportedMediaType() => Empty(HttpStatusCode.UnsupportedMediaType);

    public static EmptyResponse RequestedRangeNotSatisfiable() => Empty(HttpStatusCode.RequestedRangeNotSatisfiable);

    public static EmptyResponse ExpectationFailed() => Empty(HttpStatusCode.ExpectationFailed);

    public static EmptyResponse UpgradeRequired() => Empty(HttpStatusCode.UpgradeRequired);

    public static EmptyResponse InternalServerError() => Empty(HttpStatusCode.InternalServerError);

    public static EmptyResponse NotImplemented() => Empty(HttpStatusCode.NotImplemented);

    public static EmptyResponse BadGateway() => Empty(HttpStatusCode.BadGateway);

    public static EmptyResponse ServiceUnavailable() => Empty(HttpStatusCode.ServiceUnavailable);

    public static EmptyResponse GatewayTimeout() => Empty(HttpStatusCode.GatewayTimeout);

    public static EmptyResponse HttpVersionNotSupported() => Empty(HttpStatusCode.HttpVersionNotSupported);

    public static StringResponse Continue(string @string) => FromString(HttpStatusCode.Continue, @string);

    public static StringResponse SwitchingProtocols(string @string) => FromString(HttpStatusCode.SwitchingProtocols, @string);

    public static StringResponse Ok(string @string) => FromString(HttpStatusCode.OK, @string);

    public static StringResponse Created(string @string) => FromString(HttpStatusCode.Created, @string);

    public static StringResponse Accepted(string @string) => FromString(HttpStatusCode.Accepted, @string);

    public static StringResponse NonAuthoritativeInformation(string @string) => FromString(HttpStatusCode.NonAuthoritativeInformation, @string);

    public static StringResponse NoContent(string @string) => FromString(HttpStatusCode.NoContent, @string);

    public static StringResponse ResetContent(string @string) => FromString(HttpStatusCode.ResetContent, @string);

    public static StringResponse PartialContent(string @string) => FromString(HttpStatusCode.PartialContent, @string);

    public static StringResponse MultipleChoices(string @string) => FromString(HttpStatusCode.MultipleChoices, @string);

    public static StringResponse Ambiguous(string @string) => FromString(HttpStatusCode.Ambiguous, @string);

    public static StringResponse MovedPermanently(string @string) => FromString(HttpStatusCode.MovedPermanently, @string);

    public static StringResponse Moved(string @string) => FromString(HttpStatusCode.Moved, @string);

    public static StringResponse Found(string @string) => FromString(HttpStatusCode.Found, @string);

    public static StringResponse Redirect(string @string) => FromString(HttpStatusCode.Redirect, @string);

    public static StringResponse SeeOther(string @string) => FromString(HttpStatusCode.SeeOther, @string);

    public static StringResponse RedirectMethod(string @string) => FromString(HttpStatusCode.RedirectMethod, @string);

    public static StringResponse NotModified(string @string) => FromString(HttpStatusCode.NotModified, @string);

    public static StringResponse UseProxy(string @string) => FromString(HttpStatusCode.UseProxy, @string);

    public static StringResponse Unused(string @string) => FromString(HttpStatusCode.Unused, @string);

    public static StringResponse TemporaryRedirect(string @string) => FromString(HttpStatusCode.TemporaryRedirect, @string);

    public static StringResponse RedirectKeepVerb(string @string) => FromString(HttpStatusCode.RedirectKeepVerb, @string);

    public static StringResponse BadRequest(string @string) => FromString(HttpStatusCode.BadRequest, @string);

    public static StringResponse Unauthorized(string @string) => FromString(HttpStatusCode.Unauthorized, @string);

    public static StringResponse PaymentRequired(string @string) => FromString(HttpStatusCode.PaymentRequired, @string);

    public static StringResponse Forbidden(string @string) => FromString(HttpStatusCode.Forbidden, @string);

    public static StringResponse NotFound(string @string) => FromString(HttpStatusCode.NotFound, @string);

    public static StringResponse MethodNotAllowed(string @string) => FromString(HttpStatusCode.MethodNotAllowed, @string);

    public static StringResponse NotAcceptable(string @string) => FromString(HttpStatusCode.NotAcceptable, @string);

    public static StringResponse ProxyAuthenticationRequired(string @string) => FromString(HttpStatusCode.ProxyAuthenticationRequired, @string);

    public static StringResponse RequestTimeout(string @string) => FromString(HttpStatusCode.RequestTimeout, @string);

    public static StringResponse Conflict(string @string) => FromString(HttpStatusCode.Conflict, @string);

    public static StringResponse Gone(string @string) => FromString(HttpStatusCode.Gone, @string);

    public static StringResponse LengthRequired(string @string) => FromString(HttpStatusCode.LengthRequired, @string);

    public static StringResponse PreconditionFailed(string @string) => FromString(HttpStatusCode.PreconditionFailed, @string);

    public static StringResponse RequestEntityTooLarge(string @string) => FromString(HttpStatusCode.RequestEntityTooLarge, @string);

    public static StringResponse RequestUriTooLong(string @string) => FromString(HttpStatusCode.RequestUriTooLong, @string);

    public static StringResponse UnsupportedMediaType(string @string) => FromString(HttpStatusCode.UnsupportedMediaType, @string);

    public static StringResponse RequestedRangeNotSatisfiable(string @string) => FromString(HttpStatusCode.RequestedRangeNotSatisfiable, @string);

    public static StringResponse ExpectationFailed(string @string) => FromString(HttpStatusCode.ExpectationFailed, @string);

    public static StringResponse UpgradeRequired(string @string) => FromString(HttpStatusCode.UpgradeRequired, @string);

    public static StringResponse InternalServerError(string @string) => FromString(HttpStatusCode.InternalServerError, @string);

    public static StringResponse NotImplemented(string @string) => FromString(HttpStatusCode.NotImplemented, @string);

    public static StringResponse BadGateway(string @string) => FromString(HttpStatusCode.BadGateway, @string);

    public static StringResponse ServiceUnavailable(string @string) => FromString(HttpStatusCode.ServiceUnavailable, @string);

    public static StringResponse GatewayTimeout(string @string) => FromString(HttpStatusCode.GatewayTimeout, @string);

    public static StringResponse HttpVersionNotSupported(string @string) => FromString(HttpStatusCode.HttpVersionNotSupported, @string);

    public static JsonResponse Continue<T>(T value) => FromJson(HttpStatusCode.Continue, value);

    public static JsonResponse SwitchingProtocols<T>(T value) => FromJson(HttpStatusCode.SwitchingProtocols, value);

    public static JsonResponse Ok<T>(T value) => FromJson(HttpStatusCode.OK, value);

    public static JsonResponse Created<T>(T value) => FromJson(HttpStatusCode.Created, value);

    public static JsonResponse Accepted<T>(T value) => FromJson(HttpStatusCode.Accepted, value);

    public static JsonResponse NonAuthoritativeInformation<T>(T value) => FromJson(HttpStatusCode.NonAuthoritativeInformation, value);

    public static JsonResponse NoContent<T>(T value) => FromJson(HttpStatusCode.NoContent, value);

    public static JsonResponse ResetContent<T>(T value) => FromJson(HttpStatusCode.ResetContent, value);

    public static JsonResponse PartialContent<T>(T value) => FromJson(HttpStatusCode.PartialContent, value);

    public static JsonResponse MultipleChoices<T>(T value) => FromJson(HttpStatusCode.MultipleChoices, value);

    public static JsonResponse Ambiguous<T>(T value) => FromJson(HttpStatusCode.Ambiguous, value);

    public static JsonResponse MovedPermanently<T>(T value) => FromJson(HttpStatusCode.MovedPermanently, value);

    public static JsonResponse Moved<T>(T value) => FromJson(HttpStatusCode.Moved, value);

    public static JsonResponse Found<T>(T value) => FromJson(HttpStatusCode.Found, value);

    public static JsonResponse Redirect<T>(T value) => FromJson(HttpStatusCode.Redirect, value);

    public static JsonResponse SeeOther<T>(T value) => FromJson(HttpStatusCode.SeeOther, value);

    public static JsonResponse RedirectMethod<T>(T value) => FromJson(HttpStatusCode.RedirectMethod, value);

    public static JsonResponse NotModified<T>(T value) => FromJson(HttpStatusCode.NotModified, value);

    public static JsonResponse UseProxy<T>(T value) => FromJson(HttpStatusCode.UseProxy, value);

    public static JsonResponse Unused<T>(T value) => FromJson(HttpStatusCode.Unused, value);

    public static JsonResponse TemporaryRedirect<T>(T value) => FromJson(HttpStatusCode.TemporaryRedirect, value);

    public static JsonResponse RedirectKeepVerb<T>(T value) => FromJson(HttpStatusCode.RedirectKeepVerb, value);

    public static JsonResponse BadRequest<T>(T value) => FromJson(HttpStatusCode.BadRequest, value);

    public static JsonResponse Unauthorized<T>(T value) => FromJson(HttpStatusCode.Unauthorized, value);

    public static JsonResponse PaymentRequired<T>(T value) => FromJson(HttpStatusCode.PaymentRequired, value);

    public static JsonResponse Forbidden<T>(T value) => FromJson(HttpStatusCode.Forbidden, value);

    public static JsonResponse NotFound<T>(T value) => FromJson(HttpStatusCode.NotFound, value);

    public static JsonResponse MethodNotAllowed<T>(T value) => FromJson(HttpStatusCode.MethodNotAllowed, value);

    public static JsonResponse NotAcceptable<T>(T value) => FromJson(HttpStatusCode.NotAcceptable, value);

    public static JsonResponse ProxyAuthenticationRequired<T>(T value) => FromJson(HttpStatusCode.ProxyAuthenticationRequired, value);

    public static JsonResponse RequestTimeout<T>(T value) => FromJson(HttpStatusCode.RequestTimeout, value);

    public static JsonResponse Conflict<T>(T value) => FromJson(HttpStatusCode.Conflict, value);

    public static JsonResponse Gone<T>(T value) => FromJson(HttpStatusCode.Gone, value);

    public static JsonResponse LengthRequired<T>(T value) => FromJson(HttpStatusCode.LengthRequired, value);

    public static JsonResponse PreconditionFailed<T>(T value) => FromJson(HttpStatusCode.PreconditionFailed, value);

    public static JsonResponse RequestEntityTooLarge<T>(T value) => FromJson(HttpStatusCode.RequestEntityTooLarge, value);

    public static JsonResponse RequestUriTooLong<T>(T value) => FromJson(HttpStatusCode.RequestUriTooLong, value);

    public static JsonResponse UnsupportedMediaType<T>(T value) => FromJson(HttpStatusCode.UnsupportedMediaType, value);

    public static JsonResponse RequestedRangeNotSatisfiable<T>(T value) => FromJson(HttpStatusCode.RequestedRangeNotSatisfiable, value);

    public static JsonResponse ExpectationFailed<T>(T value) => FromJson(HttpStatusCode.ExpectationFailed, value);

    public static JsonResponse UpgradeRequired<T>(T value) => FromJson(HttpStatusCode.UpgradeRequired, value);

    public static JsonResponse InternalServerError<T>(T value) => FromJson(HttpStatusCode.InternalServerError, value);

    public static JsonResponse NotImplemented<T>(T value) => FromJson(HttpStatusCode.NotImplemented, value);

    public static JsonResponse BadGateway<T>(T value) => FromJson(HttpStatusCode.BadGateway, value);

    public static JsonResponse ServiceUnavailable<T>(T value) => FromJson(HttpStatusCode.ServiceUnavailable, value);

    public static JsonResponse GatewayTimeout<T>(T value) => FromJson(HttpStatusCode.GatewayTimeout, value);

    public static JsonResponse HttpVersionNotSupported<T>(T value) => FromJson(HttpStatusCode.HttpVersionNotSupported, value);

    public static CoroutineResponse Continue(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Continue, coroutine);

    public static CoroutineResponse SwitchingProtocols(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.SwitchingProtocols, coroutine);

    public static CoroutineResponse Ok(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.OK, coroutine);

    public static CoroutineResponse Created(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Created, coroutine);

    public static CoroutineResponse Accepted(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Accepted, coroutine);

    public static CoroutineResponse NonAuthoritativeInformation(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NonAuthoritativeInformation, coroutine);

    public static CoroutineResponse NoContent(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NoContent, coroutine);

    public static CoroutineResponse ResetContent(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.ResetContent, coroutine);

    public static CoroutineResponse PartialContent(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.PartialContent, coroutine);

    public static CoroutineResponse MultipleChoices(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.MultipleChoices, coroutine);

    public static CoroutineResponse Ambiguous(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Ambiguous, coroutine);

    public static CoroutineResponse MovedPermanently(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.MovedPermanently, coroutine);

    public static CoroutineResponse Moved(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Moved, coroutine);

    public static CoroutineResponse Found(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Found, coroutine);

    public static CoroutineResponse Redirect(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Redirect, coroutine);

    public static CoroutineResponse SeeOther(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.SeeOther, coroutine);

    public static CoroutineResponse RedirectMethod(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RedirectMethod, coroutine);

    public static CoroutineResponse NotModified(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NotModified, coroutine);

    public static CoroutineResponse UseProxy(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.UseProxy, coroutine);

    public static CoroutineResponse Unused(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Unused, coroutine);

    public static CoroutineResponse TemporaryRedirect(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.TemporaryRedirect, coroutine);

    public static CoroutineResponse RedirectKeepVerb(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RedirectKeepVerb, coroutine);

    public static CoroutineResponse BadRequest(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.BadRequest, coroutine);

    public static CoroutineResponse Unauthorized(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Unauthorized, coroutine);

    public static CoroutineResponse PaymentRequired(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.PaymentRequired, coroutine);

    public static CoroutineResponse Forbidden(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Forbidden, coroutine);

    public static CoroutineResponse NotFound(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NotFound, coroutine);

    public static CoroutineResponse MethodNotAllowed(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.MethodNotAllowed, coroutine);

    public static CoroutineResponse NotAcceptable(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NotAcceptable, coroutine);

    public static CoroutineResponse ProxyAuthenticationRequired(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.ProxyAuthenticationRequired, coroutine);

    public static CoroutineResponse RequestTimeout(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RequestTimeout, coroutine);

    public static CoroutineResponse Conflict(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Conflict, coroutine);

    public static CoroutineResponse Gone(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.Gone, coroutine);

    public static CoroutineResponse LengthRequired(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.LengthRequired, coroutine);

    public static CoroutineResponse PreconditionFailed(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.PreconditionFailed, coroutine);

    public static CoroutineResponse RequestEntityTooLarge(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RequestEntityTooLarge, coroutine);

    public static CoroutineResponse RequestUriTooLong(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RequestUriTooLong, coroutine);

    public static CoroutineResponse UnsupportedMediaType(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.UnsupportedMediaType, coroutine);

    public static CoroutineResponse RequestedRangeNotSatisfiable(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.RequestedRangeNotSatisfiable, coroutine);

    public static CoroutineResponse ExpectationFailed(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.ExpectationFailed, coroutine);

    public static CoroutineResponse UpgradeRequired(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.UpgradeRequired, coroutine);

    public static CoroutineResponse InternalServerError(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.InternalServerError, coroutine);

    public static CoroutineResponse NotImplemented(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.NotImplemented, coroutine);

    public static CoroutineResponse BadGateway(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.BadGateway, coroutine);

    public static CoroutineResponse ServiceUnavailable(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.ServiceUnavailable, coroutine);

    public static CoroutineResponse GatewayTimeout(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.GatewayTimeout, coroutine);

    public static CoroutineResponse HttpVersionNotSupported(IEnumerator coroutine) => FromCoroutine(HttpStatusCode.HttpVersionNotSupported, coroutine);

    // csharpier-ignore-end
}

using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class StatusCodeRanges
    {
        public static List<int> Status1xx = new List<int>()
        {
            StatusCodes.Status100Continue,
            StatusCodes.Status101SwitchingProtocols,
            StatusCodes.Status102Processing
        };

        public static List<int> Status2xx = new List<int>()
        {
            StatusCodes.Status200OK,
            StatusCodes.Status201Created,
            StatusCodes.Status202Accepted,
            StatusCodes.Status203NonAuthoritative,
            StatusCodes.Status204NoContent,
            StatusCodes.Status205ResetContent,
            StatusCodes.Status206PartialContent,
            StatusCodes.Status207MultiStatus,
            StatusCodes.Status208AlreadyReported,
            StatusCodes.Status226IMUsed
        };

        public static List<int> Status3xx = new List<int>()
        {
            StatusCodes.Status300MultipleChoices,
            StatusCodes.Status301MovedPermanently,
            StatusCodes.Status302Found,
            StatusCodes.Status303SeeOther,
            StatusCodes.Status304NotModified,
            StatusCodes.Status305UseProxy,
            StatusCodes.Status306SwitchProxy,
            StatusCodes.Status307TemporaryRedirect,
            StatusCodes.Status308PermanentRedirect
        };

        public static List<int> Status4xx = new List<int>()
        {
            StatusCodes.Status400BadRequest,
            StatusCodes.Status401Unauthorized,
            StatusCodes.Status402PaymentRequired,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status405MethodNotAllowed,
            StatusCodes.Status406NotAcceptable,
            StatusCodes.Status407ProxyAuthenticationRequired,
            StatusCodes.Status408RequestTimeout,
            StatusCodes.Status409Conflict,
            StatusCodes.Status410Gone,
            StatusCodes.Status411LengthRequired,
            StatusCodes.Status412PreconditionFailed,
            StatusCodes.Status413PayloadTooLarge,
            StatusCodes.Status413RequestEntityTooLarge,
            StatusCodes.Status414RequestUriTooLong,
            StatusCodes.Status414UriTooLong,
            StatusCodes.Status415UnsupportedMediaType,
            StatusCodes.Status416RangeNotSatisfiable,
            StatusCodes.Status416RequestedRangeNotSatisfiable,
            StatusCodes.Status417ExpectationFailed,
            StatusCodes.Status418ImATeapot,
            StatusCodes.Status419AuthenticationTimeout,
            StatusCodes.Status421MisdirectedRequest,
            StatusCodes.Status422UnprocessableEntity,
            StatusCodes.Status423Locked,
            StatusCodes.Status424FailedDependency,
            StatusCodes.Status426UpgradeRequired,
            StatusCodes.Status428PreconditionRequired,
            StatusCodes.Status429TooManyRequests,
            StatusCodes.Status431RequestHeaderFieldsTooLarge,
            StatusCodes.Status451UnavailableForLegalReasons
        };

        public static List<int> Status5xx = new List<int>()
        {
            StatusCodes.Status500InternalServerError,
            StatusCodes.Status501NotImplemented,
            StatusCodes.Status502BadGateway,
            StatusCodes.Status503ServiceUnavailable,
            StatusCodes.Status504GatewayTimeout,
            StatusCodes.Status505HttpVersionNotsupported,
            StatusCodes.Status506VariantAlsoNegotiates,
            StatusCodes.Status507InsufficientStorage,
            StatusCodes.Status508LoopDetected,
            StatusCodes.Status510NotExtended,
            StatusCodes.Status511NetworkAuthenticationRequired
        };
    }
}

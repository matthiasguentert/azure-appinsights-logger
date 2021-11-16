using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class BodyLoggerMiddleware : IMiddleware
    {
        private readonly IOptions<BodyLoggerOptions> _options;
        private readonly IBodyReader _bodyReader;

        public BodyLoggerMiddleware(IOptions<BodyLoggerOptions> options, IBodyReader bodyReader)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _bodyReader = bodyReader ?? throw new ArgumentNullException(nameof(bodyReader));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestBody = string.Empty;
            if (_options.Value.HttpVerbs.Contains(context.Request.Method))
            {
                // Temporarily store request body
                requestBody = await _bodyReader.ReadRequestBodyAsync(context, _options.Value.MaxBytes, _options.Value.Appendix);

                _bodyReader.PrepareResponseBodyReading(context);
            }

            // hand over to the next middleware and wait for the call to return
            await next(context);

            if (_options.Value.HttpVerbs.Contains(context.Request.Method))
            {
                if (_options.Value.HttpCodes.Contains(context.Response.StatusCode))
                {
                    var responseBody = await _bodyReader.ReadResponseBodyAsync(context, _options.Value.MaxBytes, _options.Value.Appendix);

                    var requestTelemtry = context.Features.Get<RequestTelemetry>();
                    requestTelemtry?.Properties.Add(_options.Value.RequestBodyPropertyKey, requestBody);
                    requestTelemtry?.Properties.Add(_options.Value.ResponseBodyPropertyKey, responseBody);
                }
            }
        }
    }
}

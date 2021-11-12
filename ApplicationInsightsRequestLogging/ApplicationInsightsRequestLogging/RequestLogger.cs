using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class RequestLogger : IMiddleware
    {
        private readonly IOptions<RequestLoggerOptions> _options;

        public RequestLogger(IOptions<RequestLoggerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var method = context.Request.Method;
            var contentType = context.Request.ContentType;
            var canRead = context.Request.Body.CanRead;
            var path = context.Request.Path;

            // Ensure the request body can be read multiple times
            context.Request.EnableBuffering();

            if
            (
                canRead &&
                _options.Value.HttpVerbs.Contains(method) &&
                _options.Value.ContentType == contentType &&
                _options.Value.Path == path
            )
            {
                // Leave stream open so next middleware can read it
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 512, leaveOpen: true);

                string requestBody = string.Empty;

                if (_options.Value.MaxBytes > 0)
                {
                    var buffer = new char[_options.Value.MaxBytes];
                    var readBytes = await reader.ReadAsync(buffer, 0, _options.Value.MaxBytes);

                    requestBody = new string(buffer);

                    if (!string.IsNullOrEmpty(_options.Value.CutOffText))
                        requestBody += _options.Value.CutOffText;
                }
                else
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                // Reset stream position, so next middleware can read it
                context.Request.Body.Position = 0;

                // Write request body to App Insights
                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                requestTelemetry?.Properties.Add(_options.Value.PropertyKey, requestBody);
            }

            // Call next middleware in the pipeline
            await next(context);
        }
    }
}

using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class ResponseLogger : IMiddleware
    {
        private readonly IOptions<ResponseLoggerOptions> _options;

        public ResponseLogger(IOptions<ResponseLoggerOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Store original body stream
            var originalBodyStream = context.Response.Body;

            // Swap out stream with one that is buffered and suports seeking
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // hand over to the next middleware and wait for the call to return
            await next(context);

            try
            {
                if (_options.Value.ContentType == context.Response.ContentType)
                {
                    // Read response body from memory stream
                    memoryStream.Position = 0;
                    var reader = new StreamReader(memoryStream);

                    string responseBody;
                    if (_options.Value.MaxBytes > 0)
                    {
                        var buffer = new char[_options.Value.MaxBytes];
                        _ = await reader.ReadAsync(buffer, 0, _options.Value.MaxBytes);

                        responseBody = new string(buffer);

                        if (!string.IsNullOrEmpty(_options.Value.CutOffText))
                            responseBody += _options.Value.CutOffText;
                    }
                    else
                    {
                        responseBody = await reader.ReadToEndAsync();
                    }

                    // Write response body to App Insights
                    var requestTelemetry = context.Features.Get<RequestTelemetry>();
                    requestTelemetry?.Properties.Add(_options.Value.PropertyKey, responseBody);
                }
            }
            finally
            {
                // Copy body back so its available to the user agent
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);

                context.Response.Body = originalBodyStream;
            }
        }
    }
}

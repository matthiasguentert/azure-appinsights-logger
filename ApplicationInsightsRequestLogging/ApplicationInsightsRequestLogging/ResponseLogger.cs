using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class ResponseLogger : IMiddleware
    {
        private readonly ResponseLoggerOptions options;

        public ResponseLogger(ResponseLoggerOptions options) => this.options = options;

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
                if (this.options.ContentType == context.Response.ContentType)
                {
                    // Read response body from memory stream
                    memoryStream.Position = 0;
                    var reader = new StreamReader(memoryStream);

                    string responseBody;
                    if (this.options.MaxSize > 0)
                    {
                        var buffer = new char[this.options.MaxSize];
                        _ = await reader.ReadAsync(buffer, 0, this.options.MaxSize);

                        responseBody = new string(buffer);

                        if (!string.IsNullOrEmpty(this.options.CutOffText))
                            responseBody += this.options.CutOffText;
                    }
                    else
                    {
                        responseBody = await reader.ReadToEndAsync();
                    }

                    // Write response body to App Insights
                    var requestTelemetry = context.Features.Get<RequestTelemetry>();
                    requestTelemetry?.Properties.Add(this.options.PropertyKey, responseBody);
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

using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class RequestLogger : IMiddleware
    {
        private readonly RequestLoggerOptions options;

        public RequestLogger(RequestLoggerOptions options) => this.options = options;

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
                this.options.HttpVerbs.Contains(method) &&
                this.options.ContentType == contentType &&
                this.options.Path == path
            )
            {
                // Leave stream open so next middleware can read it
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 512, leaveOpen: true);

                string requestBody = string.Empty;

                if (this.options.MaxSize > 0)
                {
                    var buffer = new char[this.options.MaxSize];
                    var readBytes = await reader.ReadAsync(buffer, 0, this.options.MaxSize);

                    requestBody = new string(buffer);

                    if (!string.IsNullOrEmpty(this.options.CutOffText))
                        requestBody += this.options.CutOffText;
                }
                else
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                // Reset stream position, so next middleware can read it
                context.Request.Body.Position = 0;

                // Write request body to App Insights
                var requestTelemetry = context.Features.Get<RequestTelemetry>();
                requestTelemetry?.Properties.Add(options.PropertyKey, requestBody);
            }

            // Call next middleware in the pipeline
            await next(context);
        }
    }
}

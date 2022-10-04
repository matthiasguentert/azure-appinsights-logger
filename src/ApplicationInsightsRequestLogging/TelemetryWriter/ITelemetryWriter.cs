using Microsoft.AspNetCore.Http;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public interface ITelemetryWriter
    {
        public void Write(HttpContext context, string key, string value);
    }
}

using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class TelemetryWriter : ITelemetryWriter
    {
        public void Write(HttpContext context, string key, string value)
        {
            var requestTelemtry = context.Features.Get<RequestTelemetry>();

            // add to dictionary, creating an altered key name if already present
            requestTelemetry?.Properties.Add(
                !requestTelemetry.Properties.ContainsKey(key) ? key : $"{key}-dupe-{Guid.NewGuid().ToString()[..8]}",
                value);
        }
    }
}

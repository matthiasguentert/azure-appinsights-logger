using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class TelemetryWriter : ITelemetryWriter
    {
        public void Write(HttpContext context, string key, string value)
        {
            var requestTelemtry = context.Features.Get<RequestTelemetry>();
            requestTelemtry?.Properties.Add(key, value);
        }
    }
}

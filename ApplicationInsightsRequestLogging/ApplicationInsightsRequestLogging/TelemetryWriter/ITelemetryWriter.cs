using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public interface ITelemetryWriter
    {
        public void Write(HttpContext context, string key, string value);
    }

    public class TelemetryWriter : ITelemetryWriter
    {
        public void Write(HttpContext context, string key, string value)
        {
            var requestTelemtry = context.Features.Get<RequestTelemetry>();
            requestTelemtry?.Properties.Add(key, value);
        }
    }
}

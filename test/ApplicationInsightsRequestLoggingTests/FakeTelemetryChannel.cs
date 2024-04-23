using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;

namespace ApplicationInsightsRequestLoggingTests;

public class FakeTelemetryChannel : ITelemetryChannel
{
    public List<ITelemetry> SentTelemtries = new();
    public bool IsFlushed { get; private set; }
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; }

    public void Send(ITelemetry item)
    {
        SentTelemtries.Add(item);
    }

    public void Flush()
    {
        IsFlushed = true;
    }

    public void Dispose()
    {
    }
}

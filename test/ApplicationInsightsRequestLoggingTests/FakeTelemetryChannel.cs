using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;

namespace ApplicationInsightsRequestLoggingTests;

public class FakeTelemetryChannel : ITelemetryChannel
{
    public FakeTelemetryChannel()
    {
        
    }
    public IList<ITelemetry> Items { get; private set; }
    
    public void Dispose()
    {
    }

    public void Send(ITelemetry item)
    {
        Items.Add(item);
    }

    public void Flush()
    {
        throw new System.NotImplementedException();
    }

    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; }
}
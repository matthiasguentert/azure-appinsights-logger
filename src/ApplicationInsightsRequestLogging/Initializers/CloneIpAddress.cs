using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class CloneIpAddress : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is ISupportProperties propTelemetry && !propTelemetry.Properties.ContainsKey("client-ip"))
            {
                var clientIpValue = telemetry.Context.Location.Ip;
                propTelemetry.Properties.Add("client-ip", clientIpValue);
            }
        }
    }
}
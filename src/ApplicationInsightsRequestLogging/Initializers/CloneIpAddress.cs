using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class CloneIpAddress : ITelemetryInitializer
    {
        private readonly BodyLoggerOptions _options;

        public CloneIpAddress(IOptions<BodyLoggerOptions> options) => _options = options.Value;

        public void Initialize(ITelemetry telemetry)
        {
            if (!_options.DisableIpMasking) return;

            if (telemetry is ISupportProperties propTelemetry && !propTelemetry.Properties.ContainsKey("client-ip"))
            {
                var clientIpValue = telemetry.Context.Location.Ip;
                propTelemetry.Properties.Add("client-ip", clientIpValue);
            }
        }
    }
}
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class ClientIpInitializer : ITelemetryInitializer
    {
        private readonly BodyLoggerOptions _options;

        public ClientIpInitializer(IOptions<BodyLoggerOptions> options) => _options = options.Value;

        public void Initialize(ITelemetry telemetry)
        {
            if (!_options.DisableIpMasking) return;

            var clientIpKey = _options.ClientIpPropertyKey;
            if (telemetry is ISupportProperties propTelemetry && !propTelemetry.Properties.ContainsKey(clientIpKey))
            {
                var clientIpValue = telemetry.Context.Location.Ip;
                propTelemetry.Properties.Add(clientIpKey, clientIpValue);
            }
        }
    }
}
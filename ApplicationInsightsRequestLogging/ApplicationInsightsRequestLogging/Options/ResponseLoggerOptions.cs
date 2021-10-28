using System.Net.Mime;

namespace Azureblue.ApplicationInsights.RequestLogging.Options
{
    public class ResponseLoggerOptions
    {
        public string PropertyKey { get; set; } = "ResponseBody";

        public int MaxSize { get; set; } = 100;

        public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

        public string? ContentType { get; set; } = MediaTypeNames.Application.Json;
    }
}

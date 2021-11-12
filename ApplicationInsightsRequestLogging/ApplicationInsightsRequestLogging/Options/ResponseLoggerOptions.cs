using System.Net.Mime;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class ResponseLoggerOptions
    {
        public string PropertyKey { get; set; } = "ResponseBody";

        public int MaxBytes { get; set; } = 80000;

        public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

        public string? ContentType { get; set; } = MediaTypeNames.Application.Json;
    }
}

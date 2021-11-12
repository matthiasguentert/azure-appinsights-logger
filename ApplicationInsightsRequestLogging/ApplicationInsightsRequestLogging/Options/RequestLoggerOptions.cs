namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class RequestLoggerOptions
    {
        public string PropertyKey { get; set; } = "RequestBody";

        public string[] HttpVerbs { get; set; } = { "POST", "PUT" };

        public int MaxBytes { get; set; } = 80000;

        public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

        public string? ContentType { get; set; } = null;

        public string Path { get; set; } = "/";
    }
}

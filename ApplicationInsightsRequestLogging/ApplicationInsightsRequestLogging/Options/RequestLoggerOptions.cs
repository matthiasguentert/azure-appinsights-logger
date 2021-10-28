namespace Azureblue.ApplicationInsights.RequestLogging.Options
{
    public class RequestLoggerOptions
    {
        public string PropertyKey { get; set; } = "RequestBody";

        public string[] HttpVerbs { get; set; } = { "POST", "PUT" };

        public int MaxSize { get; set; } = 100;

        public string CutOffText { get; set; } = "\n---8<------------------------\nSSHORTENED-DUE-TO-MAXSIZE-LIMIT";

        public string? ContentType { get; set; } = null;

        public string Path { get; set; } = "/";
    }
}

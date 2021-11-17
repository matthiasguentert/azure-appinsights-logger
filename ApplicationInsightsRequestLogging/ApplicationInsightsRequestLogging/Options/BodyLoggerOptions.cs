using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public class BodyLoggerOptions
    {
        public BodyLoggerOptions()
        {
            HttpCodes.AddRange(StatusCodeRanges.Status4xx);
            HttpCodes.AddRange(StatusCodeRanges.Status5xx);
        }

        /// <summary>
        ///     Only write to App Insights on these HTTP status codes
        /// </summary>
        public List<int> HttpCodes { get; set; } = new List<int>();

        /// <summary>
        ///     Only these HTTP verbs will trigger logging
        /// </summary>
        public List<string> HttpVerbs { get; set; } = new List<string>()
        {
            HttpMethods.Post, 
            HttpMethods.Put,
            HttpMethods.Patch
        };

        /// <summary>
        ///     Which property key should be used
        /// </summary>
        public string RequestBodyPropertyKey { get; set; } = "RequestBody";

        /// <summary>
        ///     Which property key should be used
        /// </summary>
        public string ResponseBodyPropertyKey { get; set; } = "ResponseBody";

        /// <summary>
        ///     Defines how many bytes should be read from HTTP context
        /// </summary>
        public int MaxBytes { get; set; } = 80000;

        /// <summary>
        ///     Defines the text to append in case the body should be truncated <seealso cref="MaxBytes"/>
        /// </summary>
        public string Appendix { get; set; } = "\n---8<------------------------\nTRUNCATED DUE TO MAXBYTES LIMIT";
    }
}

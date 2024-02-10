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
        ///     Which property key should be used
        /// </summary>
        public string ClientIpPropertyKey { get; set; } = "ClientIp";
        
        /// <summary>
        ///     Defines the amount of bytes that should be read from HTTP context
        /// </summary>
        public int MaxBytes { get; set; } = 80000;

        /// <summary>
        ///     Defines the text to append in case the body should be truncated <seealso cref="MaxBytes"/>
        /// </summary>
        public string Appendix { get; set; } = "\n---8<------------------------\nTRUNCATED DUE TO MAXBYTES LIMIT";

        /// <summary>
        ///     Controls storage of client IP addresses https://learn.microsoft.com/en-us/azure/azure-monitor/app/ip-collection?tabs=net
        /// </summary>
        public bool DisableIpMasking { get; set; } = false;
        public List<string> PropertyNamesWithSensitiveData { get; set; } = new List<string>()
        {
            "password",
            "secret",
            "passwd",
            "api_key",
            "access_token",
            "accessToken",
            "auth",
            "credentials",
            "mysql_pwd"
        };

        public List<string> SensitiveDataRegexes { get; set; } = new List<string>()
        {
            @"(?:4[0-9]{12}(?:[0-9]{3})?|[25][1-7][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})" // credit cards from https://stackoverflow.com/questions/9315647/regex-credit-card-number-tests
        };
    }
}

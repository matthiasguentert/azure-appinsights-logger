using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public interface IBodyReader
    {
        public Task<string> ReadRequestBodyAsync(HttpContext context, int bytes, string appendix);

        public void PrepareResponseBodyReading(HttpContext context);

        public Task<string> ReadResponseBodyAsync(HttpContext context, int bytes, string appendix);

        public Task RestoreOriginalResponseBodyStreamAsync(HttpContext context);
    }
}

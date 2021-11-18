using Microsoft.AspNetCore.Http;
using System.IO;

namespace ApplicationInsightsRequestLoggingTests
{
    public static class HttpContextTestFactory
    {
        public static DefaultHttpContext CreateWithRequestBody(string body)
        {
            var context = new DefaultHttpContext();

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(body);
            streamWriter.Flush();
            memoryStream.Position = 0;

            context.Request.Body = memoryStream;
            context.Request.ContentLength = body.Length;

            return context;
        }
    }
}

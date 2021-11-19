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
            
            memoryStream.Seek(0, SeekOrigin.Begin);

            context.Request.Body = memoryStream;
            context.Request.ContentLength = body.Length;

            return context;
        }

        public static DefaultHttpContext CreateWithResponseBody(string body)
        {
            var context = new DefaultHttpContext();

            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine(body);
            streamWriter.Flush();

            memoryStream.Seek(0, SeekOrigin.Begin);

            context.Response.Body = memoryStream;
            context.Response.ContentLength = body.Length;

            var r = new StreamReader(memoryStream);            
            var foo = r.ReadToEnd();
            memoryStream.Seek(0, SeekOrigin.Begin);

            return context;
        }
    }
}

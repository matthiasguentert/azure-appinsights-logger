using Microsoft.AspNetCore.Builder;

namespace Azureblue.ApplicationInsights.RequestLogging.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestBodyLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLogger>();
        }

        public static IApplicationBuilder UseResponseBodyLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseLogger>();
        }
    }
}

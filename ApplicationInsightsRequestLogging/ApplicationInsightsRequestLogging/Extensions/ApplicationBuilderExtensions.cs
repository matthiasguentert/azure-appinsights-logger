using Microsoft.AspNetCore.Builder;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLogger>();
        }

        public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ResponseLogger>();
        }
    }
}

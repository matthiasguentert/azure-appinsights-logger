using Microsoft.AspNetCore.Builder;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseAppInsightsHttpBodyLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BodyLoggerMiddleware>();
        }
    }
}

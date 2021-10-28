using Microsoft.Extensions.DependencyInjection;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequestLogging(this IServiceCollection services, RequestLoggerOptions options)
        {
            return services.AddTransient(s => new RequestLogger(options));
        }

        public static IServiceCollection AddResponseLogging(this IServiceCollection services, ResponseLoggerOptions options)
        {
            return services.AddTransient(s => new ResponseLogger(options));
        }
    }
}

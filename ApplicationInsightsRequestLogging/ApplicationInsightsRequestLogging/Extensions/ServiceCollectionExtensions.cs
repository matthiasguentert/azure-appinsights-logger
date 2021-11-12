using Microsoft.Extensions.DependencyInjection;
using System;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class ServiceCollectionExtensions
    {
        #region RequestLogging

        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            AddRequestLogger(services);

            return services;
        }

        public static IServiceCollection AddRequestLogging(this IServiceCollection services, Action<RequestLoggerOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            AddRequestLogger(services, setupAction);

            return services;
        }

        internal static void AddRequestLogger(IServiceCollection services)
        {
            services.AddTransient<RequestLogger>();
        }

        internal static void AddRequestLogger(IServiceCollection services, Action<RequestLoggerOptions> setupAction)
        {
            AddRequestLogger(services);
            services.Configure(setupAction);
        }

        #endregion

        #region ResponseLogging

        public static IServiceCollection AddResponseLogging(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddOptions();
            AddResponseLogger(services);

            return services;
        }

        public static IServiceCollection AddResponseLogging(this IServiceCollection services, Action<ResponseLoggerOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            AddResponseLogger(services, setupAction);

            return services;
        }

        internal static void AddResponseLogger(IServiceCollection services)
        {
            services.AddTransient<ResponseLogger>();
        }

        internal static void AddResponseLogger(IServiceCollection services, Action<ResponseLoggerOptions> setupAction)
        {
            AddResponseLogger(services);
            services.Configure(setupAction);
        }

        #endregion
    }
}

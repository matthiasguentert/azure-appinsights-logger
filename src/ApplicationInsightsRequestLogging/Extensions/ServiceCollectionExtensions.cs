﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.ApplicationInsights.Extensibility;

namespace Azureblue.ApplicationInsights.RequestLogging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAppInsightsHttpBodyLogging(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();
            AddBodyLogger(services);

            return services;
        }

        public static IServiceCollection AddAppInsightsHttpBodyLogging(this IServiceCollection services, Action<BodyLoggerOptions> setupAction)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (setupAction == null)
                throw new ArgumentNullException(nameof(setupAction));

            AddBodyLogger(services, setupAction);

            return services;
        }

        private static void AddBodyLogger(IServiceCollection services, Action<BodyLoggerOptions> setupAction)
        {
            AddBodyLogger(services);
            services.Configure(setupAction);
        }
        
        private static void AddBodyLogger(IServiceCollection services)
        {
            services.AddSingleton<BodyLoggerMiddleware>();
            services.AddSingleton<IBodyReader, BodyReader>();
            services.AddSingleton<ITelemetryWriter, TelemetryWriter>();
            services.AddSingleton<ITelemetryInitializer, ClientIpInitializer>();
        }
    }
}

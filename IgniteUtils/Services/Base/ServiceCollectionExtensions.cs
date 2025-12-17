using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteUtils.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSingletonWithBase<TService>(
                this IServiceCollection services,
                TService instance)
                where TService : class
        {
            // Register the instance under its own type
            services.AddSingleton(instance);

            // If the instance also implements ServiceBase, register it under ServiceBase
            if (instance is ServiceBase baseService)
            {
                services.AddSingleton<ServiceBase>(sp => baseService);
            }

            return services;
        }

        // Register by type (let DI construct it)
        public static IServiceCollection AddSingletonWithBase<TService>(
            this IServiceCollection services)
            where TService : class
        {
            services.AddSingleton<TService>();

            // At runtime, resolve and cast
            services.AddSingleton<ServiceBase>(sp =>
            {
                var resolved = sp.GetRequiredService<TService>();
                return resolved as ServiceBase
                       ?? throw new InvalidOperationException(
                           $"{typeof(TService).Name} must inherit ServiceBase to be registered as ServiceBase.");
            });

            return services;
        }

        public static IServiceCollection AddSingletonWithBase<TService, TInterface>(
            this IServiceCollection services,
            TService instance)
            where TService : class, TInterface
            where TInterface : class
        {
            // Register concrete type
            services.AddSingleton(instance);

            // Register interface → same instance
            services.AddSingleton<TInterface>(sp => instance);

            // Register base class if applicable
            if (instance is ServiceBase baseService)
            {
                services.AddSingleton<ServiceBase>(sp => baseService);
            }

            return services;
        }
    }
}

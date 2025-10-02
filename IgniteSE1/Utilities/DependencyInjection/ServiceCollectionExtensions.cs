using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities.DependencyInjection
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
    }
}

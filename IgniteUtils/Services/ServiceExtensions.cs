using Google.Protobuf.WellKnownTypes;
using InstanceUtils.Services.Networking;
using InstanceUtils.Services.WebPanel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace InstanceUtils.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, Uri TargetWebApp)
        {
            //Register Singletons with base service type
            services.AddSingletonWithBase<PatchService>();
            services.AddSingletonWithBase<ServerStateService>();
            services.AddSingletonWithBase<AssemblyResolverService>();
           


            services.AddSingleton<PanelCoreService>();
            services.AddSingleton<PanelSocketClient>();
            services.AddHostedService<PanelBackgroundService>();

            //Register a specific HTTP client for the web panel. This will include the necessary headers (PanelHTTPClient)
            services.AddIgniteHttpClient(TargetWebApp);


            return services;
        }


        /// <summary>
        /// Adds a typed HTTP client for the specified client type to the dependency injection container and configures
        /// it with Ignite-specific message handlers.
        /// </summary>
        /// <remarks>This method registers the specified client type with the default HttpClientFactory
        /// and adds Ignite-specific HTTP message handlers to the pipeline. Use this method to ensure that outgoing HTTP
        /// requests from the client include required Ignite headers.</remarks>
        /// <typeparam name="TClient">The type of the HTTP client to register. Must be a class.</typeparam>
        /// <param name="services">The service collection to which the HTTP client and its handlers are added.</param>
        /// <returns>An IHttpClientBuilder that can be used to further configure the HTTP client.</returns>
        public static IHttpClientBuilder AddIgniteHttpClient(
            this IServiceCollection services,
            Uri baseAddress)
        {
            services.TryAddTransient<InstanceHeaderHandler>();

            return services
                .AddHttpClient<PanelHTTPClient>(client =>
                {
                    client.BaseAddress = baseAddress;
                })
                .AddHttpMessageHandler<InstanceHeaderHandler>();
        }

    }
}

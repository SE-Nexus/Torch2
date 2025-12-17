using Google.Protobuf.WellKnownTypes;
using IgniteUtils.Services.Networking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace IgniteUtils.Services
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, Uri TargetWebApp)
        {
            //Register Singletons with base service type
            services.AddSingletonWithBase<PatchService>();
            services.AddSingletonWithBase<ServerStateService>();
            services.AddSingletonWithBase<AssemblyResolverService>();

            //Register ConsoleLogStream HTTP Client
            services.AddIgniteHttpClient<HttpConsoleLogClient>(new Uri(TargetWebApp, "api/instance/logstream/"));


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
        public static IHttpClientBuilder AddIgniteHttpClient<TClient>(
                this IServiceCollection services, Uri baseAddress)
                where TClient : class
        {
            services.TryAddTransient<InstanceHeaderHandler>();
            //services.TryAddTransient<RunIdHeaderHandler>();
            // services.TryAddTransient<AuthHeaderHandler>();

            return services
                .AddHttpClient<TClient>(client =>
                {
                    client.BaseAddress = baseAddress;
                })
                .AddHttpMessageHandler<InstanceHeaderHandler>();
                //.AddHttpMessageHandler<RunIdHeaderHandler>()
                //.AddHttpMessageHandler<AuthHeaderHandler>();
        }

    }
}

using Google.Protobuf.WellKnownTypes;
using InstanceUtils.Services.Commands.Contexts;
using InstanceUtils.Services.Networking;
using InstanceUtils.Services.WebPanel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Text;
using Torch2API.Models.Commands;

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
            services.AddSingletonWithBase<CommandService>();


            services.AddSingleton<PanelCoreService>();
            services.AddSingleton<PanelSocketClient>();
            services.AddScoped<CommandContextAccessor>();
            services.AddScoped<ICommandContext>(sp => sp.GetRequiredService<CommandContextAccessor>().context);
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

        /// <summary>
        /// Creates an instance of the specified type, optionally using dependency injection services if provided.
        /// </summary>
        /// <remarks>If a service provider is supplied, constructor dependencies are resolved using
        /// dependency injection. If instantiation via dependency injection fails, the method attempts to create the
        /// instance using reflection. If both approaches fail, an exception is thrown. This method supports
        /// instantiating types with non-public constructors when dependency injection is not used.</remarks>
        /// <param name="type">The type of object to create. Must be a concrete, instantiable type.</param>
        /// <param name="services">An optional service provider used to resolve constructor dependencies. If null, the instance is created
        /// using reflection without dependency injection.</param>
        /// <returns>An instance of the specified type, or null if instantiation fails and no services are provided.</returns>
        /// <exception cref="InvalidOperationException">Thrown if both dependency injection and reflection-based instantiation fail for the specified type.</exception>
        public static object CreateInstance(System.Type type, IServiceProvider? services)
        {
            try
            {
                //No DI container? Fallback to plain reflection
                if (services == null)
                    return Activator.CreateInstance(type, nonPublic: true)!;


                //just use the main provider. Maybe add scoped stuff in future
                return ActivatorUtilities.CreateInstance(services, type);
            }
            catch (Exception ex)
            {
                // 💥 Fallback to reflection if DI creation fails
                AnsiConsole.MarkupLineInterpolated(
                $"[bold red][[DI]][/] Failed to create instance of [yellow]{type.Name}[/]: {ex.Message}");

                if (services == null)
                    return null;

                try
                {
                    return Activator.CreateInstance(type, nonPublic: true)!;
                }
                catch (Exception innerEx)
                {
                    throw new InvalidOperationException(
                        $"Unable to create instance of type {type.FullName}. " +
                        $"DI failed and fallback instantiation also failed.",
                        innerEx);
                }
            }
        }

    }
}

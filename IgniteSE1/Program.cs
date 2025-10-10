using IgniteSE1.Models;
using IgniteSE1.Services;
using IgniteSE1.Utilities;
using IgniteSE1.Utilities.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            //Setup directories and logging
            ConfigService ConfigService = new ConfigService();
            ConsoleManager IgniteConsole = new ConsoleManager(ConfigService);
            

            // Load Config. Even if we are in CLI mode, we may need config values
            ConfigService.LoadConfig();

            // Initialize Console
            if (!await IgniteConsole.InitConsole(args))
                return;


            // Setup Dependency Injection
            IServiceCollection services = new ServiceCollection();
            services.AddSingletonWithBase<ConfigService>(ConfigService);
            services.AddSingletonWithBase<ConsoleManager>(IgniteConsole);
            services.AddSingletonWithBase<SteamService>();
            services.AddSingletonWithBase<ServerStateService>();
            services.AddSingletonWithBase<AssemblyResolverService>();
            services.AddSingletonWithBase<GameService>();
            services.AddSingletonWithBase<InstanceManager>();
            services.AddSingletonWithBase<PatchService>();

            services.AddSingleton<ServiceManager>();
            services.AddHttpClient();







            ConfigureServices(services);
            IServiceProvider provider = services.BuildServiceProvider(true);
            

            ServiceManager serviceManager = provider.GetService<ServiceManager>();
            bool success = await serviceManager.StartAllServices();

            //await Task.Delay(2000);
            //s.RequestServerStateChange(ServerStateCommand.Kill);

            Console.ReadKey();
            //AnsiConsole.Markup("[underline red]Hello[/] World!");
        }




        public static void ConfigureServices(IServiceCollection services)
        {
            // Add other services



        }
    }
}

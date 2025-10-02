using IgniteSE1.Services;
using IgniteSE1.Utilities;
using IgniteSE1.Utilities.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
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
            IgniteConfigService ConfigService = new IgniteConfigService();
            ConsoleManager IgniteConsole = new ConsoleManager(ConfigService);
            

            // Load Config. Even if we are in CLI mode, we may need config values
            ConfigService.LoadConfig();

            // Initialize Console
            if (!IgniteConsole.InitConsole(args))
                return;


            // Setup Dependency Injection
            IServiceCollection services = new ServiceCollection();
            services.AddSingletonWithBase<IgniteConfigService>(ConfigService);
            services.AddSingletonWithBase<ConsoleManager>(IgniteConsole);
            services.AddSingleton<ServiceManager>();



           



            ConfigureServices(services);

            IServiceProvider provider = services.BuildServiceProvider(true);
            

            foreach (var svc in provider.GetServices<ServiceBase>())
            {
                AnsiConsole.MarkupLine($"[green]Starting service:[/] [yellow]{svc.GetType().Name}[/]");
            }


            var s = provider.GetService<ServiceManager>();
            //AnsiConsole.Markup("[underline red]Hello[/] World!");
        }




        public static void ConfigureServices(IServiceCollection services)
        {
            // Add other services



        }
    }
}

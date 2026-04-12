using IgniteSE1.Commands;
using IgniteSE1.Services;
using IgniteSE1.Services.Networking;
using InstanceUtils.Commands;
using InstanceUtils.Commands.TestCommand;
using InstanceUtils.Models.Server;
using InstanceUtils.Services;
using InstanceUtils.Services.Networking;
using InstanceUtils.Services.WebPanel;
using InstanceUtils.Utils.CommandUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage;

namespace IgniteSE1
{
    internal class Program
    {
        public const string AppName = "IgniteSE1";

        static async Task Main(string[] args)
        {
            Console.Clear();

            //Setup directories and logging
            ConfigService ConfigService = new ConfigService();
            ConfigService.LoadConfig();


            ConsoleManager IgniteConsole = new ConsoleManager(AppName, ConfigService);

            // Initialize Console
            if (!await IgniteConsole.InitConsole(args))
                return;

            IHostBuilder builder = Host.CreateDefaultBuilder(args).UseConsoleLifetime();
            builder.ConfigureServices((hostContext, services) =>
            {
                // Add hosted services or other services here if needed
                services.AddHostedService<ServiceManager>();

                // Setup Dependency Injection
                services.AddCoreServices(ConfigService.TargetWebPanel);

                services.AddSingletonWithBase<ConfigService, IConfigService>(ConfigService);
                services.AddSingletonWithBase<ConsoleManager>(IgniteConsole);
                services.AddSingletonWithBase<SteamService>();

                services.AddSingletonWithBase<GameService>();
                services.AddSingletonWithBase<GameChatService>();
                services.AddSingletonWithBase<ProfileManager>();

                services.AddSingleton<IPanelCoreService, PanelCoreService>();

                services.AddHttpClient();

            }).ConfigureLogging(logging => { logging.ClearProviders(); });

            IHost host = builder.Build();
            IServiceProvider provider = host.Services;

            // Start Named Pipe server for CLI communication
            var commandService = provider.GetService<CommandService>();
            var namedPipeServer = new NamedPipeCommandServer(commandService, ConfigService.Identification.InstanceID);
            namedPipeServer.Start();
            //Console.WriteLine("Named Pipe server started for CLI communication");

            await host.RunAsync();
        }
    }
}

using Grpc.Core;
using IgniteSE1.Commands;
using IgniteSE1.Services;
using IgniteSE1.Services.ProtoServices;
using IgniteUtils.Commands;
using IgniteUtils.Commands.TestCommand;
using IgniteUtils.Models.Server;
using IgniteUtils.Services;
using IgniteUtils.Services.Networking;
using IgniteUtils.Utils.CommandUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyGrpcApp;
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


            CommandLineManager CLI = new CommandLineManager(ConfigService.Config.ProtoServerPort);
            ConsoleManager IgniteConsole = new ConsoleManager(AppName, CLI);
         
            
            // Initialize Console
            if (!await IgniteConsole.InitConsole(args))
                return;

            IHostBuilder builder = Host.CreateDefaultBuilder(args).UseConsoleLifetime();
            builder.ConfigureServices((hostContext, services) =>
            {
                // Add hosted services or other services here if needed
                services.AddHostedService<ServiceManager>();
                ConfigureServices(services);

                // Setup Dependency Injection
                services.AddCoreServices(ConfigService.TargetWebPanel);

                services.AddSingletonWithBase<ConfigService, IConfigService>(ConfigService);
                services.AddSingletonWithBase<ConsoleManager>(IgniteConsole);
                services.AddSingletonWithBase<CommandLineManager>(CLI);
                services.AddSingletonWithBase<SteamService>();

                services.AddSingletonWithBase<GameService>();
                services.AddSingletonWithBase<InstanceManager>();


                services.AddSingleton<CommandLineProtoService>();
               
                services.AddHttpClient();

            });

            IHost host = builder.Build();
            IServiceProvider provider = host.Services;


            CommandLineManager cmdLine = provider.GetService<CommandLineManager>();
            cmdLine.RootCommand.Add(CommandLineBuilder.BuildFromType<StateCommands>(provider));
            cmdLine.RootCommand.Add(CommandLineBuilder.BuildFromType<TestCommand>(provider));

            //ServiceManager serviceManager = provider.GetService<ServiceManager>();

            int Port = ConfigService.Config.ProtoServerPort;
            Server server = new Server
            {
                Services =
                {
                    Greeter.BindService(new GreeterService()),
                    CommandLine.BindService(provider.GetService<CommandLineProtoService>())

                },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("gRPC server listening on port " + Port);


            await host.RunAsync();

          
            //await Task.Delay(2000);
            //s.RequestServerStateChange(ServerStateCommand.Kill);


           
            //AnsiConsole.Markup("[underline red]Hello[/] World!");
        }




        public static void ConfigureServices(IServiceCollection services)
        {
            // Add other services



        }
    }
}

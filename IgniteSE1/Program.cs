using Grpc.Core;
using IgniteSE1.Commands;
using IgniteSE1.Services;
using IgniteSE1.Services.ProtoServices;
using IgniteSE1.Utilities;
using IgniteUtils.Commands;
using IgniteUtils.Commands.TestCommand;
using IgniteUtils.Services;
using Microsoft.Extensions.DependencyInjection;
using MyGrpcApp;
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
using VRage;

namespace IgniteSE1
{
    internal class Program
    {
        public const string AppName = "IgniteSE1";



        static async Task Main(string[] args)
        {
            //Setup directories and logging
            ConfigService ConfigService = new ConfigService();
            ConfigService.LoadConfig();


            CommandLineManager CLI = new CommandLineManager(ConfigService.Config.ProtoServerPort);
            ConsoleManager IgniteConsole = new ConsoleManager(AppName, CLI);
         
            
            // Initialize Console
            if (!await IgniteConsole.InitConsole(args))
                return;

           

            // Setup Dependency Injection
            IServiceCollection services = new ServiceCollection();
            services.AddSingletonWithBase<ConfigService>(ConfigService);
            services.AddSingletonWithBase<ConsoleManager>(IgniteConsole);
            services.AddSingletonWithBase<CommandLineManager>(CLI);
            services.AddSingletonWithBase<SteamService>();
            services.AddSingletonWithBase<ServerStateService>();
            services.AddSingletonWithBase<AssemblyResolverService>();
            services.AddSingletonWithBase<GameService>();
            services.AddSingletonWithBase<InstanceManager>();
            services.AddSingletonWithBase<PatchService>();




            services.AddSingleton<CommandLineProtoService>();
            services.AddSingleton<ServiceManager>();
            services.AddHttpClient();


            


            ConfigureServices(services);
            IServiceProvider provider = services.BuildServiceProvider(true);


            CommandLineManager cmdLine = provider.GetService<CommandLineManager>();
            cmdLine.RootCommand.Add(CommandLineBuilder.BuildFromType<StateCommands>(provider));
            cmdLine.RootCommand.Add(CommandLineBuilder.BuildFromType<TestCommand>(provider));

            ServiceManager serviceManager = provider.GetService<ServiceManager>();

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

            bool success = await serviceManager.InitAllServicesAsync(provider);

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

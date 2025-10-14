using Grpc.Core;
using IgniteSE1.Models;
using IgniteSE1.Services;
using IgniteSE1.Services.ProtoServices;
using IgniteSE1.Utilities;
using IgniteSE1.Utilities.CLI;
using IgniteSE1.Utilities.DependencyInjection;
using IgniteSE1.Utilities.TestCommand;
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
            


            services.AddSingleton<CommandLineProtoService>();
            services.AddSingleton<ServiceManager>();
            services.AddHttpClient();


            


            ConfigureServices(services);
            IServiceProvider provider = services.BuildServiceProvider(true);
            IgniteConsole.CommandLineManager.RootCommand.Add(CommandLineBuilder.BuildFromType<TestCommand>(provider));



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

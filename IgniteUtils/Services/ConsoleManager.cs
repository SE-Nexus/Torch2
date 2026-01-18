using Grpc.Core;
using IgniteSE1.Services;
using InstanceUtils.Logging;
using InstanceUtils.Models.Server;
using InstanceUtils.Services;
using InstanceUtils.Services.Networking;
using InstanceUtils.Utils.CommandUtils;
using Microsoft.Extensions.DependencyInjection;
using MyGrpcApp;
using NLog;
using NLog.Config;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstanceUtils.Services
{
    public class ConsoleManager : ServiceBase
    {
        private static Logger Logger;
        private static Mutex appMutex;

        private readonly ConfigService _ConfigService;

        private readonly Stack<string> _PreviousCommands;
        
        //private CommandLineManager _cli;


        public string ConsoleName { get; private set; }
        public string mutexName { get; private set; }



        public static string AppArguments => Environment.CommandLine;

        int _protoServicePort;


        public ConsoleManager(string Name, ConfigService configs)
        {
            //_cli = cli;
            _ConfigService = configs;
            ConsoleName = Name;
            _protoServicePort = _ConfigService.Config.ProtoServerPort;
            mutexName = AppDomain.CurrentDomain.FriendlyName.Replace("\\", "_");
        }


        /// <summary>
        /// Initializes the console application and determines whether to start a new instance or continue in
        /// command-line interface (CLI) mode.
        /// </summary>
        /// <param name="args">An array of command-line arguments passed to the application. May be empty if no arguments are provided.</param>
        /// <returns>true if a new instance of the application is started; otherwise, false if an existing instance is detected
        /// and CLI mode should be used.</returns>
        public async Task<bool> InitConsole(string[] args)
        {
            bool IsServer = CheckMutexNew();

            if (IsServer)
            {
                UpdateConsoleTitle($"{_ConfigService.InstanceName} - Loading");
                // New App
                SetupConsole();
            }
            else
            {
                //Existing App Already Running. We should continue with CLI mode
                Console.Title = $"{ConsoleName} CLI Mode";
                await SetupCommandLineManager(args);
            }

            //await SetupCommandLineManager(IsServer, args);
            return IsServer;
        }

        public async Task SetupCommandLineManager(string[] args)
        {
             await ProcessCLICommand(args);
        }

        public async Task ProcessCLICommand(string[] args)
        {
            if (args.Length == 0)
                return;


            //Check if interactive mode
            if (args[0].Equals("--interactive", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.Write(new Panel("[bold green]Interactive mode enabled![/] [grey]Type [yellow]exit[/] to quit.[/]").Border(BoxBorder.Rounded).Header("[white on green] CLI Mode [/]"));

                while (true)
                {
                    var input = AnsiConsole.Prompt(
                            new TextPrompt<string>("[grey]>[/]")
                                .PromptStyle("deepskyblue1")
                        );

                    if (string.IsNullOrEmpty(input))
                        continue;

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                        break;


                    //Store previous commands
                    _PreviousCommands.Push(input);
                    
                    

                    string[] inputArgs = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    await SendCLIRequest(inputArgs);
                }

                //Lets return. As the command would attempt to continue with the --interactive stuff
                return;
            }
            else if (args[0].Equals("--snake", StringComparison.OrdinalIgnoreCase))
            {
                SnakeGame game = new SnakeGame();
                game.Run();
                return;
            }


            //Send singular command;
            await SendCLIRequest(args);
        }

        private async Task SendCLIRequest(string[] args)
        {
            Channel channel = new Channel($"localhost:{_protoServicePort}", ChannelCredentials.Insecure);
            var client = new CommandLine.CommandLineClient(channel);

            var request = new CLIRequest();
            request.Command.AddRange(args);   // ✅ this is the key line

            var reply = await client.ProcessCLIAsync(request);
            AnsiConsole.WriteLine(reply.Result);
        }

        public void UpdateConsoleTitle(string newTitle)
        {
            Console.Title = newTitle;
        }


        private void SetupConsole()
        {

            // Very early in startup, before loading config / creating loggers
            LogManager.Setup()
                .SetupExtensions(ext => ext.RegisterTarget<ColoredConsoleLogTarget>("ColoredConsole"))

                .LoadConfigurationFromFile("nlog.config");

            LogManager.ReconfigExistingLoggers();
            Logger = LogManager.GetCurrentClassLogger();

            

            //Enable Colored Console Formattings
            Console.InputEncoding = new System.Text.UTF8Encoding(false);
            Console.OutputEncoding = new System.Text.UTF8Encoding(false);

            var version = Assembly.GetExecutingAssembly().GetName().Version;

            AnsiConsole.Write(
                new Rule($"[yellow]Ignite (Torch2) v{version}[/]")
                    .RuleStyle("grey")
                    .Centered());


        }

        public bool CheckMutexNew()
        {
            appMutex = new Mutex(true, mutexName, out bool isNewInstance);
            return isNewInstance;
        }

        public override Task<bool> Init()
        {
            //HttpConsoleLogClient client = this.ServiceProvider.GetService<HttpConsoleLogClient>();
            //var rule = new LoggingRule("*", LogLevel.Debug, client);
            //LogManager.Configuration.AddTarget("HttpConsoleLog", client);
          
            //LogManager.Configuration.LoggingRules.Add(rule);
            //LogManager.ReconfigExistingLoggers();

            return Task.FromResult(true);
        }
    }
}

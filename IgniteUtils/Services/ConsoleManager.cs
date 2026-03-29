using Grpc.Core;
using IgniteSE1.Services;
using InstanceUtils.Logging;
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

        // replaced Stack with history List to support indexed navigation (Up/Down)
        private readonly List<string> _History;
        
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

            // Initialize history
            _History = new List<string>();
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
                    // use history-aware reader (supports Up/Down)
                    var input = ReadLineWithHistory("> ");

                    if (string.IsNullOrEmpty(input))
                        continue;

                    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                        break;


                    // store previous commands (append)
                    _History.Add(input);

                    string[] inputArgs = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        await SendCLIRequest(inputArgs);
                    }
                    catch (Exception ex)
                    {
                        // avoid killing interactive mode on a single-bad command
                        AnsiConsole.MarkupLine($"[red]Command failed:[/] {ex.Message}");
                    }
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
                .SetupExtensions(ext =>
                {
                    ext.RegisterTarget<ColoredConsoleLogTarget>("ColoredConsole");
                    ext.RegisterTarget<WebSocketLogTarget>("WebSocketLog");
                })
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

        // history-aware console reader: supports Up/Down to navigate previous commands, Backspace, Escape (clear), basic editing.
        private string ReadLineWithHistory(string prompt)
        {
            var buffer = new StringBuilder();
            int historyIndex = _History.Count; // one past the end
            void Render()
            {
                try
                {
                    // Build full line and clear then write
                    var line = prompt + buffer.ToString();
                    int width = Console.WindowWidth;
                    Console.Write("\r" + new string(' ', Math.Max(0, width - 1)) + "\r");
                    Console.Write(line);
                }
                catch
                {
                    // Fallback if Console.WindowWidth isn't available
                    Console.Write("\r" + prompt + buffer.ToString());
                }
            }

            Console.Write(prompt);
            while (true)
            {
                var key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return buffer.ToString();
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (buffer.Length > 0)
                    {
                        buffer.Remove(buffer.Length - 1, 1);
                        Render();
                    }
                    continue;
                }

                if (key.Key == ConsoleKey.Escape)
                {
                    buffer.Clear();
                    Render();
                    continue;
                }

                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (_History.Count > 0 && historyIndex > 0)
                    {
                        historyIndex--;
                        buffer.Clear();
                        buffer.Append(_History[historyIndex]);
                        Render();
                    }
                    continue;
                }

                if (key.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex < _History.Count - 1)
                    {
                        historyIndex++;
                        buffer.Clear();
                        buffer.Append(_History[historyIndex]);
                    }
                    else
                    {
                        historyIndex = _History.Count;
                        buffer.Clear();
                    }
                    Render();
                    continue;
                }

                // Basic character input
                var c = key.KeyChar;
                if (!char.IsControl(c))
                {
                    buffer.Append(c);
                    Console.Write(c);
                }
            }
        }
    }
}

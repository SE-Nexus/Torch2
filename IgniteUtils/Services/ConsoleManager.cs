using IgniteUtils.Logging;
using IgniteUtils.Services;
using IgniteUtils.Services.Networking;
using Microsoft.Extensions.DependencyInjection;
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

namespace IgniteUtils.Services
{
    public class ConsoleManager : ServiceBase
    {
        private static Logger Logger;
        private static Mutex appMutex;
        private CommandLineManager _cli;


        public string ConsoleName { get; private set; }
        public string mutexName { get; private set; }


        public ConsoleManager(string Name, CommandLineManager cli)
        {
            _cli = cli;
            ConsoleName = Name;
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
                // New App
                SetupConsole();
            }
            else
            {
                //Existing App Already Running. We should continue with CLI mode
                Console.Title = $"{ConsoleName} CLI Mode";
            }

            await _cli.SetupCommandLineManager(IsServer, args);
            return IsServer;
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
            HttpConsoleLogClient client = this.ServiceProvider.GetService<HttpConsoleLogClient>();
            var rule = new LoggingRule("*", LogLevel.Debug, client);
            LogManager.Configuration.AddTarget("HttpConsoleLog", client);
          
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.ReconfigExistingLoggers();

            return Task.FromResult(true);
        }
    }
}

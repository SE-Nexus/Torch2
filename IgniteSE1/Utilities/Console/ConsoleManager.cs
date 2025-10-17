using IgniteSE1.Models;
using IgniteSE1.Services;
using IgniteSE1.Utilities;
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

namespace IgniteSE1.Utilities
{
    public class ConsoleManager : ServiceBase
    {
        public CommandLineManager CommandLineManager { get; private set; }

        private static Logger Logger;
        private static Mutex appMutex;

        // Configuration service reference
        public ConfigService configs { get; private set; }
        public string mutexName { get; private set; }

        public ConsoleManager(ConfigService configs) 
        {
            CommandLineManager = new CommandLineManager(this);
            this.configs = configs;
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
                UpdateConsoleTitleStatus(ServerStatusEnum.Initializing);
                SetupConsole();
            }
            else
            {
                //Existing App Already Running. We should continue with CLI mode
                Console.Title = "IgniteSE1 CLI Mode";
            }

            await CommandLineManager.SetupCommandLineManager(IsServer, args);
            return IsServer;
        }


        public void UpdateConsoleTitleStatus(ServerStatusEnum status)
        {
            UpdateConsoleTitle($"{configs.Config.IgniteCMDName} [{status}]");
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
    }
}

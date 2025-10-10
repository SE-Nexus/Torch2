using IgniteSE1.Models;
using IgniteSE1.Services;
using IgniteSE1.Utilities;
using NLog;
using NLog.Config;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{
    public class ConsoleManager : ServiceBase
    {
        private CommandLineManager _commandLineManager;

        private static Logger Logger;
        private static Mutex appMutex;

        private static string mutexName;

        // Configuration service reference
        private ConfigService _configs;

        public ConsoleManager(ConfigService configs) 
        {
            _commandLineManager = new CommandLineManager();
            _configs = configs;
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
            
            if (CheckMutexNew())
            {
                // New App
                UpdateConsoleTitleStatus(ServerStatusEnum.Initializing);
                SetupConsole();

                return true;
            }
            else
            {
                //Existing App Already Running. We should continue with CLI mode
                Console.Title = "IgniteSE1 CLI Mode";
                await _commandLineManager.ProcessCLICommand(args);
                return false;
            }
        }


        public void UpdateConsoleTitleStatus(ServerStatusEnum status)
        {
            UpdateConsoleTitle($"{_configs.Config.IgniteCMDName} [{status}]");
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

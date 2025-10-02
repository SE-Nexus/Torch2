using IgniteSE1.Utilities;
using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1.Utilities
{
    public class ConsoleManager : IAppService
    {
        private static Logger Logger;
        private static Mutex appMutex;

        private static string mutexName;

        public ConsoleManager() 
        {
            mutexName = AppDomain.CurrentDomain.FriendlyName.Replace("\\", "_");
            
            
            
            SetupConsole();
            


        }

        public bool InitConsole()
        {
            //Check for existing instance
            if (CheckMutex())
            {
                return true;
            }
            else
            {
                //New instance




                return false;
            }


        }


        private void SetupConsole()
        {
            Console.Title = "IgniteSE1 Console";

            //Add our Colored Console Target
            ColoredConsoleLogTarget consoleLogTarget = new ColoredConsoleLogTarget();
            LoggingRule consoleRule = new LoggingRule("*", NLog.LogLevel.Debug, consoleLogTarget);
            LogManager.Configuration?.LoggingRules.Add(consoleRule);

            LogManager.ReconfigExistingLoggers();
            Logger = LogManager.GetCurrentClassLogger();

            //Enable Colored Console Formattings
            Console.InputEncoding = new System.Text.UTF8Encoding(false);
            Console.OutputEncoding = new System.Text.UTF8Encoding(false);

            Logger.Warn("Hello world!");
        }

        public bool CheckMutex()
        {
            appMutex
        }


    }
}

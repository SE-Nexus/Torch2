using IgniteSE1.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteSE1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            

            //Setup directories and logging
            ConsoleManager consoleM = new ConsoleManager();


            IServiceCollection services = new ServiceCollection();
            //AnsiConsole.Markup("[underline red]Hello[/] World!");

        }




        public static void ConfigureServices(IServiceCollection services)
        {
            // Add other services



        }
    }
}

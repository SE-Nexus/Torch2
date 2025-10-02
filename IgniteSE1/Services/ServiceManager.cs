using IgniteSE1.Utilities;
using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class ServiceManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public IEnumerable<ServiceBase> _Services { get; private set; }

        public ServiceManager(IEnumerable<ServiceBase> services) 
        {
            _Services = services;
        }

        public async Task StartAllServices()
        {
            foreach(var svc in _Services)
            {
                string type = svc.GetType().Name;
                AnsiConsole.MarkupLine($"[green]Init service:[/] [yellow]{type}[/]");
                bool result = await svc.Init();

                if (!result)
                {
                    _logger.Fatal($"Failed to init {type}");
                }
            }

            //Start Steam



        }



    }
}

using IgniteSE1.Utilities;
using Microsoft.Extensions.DependencyInjection;
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
        private SteamService _steamService; 
        private ServerStateService _serverState;
        public IServiceProvider ServiceProvider { private set; get; }

        public IEnumerable<ServiceBase> Services { get; private set; }

        public ServiceManager(IEnumerable<ServiceBase> services, SteamService steamService, ServerStateService serverstate) 
        {
            Services = services;
            _steamService = steamService; // Ensure SteamService is injected. We will need to init this first
            _serverState = serverstate;

            _serverState.ServerStatusChanged += _serverState_ServerStatusChanged;
        }

        private void _serverState_ServerStatusChanged(object sender, Models.ServerStatusEnum e)
        {
            if(e == Models.ServerStatusEnum.Starting)
            {
                foreach (var item in Services)
                {
                    item.ServerStarting();
                }
            }
        }



        public async Task<bool> StartAllServices(IServiceProvider provider)
        {
            ServiceProvider = provider;


            if (!await _steamService.Init())
            {
                _logger.Fatal("Failed to init SteamService. Exiting...");
                return false;
            }

            const int initTimeoutMs = 4000; // 10 seconds
            bool allSucceeded = true;

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("yellow"))
                .StartAsync("Initializing services...", async ctx =>
                {
                    foreach (var svc in Services)
                    {
                        if (svc.IsInitialized)
                            continue;

                        //Set the service provider
                        svc.SetServiceProvider(provider);

                        string type = svc.GetType().Name;
                        ctx.Status($"[green]Initializing[/] [yellow]{type}[/]");

                        var initTask = svc.Init();
                        var timeoutTask = Task.Delay(initTimeoutMs);

                        var completed = await Task.WhenAny(initTask, timeoutTask);

                        if (completed == timeoutTask)
                        {
                            _logger.Fatal($"Timeout initializing {type}");
                            AnsiConsole.MarkupLine($"[red] Timeout:[/] {type}");
                            allSucceeded = false;
                            continue;
                        }

                        bool result = await initTask; // safe, it's finished
                        if (!result)
                        {
                            _logger.Fatal($"Failed to init {type}");
                            AnsiConsole.MarkupLine($"[red] Failed:[/] {type}");
                            allSucceeded = false;
                        }
                        else
                        {
                            svc.IsInitialized = true;

                            AnsiConsole.MarkupLine($"[green] Initialized:[/] {type}");
                        }
                    }

                    //After init hooks
                    if (allSucceeded)
                    {
                        foreach(var svc in Services)
                        {
                            if (!svc.IsInitialized)
                                continue;

                            svc.AfterInit();
                        }
                    }


                });


            if (allSucceeded)
                _serverState.ChangeServerStatus(Models.ServerStatusEnum.Idle);
            else
                _serverState.ChangeServerStatus(Models.ServerStatusEnum.Error);

            return allSucceeded;

        }

        public void StateChange()
        {

        }



    }
}

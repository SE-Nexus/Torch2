using IgniteUtils.Logging;
using IgniteUtils.Models;
using IgniteUtils.Services;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IgniteSE1.Services
{
    public class ServiceManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private SteamService _steamService;
        private ServerStateService _serverState;

        private Spinner consoleSpinner;
        private Style consoleSpinnerStyle;


        public IServiceProvider ServiceProvider { private set; get; }

        public IEnumerable<ServiceBase> Services { get; private set; }

        public ServiceManager(IEnumerable<ServiceBase> services, SteamService steamService, ServerStateService serverstate)
        {
            consoleSpinner = Spinner.Known.Dots2;
            consoleSpinnerStyle = Style.Parse("yellow");


            Services = services;
            _steamService = steamService; // Ensure SteamService is injected. We will need to init this first
            _serverState = serverstate;

            _serverState.ServerStatusChanged += _serverState_ServerStatusChanged;
        }

        private async void _serverState_ServerStatusChanged(object sender, ServerStatusEnum e)
        {
            //Console styles
            if (e == ServerStatusEnum.Starting)
            {
                await AnsiConsole.Status()
                .Spinner(consoleSpinner)
                .SpinnerStyle(consoleSpinnerStyle)
                .StartAsync("Starting services...", async ctx =>
                {
                    await RunServicePhaseAsync(Services, e, ServerStatusEnum.Running, ctx);
                });
            }
            else if (e == ServerStatusEnum.Stopping)
            {
                await AnsiConsole.Status()
                .Spinner(consoleSpinner)
                .SpinnerStyle(consoleSpinnerStyle)
                .StartAsync("Starting services...", async ctx =>
                {
                    bool success = await RunServicePhaseAsync(Services, e, ServerStatusEnum.Stopped, ctx);

                    if (success)
                    {
                        string exePath = Assembly.GetEntryAssembly().Location;

                        Process.Start(exePath);
                        Environment.Exit(0); // clean exit, triggers finally{} blocks

                        //_serverState.RequestServerStateChange(ServerStateCommand.Idle);
                    }

                });
            }
        }



        public async Task<bool> InitAllServicesAsync(IServiceProvider provider)
        {
            ServiceProvider = provider;


            if (!await _steamService.Init())
            {
                _logger.Fatal("Failed to init SteamService. Exiting...");
                return false;
            }

            bool allSucceeded = false;
            await AnsiConsole.Status()
                .Spinner(consoleSpinner)
                .SpinnerStyle(consoleSpinnerStyle)
                .StartAsync("Initializing services...", async ctx =>
                {
                    allSucceeded = await InitAllServicesAsync(ctx, provider);
                });



            if (allSucceeded)
                _serverState.RequestServerStateChange(ServerStateCommand.Idle);
            else
                _serverState.ChangeServerStatus(ServerStatusEnum.Error);

            return allSucceeded;

        }


        private async Task<bool> InitAllServicesAsync(StatusContext ctx, IServiceProvider provider)
        {
            bool allSucceeded = true;
            const int initTimeoutMs = 15000; // Example timeout

            var uninitializedServices = Services.Where(s => !s.IsInitialized).ToList();
            int total = uninitializedServices.Count;
            int completedCount = 0;


            //Initing should be done in order
            foreach (var svc in uninitializedServices)
            {
                // Inject provider if needed
                svc.SetServiceProvider(provider);

                string type = svc.GetType().Name;
                ctx.Status($"[green]Initializing[/] [yellow]{type}[/] ({completedCount}/{total})");

                var initTask = svc.Init();
                var timeoutTask = Task.Delay(initTimeoutMs);
                var completed = await Task.WhenAny(initTask, timeoutTask);

                completedCount++;

                if (completed == timeoutTask)
                {
                    _logger.NoConsole(LogLevel.Fatal, $"Timeout initializing {type}");
                    AnsiConsole.MarkupLine($"[red]⏱ Timeout:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                    continue;
                }

                bool result = await initTask; // safe, it's completed

                if (!result)
                {
                    _logger.NoConsole(LogLevel.Fatal, $"Failed to init {type}");
                    AnsiConsole.MarkupLine($"[red]✖ Failed:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                }
                else
                {
                    svc.IsInitialized = true;
                    _logger.NoConsole(LogLevel.Info, $"Service initialized successfully: {type}");
                    AnsiConsole.MarkupLine($"[green]+ Initialized:[/] {type} ({completedCount}/{total})");
                }
            }

            // After init hooks
            if (allSucceeded)
            {
                _logger.NoConsole(LogLevel.Info, "All services initialized successfully.");
                AnsiConsole.MarkupLine("[green]All services initialized successfully![/]");

                foreach (var svc in Services.Where(s => s.IsInitialized))
                    svc.AfterInit();
            }
            else
            {
                _logger.NoConsole(LogLevel.Fatal, "One or more services failed to initialize.");
                AnsiConsole.MarkupLine("[red]Some services failed to initialize.[/]");
            }

            return allSucceeded;
        }

        private async Task<bool> RunServicePhaseAsync(IEnumerable<ServiceBase> services, ServerStatusEnum currentStatus, ServerStatusEnum successStatus, StatusContext ctx)
        {
            var serviceList = services.ToList();
            int total = serviceList.Count;
            int completedCount = 0;
            bool allSucceeded = true;
            string phaseName = currentStatus.ToString();

            _logger.NoConsole(LogLevel.Info, $"{phaseName} {total} service(s)...");
            ctx.Status($"[yellow]{phaseName} services...[/] (0/{total})");

            var tasks = serviceList
                .Select(svc => new { Service = svc, Task = svc.CallState(currentStatus) })
                .ToList();

            while (tasks.Count > 0)
            {
                Task<bool> finishedTask = await Task.WhenAny(tasks.Select(t => t.Task));
                var finished = tasks.First(t => t.Task == finishedTask);
                tasks.Remove(finished);

                bool result = false;
                string type = finished.Service.GetType().Name;

                try
                {
                    result = await finishedTask;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Exception while {phaseName.ToLower()} {type}");
                    result = false;
                }

                completedCount++;

                if (result)
                {
                    _logger.NoConsole(LogLevel.Info, $"Service {phaseName.ToLower()} successfully: {type} ({completedCount}/{total})");
                    AnsiConsole.MarkupLine($"[green]✔ {phaseName}:[/] {type} ({completedCount}/{total})");
                }
                else
                {
                    _logger.NoConsole(LogLevel.Error, $"Service failed to {phaseName.ToLower()}: {type} ({completedCount}/{total})");
                    AnsiConsole.MarkupLine($"[red]✖ Failed to {phaseName.ToLower()}:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                }

                ctx.Status($"[yellow]{phaseName}...[/] ({completedCount}/{total})");
            }

            if (allSucceeded)
            {
                _logger.NoConsole(LogLevel.Info, $"All services {phaseName.ToLower()} successfully.");
                AnsiConsole.MarkupLine($"[green]All services {phaseName.ToLower()} successfully![/]");
                _serverState.ChangeServerStatus(successStatus);
            }
            else
            {
                _logger.NoConsole(LogLevel.Fatal, $"One or more services failed to {phaseName.ToLower()}.");
                AnsiConsole.MarkupLine($"[red]One or more services failed to {phaseName.ToLower()}.[/]");
                _serverState.ChangeServerStatus(ServerStatusEnum.Error);
            }

            return allSucceeded;
        }


    }
}

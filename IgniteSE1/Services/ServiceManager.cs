using IgniteSE1.Utilities;
using IgniteUtils.Logging;
using IgniteUtils.Models;
using IgniteUtils.Services;
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

        private async void _serverState_ServerStatusChanged(object sender, ServerStatusEnum e)
        {
            if (e == ServerStatusEnum.Starting)
            {
                await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("yellow"))
                .StartAsync("Starting services...", async ctx =>
                {
                    await StartAllServicesAsync(ctx);
                });
            }
            else if (e == ServerStatusEnum.Stopping)
            {

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

            bool allSucceeded = false;
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots2)
                .SpinnerStyle(Style.Parse("yellow"))
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

        public void StateChange()
        {

        }


        private async Task StartAllServicesAsync(StatusContext ctx)
        {
            var tasks = Services
                .Select(svc => new { Service = svc, Task = svc.ServerStarting() })
                .ToList();

            int total = tasks.Count;
            int completedCount = 0;
            bool allSucceeded = true;

            while (tasks.Count > 0)
            {
                Task<bool> finishedTask = await Task.WhenAny(tasks.Select(t => t.Task));
                var finished = tasks.First(t => t.Task == finishedTask);
                tasks.Remove(finished);

                bool result = await finishedTask;
                completedCount++;

                string type = finished.Service.GetType().Name;

                if (result)
                {
                    _logger.NoConsole(LogLevel.Info, $"Service started successfully: {type} ({completedCount}/{total})");
                    AnsiConsole.MarkupLine($"[green]✔ Started:[/] {type} ({completedCount}/{total})");
                }
                else
                {
                    _logger.NoConsole(LogLevel.Error, $"Service failed to start: {type} ({completedCount}/{total})");
                    AnsiConsole.MarkupLine($"[red]✖ Failed:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                }

                ctx.Status($"[yellow]Starting...[/] ({completedCount}/{total})");
            }

            if (allSucceeded)
            {
                _logger.NoConsole(LogLevel.Info, "All services started successfully.");
                AnsiConsole.MarkupLine("[green]All services started successfully![/]");

                _serverState.ChangeServerStatus(ServerStatusEnum.Running);
            }
            else
            {
                _logger.NoConsole(LogLevel.Fatal, "One or more services failed to start.");
                AnsiConsole.MarkupLine("[red]One or more services failed to start.[/]");
                _serverState.ChangeServerStatus(ServerStatusEnum.Error);
            }
        }

        private async Task<bool> InitAllServicesAsync(StatusContext ctx, IServiceProvider provider)
        {
            bool allSucceeded = true;
            const int initTimeoutMs = 15000; // Example timeout

            var uninitializedServices = Services.Where(s => !s.IsInitialized).ToList();
            int total = uninitializedServices.Count;
            int completedCount = 0;

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

    }
}

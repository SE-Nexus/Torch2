using InstanceUtils.Logging;
using InstanceUtils.Services;
using Microsoft.Extensions.Hosting;
using NLog;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Models;

namespace IgniteSE1.Services
{
    public class ServiceManager : BackgroundService
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly SteamService _steamService;
        private readonly ServerStateService _serverState;
        private readonly Spinner _consoleSpinner = Spinner.Known.Dots2;
        private readonly Style _consoleSpinnerStyle = Style.Parse("yellow");

        public IServiceProvider ServiceProvider { private set; get; }
        public IEnumerable<ServiceBase> Services { get; private set; }

        public ServiceManager(IEnumerable<ServiceBase> services, SteamService steamService, ServerStateService serverstate, IServiceProvider provider)
        {
            ServiceProvider = provider;
            Services = services;
            _steamService = steamService;
            _serverState = serverstate;
            _serverState.ServerStatusChanged += _serverState_ServerStatusChanged;
        }

        private async Task RunStatusAsync(string statusMessage, Func<StatusContext, Task> action)
        {
            await AnsiConsole.Status()
                .Spinner(_consoleSpinner)
                .SpinnerStyle(_consoleSpinnerStyle)
                .StartAsync(statusMessage, action);
        }

        private void LogAndDisplay(LogLevel logLevel, string markup)
        {
            _logger.NoConsole(logLevel, markup.RemoveMarkup());
            AnsiConsole.MarkupLine(markup);
        }

        private void LogServiceStatus(string type, string status, string markup, bool success = true)
        {
            _logger.NoConsole(success ? LogLevel.Info : LogLevel.Fatal, $"{status}: {type}");
            AnsiConsole.MarkupLine(markup);
        }

        private async void _serverState_ServerStatusChanged(object sender, ServerStatusEnum e)
        {
            //Console styles
            if (e == ServerStatusEnum.Starting)
            {
                await RunStatusAsync("Starting services...", async ctx =>
                {
                    await RunServicePhaseAsync(Services, e, ServerStatusEnum.Running, ctx);
                });
            }
            else if (e == ServerStatusEnum.Stopping)
            {
                await RunStatusAsync("Stopping services...", async ctx =>
                {
                    bool success = await RunServicePhaseAsync(Services, e, ServerStatusEnum.Stopped, ctx);

                    if (success)
                    {
                        string exePath = Assembly.GetEntryAssembly()!.Location;
                        string exeDir = Path.GetDirectoryName(exePath)!;

                        var psi = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/c start \"\" \"{exePath}\"",
                            WorkingDirectory = exeDir,
                            UseShellExecute = true,
                            CreateNoWindow = false
                        };

                        Process.Start(psi);
                        Environment.Exit(0);

                        //_serverState.RequestServerStateChange(ServerStateCommand.Idle);
                    }
                });
            }
        }



        public async Task<bool> InitAllServicesAsync()
        {
            if (!await _steamService.Init())
            {
                _logger.Fatal("Failed to init SteamService. Exiting...");
                return false;
            }

            bool allSucceeded = false;
            await RunStatusAsync("Initializing services...", async ctx =>
            {
                allSucceeded = await InitAllServicesAsync(ctx, ServiceProvider);
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
            const int initTimeoutMs = 15000;

            var uninitializedServices = Services.Where(s => !s.IsInitialized).ToList();
            int total = uninitializedServices.Count;
            int completedCount = 0;

            foreach (var svc in uninitializedServices)
            {
                svc.SetServiceProvider(provider);
                string type = svc.GetType().Name;
                ctx.Status($"[green]Initializing[/] [yellow]{type}[/] ({completedCount}/{total})");

                var initTask = svc.Init();
                var completed = await Task.WhenAny(initTask, Task.Delay(initTimeoutMs));
                completedCount++;

                if (completed != initTask)
                {
                    LogAndDisplay(LogLevel.Fatal, $"[red]⏱ Timeout:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                    continue;
                }

                bool result = await initTask;
                if (!result)
                {
                    LogAndDisplay(LogLevel.Fatal, $"[red]✖ Failed:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                }
                else
                {
                    svc.IsInitialized = true;
                    LogAndDisplay(LogLevel.Info, $"[green]+ Initialized:[/] {type} ({completedCount}/{total})");
                }
            }

            if (allSucceeded)
            {
                LogAndDisplay(LogLevel.Info, "[green]All services initialized successfully![/]");
                foreach (var svc in Services.Where(s => s.IsInitialized))
                    svc.AfterInit();
            }
            else
            {
                LogAndDisplay(LogLevel.Fatal, "[red]Some services failed to initialize.[/]");
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

            LogAndDisplay(LogLevel.Info, $"[yellow]{phaseName} {total} service(s)...[/]");
            ctx.Status($"[yellow]{phaseName} services...[/] (0/{total})");

            var tasks = serviceList
                .Select(svc => new { Service = svc, Task = svc.CallState(currentStatus) })
                .ToList();

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks.Select(t => t.Task));
                var finished = tasks.First(t => t.Task == finishedTask);
                tasks.Remove(finished);

                string type = finished.Service.GetType().Name;
                bool result = false;

                try
                {
                    result = await finishedTask;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, $"Exception while {phaseName.ToLower()} {type}");
                }

                completedCount++;

                if (result)
                {
                    LogAndDisplay(LogLevel.Info, $"[green]✔ {phaseName}:[/] {type} ({completedCount}/{total})");
                }
                else
                {
                    LogAndDisplay(LogLevel.Error, $"[red]✖ Failed to {phaseName.ToLower()}:[/] {type} ({completedCount}/{total})");
                    allSucceeded = false;
                }

                ctx.Status($"[yellow]{phaseName}...[/] ({completedCount}/{total})");
            }

            if (allSucceeded)
            {
                LogAndDisplay(LogLevel.Info, $"[green]All services {phaseName.ToLower()} successfully![/]");
                _serverState.ChangeServerStatus(successStatus);

                if (successStatus == ServerStatusEnum.Running)
                {
                    foreach (var svc in serviceList)
                    {
                        try
                        {
                            svc.ServerStarted();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, $"Exception in ServerStarted for {svc.GetType().Name}");
                        }
                    }
                }
            }
            else
            {
                LogAndDisplay(LogLevel.Fatal, $"[red]One or more services failed to {phaseName.ToLower()}.[/]");
                _serverState.ChangeServerStatus(ServerStatusEnum.Error);
            }

            return allSucceeded;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {

                await InitAllServicesAsync();

            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }
}

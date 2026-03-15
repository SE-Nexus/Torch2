using IgniteSE1.Configs;
using IgniteSE1.Services;
using InstanceUtils.Models.Server;
using InstanceUtils.Services.Commands.Contexts;
using Sandbox.Engine.Utils;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Constants;
using Torch2API.DTOs.Instances;
using Torch2API.DTOs.WebSockets;
using Torch2API.Models.Configs;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelCoreService : IPanelCoreService
    {
        private readonly IConfigService _ConfigService;
        private readonly PanelHTTPClient _webPanelClient;
        private readonly ProfileManager _instanceManager;
        private readonly ServerStateService _serverStateService;
        private readonly CommandService _cmdService;
        private readonly IServiceProvider _provider;

        public string InstancePublicIP { get; private set; } = "0.0.0.0";



        public PanelCoreService(IConfigService ConfigService, PanelHTTPClient webPanelClient, ProfileManager instanceManager, ServerStateService stateservice, CommandService cmdService, IServiceProvider provider)
        {
            _serverStateService = stateservice;
            _instanceManager = instanceManager;
            _webPanelClient = webPanelClient;
            _ConfigService = ConfigService;
            _cmdService = cmdService;
            _provider = provider;

            _instanceManager.ProfilesChanged += _instanceManager_ProfilesChanged;
            _instanceManager.WorldsChanged += _instanceManager_WorldsChanged;

        }

        private async void _instanceManager_WorldsChanged(List<WorldInfo> obj)
        {
            //No matter who runs this command, we need to alert the panel. Should this be here or in the create world?
            await _webPanelClient.PostAsync(WebAPIConstants.AllWorlds, obj);
        }

        private async void _instanceManager_ProfilesChanged(List<ProfileCfg> obj)
        {
            await _webPanelClient.PostAsync(WebAPIConstants.AllProfiles, obj);
        }

        public async Task GetPublicIP()
        {
            try
            {
                using var http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var ip = await http.GetStringAsync("https://api.ipify.org");
                InstancePublicIP = string.IsNullOrWhiteSpace(ip) ? null : ip.Trim();
            }
            catch (HttpRequestException)   // No internet, DNS failure, 4xx/5xx
            {
                return;
            }
            catch (TaskCanceledException)  // Timeout
            {
                
                return;
            }
        }


        public async Task SendStatus(CancellationToken ct = default)
        {
     
                ProfileCfg cfg = _instanceManager.GetCurrentProfile();

                var status = new TorchInstanceBase
                {


                    InstanceID = _ConfigService.Identification.InstanceID.ToString(),
                    Name = _ConfigService.InstanceName,
                    MachineName = Environment.MachineName,
                    IPAddress = InstancePublicIP,
                    GamePort = cfg?.InstancePort ?? 0,
                    ProfileName = cfg?.InstanceName ?? "Loading...",
                    TargetWorld = cfg?.TargetWorld ?? "Loading...",
                    TorchVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "v0.0.0",
                    ServerStatus = _serverStateService.CurrentServerStatus,
                    CurrentStateCmd = _serverStateService.CurrentSateRequest,
                    GameUpTime = _serverStateService.GetGameRunningTime(),
                    StateTime = _serverStateService.GetStateTime()
                };

            var success = await _webPanelClient.PostAsync(WebAPIConstants.Update, status,ct);
        }

        public async Task RunAsync(CancellationToken ct)
        {
            await Task.CompletedTask;
        }

        public Task RunWSCommand(SocketMsgEnvelope msg)
        {
            if(_cmdService.TryGetCommand(msg.Command, out var command))
            {
                AnsiConsole.WriteLine("Received WS Command: " + msg.Command);
                WebPanelContext ctx = new WebPanelContext(command, msg);
                ctx.RunCommand(_provider);
            }
            else
            {
                AnsiConsole.WriteLine($"WS Command not found: {msg.Command}");
            }


            return Task.CompletedTask;
        }
    }
}

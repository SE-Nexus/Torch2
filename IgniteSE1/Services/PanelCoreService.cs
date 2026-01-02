using IgniteSE1.Configs;
using IgniteSE1.Services;
using IgniteUtils.Models.Server;
using Sandbox.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Torch2API.DTOs.Instances;

namespace IgniteUtils.Services.WebPanel
{
    public class PanelCoreService : IPanelCoreService
    {
        private readonly IConfigService _ConfigService;
        private readonly PanelHTTPClient _webPanelClient;
        private readonly InstanceManager _instanceManager;
        private readonly ServerStateService _serverStateService;

        public string InstancePublicIP { get; private set; } = "0.0.0.0";



        public PanelCoreService(IConfigService ConfigService, PanelHTTPClient webPanelClient, InstanceManager instanceManager, ServerStateService stateservice)
        {
            _serverStateService = stateservice;
            _instanceManager = instanceManager;
            _webPanelClient = webPanelClient;
            _ConfigService = ConfigService;
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


        public async Task SendStatus()
        {
            InstanceCfg cfg = _instanceManager.GetCurrentInstance();



            var status = new TorchInstanceBase
            {
                InstanceID = _ConfigService.Identification.InstanceID.ToString(),
                Name = _ConfigService.InstanceName,
                MachineName = Environment.MachineName,
                IPAddress = InstancePublicIP,
                GamePort = cfg?.InstancePort ?? 0,
                InstanceName = cfg?.InstanceName ?? "Loading...",
                TargetWorld = cfg?.TargetWorld ?? "Loading...",
                TorchVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "v0.0.0",
                ServerStatus = _serverStateService.CurrentServerStatus,
                CurrentStateCmd = _serverStateService.CurrentSateRequest,
                GameUpTime = _serverStateService.GetGameRunningTime(),
                StateTime = _serverStateService.GetStateTime()
            };

            HttpResponseMessage response =
           await _webPanelClient.Http.PostAsJsonAsync(
               "api/instance/update",
               status);

            response.EnsureSuccessStatusCode();
        }

    }
}

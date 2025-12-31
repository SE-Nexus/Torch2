using IgniteUtils.Models.Server;
using Sandbox.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Torch2API.DTOs.Instances;

namespace IgniteUtils.Services.WebPanel
{
    public class PanelCoreService
    {
        private readonly IConfigService _ConfigService;
        private readonly PanelHTTPClient _webPanelClient;

        public PanelCoreService(IConfigService ConfigService, PanelHTTPClient webPanelClient)
        {
            _webPanelClient = webPanelClient;
            _ConfigService = ConfigService;
        }


        public async Task SendStatus()
        {
            var status = new TorchInstanceBase
            {
                Name = _ConfigService.InstanceName,
                InstanceID = _ConfigService.Identification.InstanceID.ToString(),
            };

            HttpResponseMessage response =
           await _webPanelClient.Http.PostAsJsonAsync(
               "api/instance/status/update",
               status);

            response.EnsureSuccessStatusCode();
        }

    }
}

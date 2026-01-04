using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelBackgroundService : BackgroundService
    {
        private readonly IPanelCoreService _WebService;
        private readonly int _UpdateIntervalMs = 500;
        private readonly Timer _UpdateTimer;
        private readonly PanelSocketClient _socketClient;

        public PanelBackgroundService(IPanelCoreService webService, PanelSocketClient sClient)
        {
            _UpdateTimer = new Timer(_UpdateIntervalMs);
            _UpdateTimer.AutoReset = false;
            _UpdateTimer.Elapsed += _UpdateTimer_Elapsed;
            //Can pull the config service here if needed for intervals/URIs

            _socketClient = sClient;
            _WebService = webService;
        }

        private async void _UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await _WebService.SendStatus();
            _UpdateTimer.Start();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _UpdateTimer.Start();
            await _socketClient.RunAsync(stoppingToken);
            await _WebService.GetPublicIP();
            return;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _UpdateTimer?.Stop();
            return base.StopAsync(cancellationToken);
        }

        public static async Task<string?> GetPublicIpAsync()
        {
            try
            {
                using var http = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                var ip = await http.GetStringAsync("https://api.ipify.org");
                return string.IsNullOrWhiteSpace(ip) ? null : ip.Trim();
            }
            catch (HttpRequestException)   // No internet, DNS failure, 4xx/5xx
            {
                return null;
            }
            catch (TaskCanceledException)  // Timeout
            {
                return null;
            }
        }
    }
}

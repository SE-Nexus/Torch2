using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelBackgroundService : BackgroundService
    {
        private readonly IPanelCoreService _webService;
        private readonly Timer _updateTimer;
        private readonly PanelSocketClient _socketClient;

        public PanelBackgroundService(IPanelCoreService webService, PanelSocketClient sClient)
        {
            _webService = webService;
            _socketClient = sClient;
            _updateTimer = new Timer(500) { AutoReset = false };
            _updateTimer.Elapsed += _UpdateTimer_Elapsed;
        }

        private async void _UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await _webService.SendStatus();
            }
            catch (Exception) { }
            finally
            {
                _updateTimer.Start();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _updateTimer.Start();
            await _webService.GetPublicIP();
            await _socketClient.RunAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _updateTimer?.Stop();
            await _socketClient.ShutdownAsync(cancellationToken);
        }
    }
}

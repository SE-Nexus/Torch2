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
            try
            {
                await _WebService.SendStatus();
            }
            catch (Exception) { }
            finally
            {
                _UpdateTimer.Start();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _UpdateTimer.Start();
            await _WebService.GetPublicIP();

            //The following is blocking
            await _socketClient.RunAsync(stoppingToken);
            return;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _UpdateTimer?.Stop();
            await _socketClient.ShutdownAsync(cancellationToken);
            return;
        }
    }
}

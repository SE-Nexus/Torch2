using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace IgniteUtils.Services.WebPanel
{
    public class PanelBackgroundService : BackgroundService
    {
        private readonly PanelCoreService _WebService;
        private readonly int _UpdateIntervalMs = 500;
        private readonly Timer _UpdateTimer;

        public PanelBackgroundService(PanelCoreService webService)
        {
            _UpdateTimer = new Timer(_UpdateIntervalMs);
            _UpdateTimer.AutoReset = false;
            _UpdateTimer.Elapsed += _UpdateTimer_Elapsed;
            //Can pull the config service here if needed for intervals/URIs

            _WebService = webService;
        }

        private void _UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _WebService.SendStatus();
            _UpdateTimer.Start();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _UpdateTimer.Start();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _UpdateTimer?.Stop();
            return base.StopAsync(cancellationToken);
        }

    }
}

using System.Collections.Concurrent;
using Torch2API.DTOs.Instances;
using Torch2WebUI.Services.SQL;
using Timer = System.Timers.Timer;

namespace Torch2WebUI.Services
{
    public class InstanceManager
    {
        private readonly ConcurrentDictionary<string, TorchInstanceBase> _PendingInstances = new();

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly Timer CleanupTimer;
        private readonly IServiceScopeFactory _scopeFactory;




        public InstanceManager(IServiceScopeFactory scopeFactory) 
        {
            _scopeFactory = scopeFactory;
            CleanupTimer = new Timer(_timeout.Add(TimeSpan.FromSeconds(2)));
            CleanupTimer.AutoReset = false;

            CleanupTimer.Elapsed += CleanupTimer_Elapsed;
            CleanupTimer.Start();
        }

        private void CleanupTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            CleanupInstances();
            CleanupTimer.Start();
        }

        private void CleanupInstances()
        {

        }
    }
}

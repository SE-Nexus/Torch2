using System.Collections.Concurrent;
using Torch2API.DTOs.Instances;
using Torch2WebUI.Services.SQL;
using Timer = System.Timers.Timer;

namespace Torch2WebUI.Services
{
    public class InstanceManager
    {
        private readonly ConcurrentDictionary<string, ITorchInstanceInfo> _PendingInstances = new();

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly Timer CleanupTimer;

        private readonly AppDbContext _Database;


        public InstanceManager(AppDbContext Database) 
        {
            _Database = Database;
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

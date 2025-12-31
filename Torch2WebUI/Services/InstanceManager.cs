using System.Collections.Concurrent;
using Torch2API.DTOs.Instances;
using Timer = System.Timers.Timer;

namespace Torch2WebUI.Services
{
    public class InstanceManager
    {
        private readonly ConcurrentDictionary<string, ITorchInstanceInfo> _instances = new();

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly Timer CleanupTimer;


        public InstanceManager() 
        {

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

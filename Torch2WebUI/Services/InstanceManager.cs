using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Torch2API.DTOs.Instances;
using Torch2WebUI.Services.SQL;
using Timer = System.Timers.Timer;

namespace Torch2WebUI.Services
{
    public class InstanceManager
    {
        public ConcurrentDictionary<string, TorchInstanceBase> PendingInstances { get; private set; } = new();
        public ConcurrentDictionary<string, TorchInstanceBase> ActiveInstances { get; private set; } = new();

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly Timer CleanupTimer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;

        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        //Do not need to notify the page when its a bind
        public bool EnableServerDiscovery { get; set; } = false;



        public InstanceManager(IServiceScopeFactory scopeFactory, IMemoryCache cache) 
        {
            _cache = cache;
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

        public void UpdateStatus(TorchInstanceBase instance)
        {
            if (!RegisterInstance(instance))
                return;

            // Update last seen or other status info here
            if (ActiveInstances.ContainsKey(instance.InstanceID))
            {
                ActiveInstances[instance.InstanceID].UpdateFromConfiguredInstance(instance);
            }

            if (PendingInstances.ContainsKey(instance.InstanceID))
            {
                PendingInstances[instance.InstanceID].UpdateFromConfiguredInstance(instance);
            }

            NotifyStateChanged();
            return;
        }

        public bool RegisterInstance(TorchInstanceBase instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.InstanceID))
                return false;

            instance.LastUpdate = DateTime.UtcNow;
            if (ActiveInstances.ContainsKey(instance.InstanceID))
                return true;

            if (PendingInstances.ContainsKey(instance.InstanceID))
                return true;



            using (var scope = _scopeFactory.CreateScope())
            {
                var _database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (_database.ConfiguredInstances.Any(i => i.InstanceID == instance.InstanceID))
                {
                    ActiveInstances.TryAdd(instance.InstanceID, instance);
                    NotifyStateChanged();
                    return true;
                }
            }
                
            
            if (EnableServerDiscovery)
            {
                PendingInstances.TryAdd(instance.InstanceID, instance);
                NotifyStateChanged();
                return true;
            }

            return false;
        }

        public void AdoptInstance(string instanceID)
        {
            if (PendingInstances.TryRemove(instanceID, out var instance))
            {
                ActiveInstances.TryAdd(instanceID, instance);
            }
        }

    }
}

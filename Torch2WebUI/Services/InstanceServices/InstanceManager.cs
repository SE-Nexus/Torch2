using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;
using Torch2API.DTOs.Instances;
using Torch2WebUI.Components.Pages;
using Torch2WebUI.Services.SQL;
using Timer = System.Timers.Timer;

namespace Torch2WebUI.Services.InstanceServices
{
    public class InstanceManager
    {
        public ConcurrentDictionary<string, TorchInstanceBase> PendingInstances { get; private set; } = new();
        public ConcurrentDictionary<string, TorchInstanceBase> ActiveInstances { get; private set; } = new();

        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);
        private readonly Timer CleanupTimer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private readonly InstanceSocketManager _InstanceSocketManager;

        public event Action<string>? OnChange;
        private void NotifyStateChanged(string instanceid) => OnChange?.Invoke(instanceid);

        //Do not need to notify the page when its a bind
        public bool EnableServerDiscovery { get; set; } = false;



        public InstanceManager(IServiceScopeFactory scopeFactory, IMemoryCache cache, InstanceSocketManager socketmanager) 
        {
            _InstanceSocketManager = socketmanager; 
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

            NotifyStateChanged(instance.InstanceID);
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
                    NotifyStateChanged(instance.InstanceID);
                    return true;
                }
            }
                
            
            if (EnableServerDiscovery)
            {
                PendingInstances.TryAdd(instance.InstanceID, instance);
                NotifyStateChanged(instance.InstanceID);
                return true;
            }

            return false;
        }

        public TorchInstanceBase? GetInstanceByID(string instanceID)
        {
            if (string.IsNullOrWhiteSpace(instanceID))
                return null;

            // 1️⃣ Exact match (fast path)
            if (ActiveInstances.TryGetValue(instanceID, out var active))
                return active;

            if (PendingInstances.TryGetValue(instanceID, out var pending))
                return pending;

            // 2️⃣ Short ID match (last 6 chars)
            if (instanceID.Length == 6)
            {
                var match = ActiveInstances.Values
                    .Concat(PendingInstances.Values)
                    .Where(i => !string.IsNullOrEmpty(i.InstanceID))
                    .Where(i => i.InstanceID.Length >= 6)
                    .Where(i => i.InstanceID
                        .Substring(i.InstanceID.Length - 6, 6)
                        .Equals(instanceID, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Safety: only return if exactly one match
                if (match.Count == 1)
                    return match[0];
            }

            return null;
        }

        public void AdoptInstance(string instanceID)
        {
            if (PendingInstances.TryRemove(instanceID, out var instance))
            {
                ActiveInstances.TryAdd(instanceID, instance);
            }
        }

        public async Task SendCommand(TorchInstanceBase instanceBase, string command)
        {
            // Placeholder for sending command to instance
            // Implementation depends on communication method (e.g., WebSocket, HTTP, etc.)

            await _InstanceSocketManager.SendCommandAsync(instanceBase.InstanceID, "restart");
        }

    }
}

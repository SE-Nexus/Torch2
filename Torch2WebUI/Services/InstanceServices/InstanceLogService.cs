using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Torch2API.DTOs.Logs;

namespace Torch2WebUI.Services.InstanceServices
{
    /// <summary>
    /// Panel-side log store. Keeps a rolling history per instance and notifies
    /// Blazor components via <see cref="OnLog"/> when new entries arrive.
    /// </summary>
    public class InstanceLogService
    {
        public static int MaxPerInstance = 2000;

        private readonly ConcurrentDictionary<string, Queue<LogLine>> _histories = new();
        private readonly object _lock = new();

        /// <summary>Raised on the thread that appended the entry: (instanceId, entry).</summary>
        public event Action<string, LogLine>? OnLog;

        public void Append(string instanceId, LogLine entry)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<LogLine>(MaxPerInstance));
                q.Enqueue(entry);
                if (q.Count > MaxPerInstance)
                    q.Dequeue();
            }

            OnLog?.Invoke(instanceId, entry);
        }

        public void AppendHistory(string instanceId, IEnumerable<LogLine> entries)
        {
            lock (_lock)
            {
                var q = _histories.GetOrAdd(instanceId, _ => new Queue<LogLine>(MaxPerInstance));
                foreach (var entry in entries)
                {
                    q.Enqueue(entry);
                    if (q.Count > MaxPerInstance)
                        q.Dequeue();
                }
            }
        }

        public LogLine[] GetHistory(string instanceId)
        {
            lock (_lock)
            {
                return _histories.TryGetValue(instanceId, out var q)
                    ? q.ToArray()
                    : Array.Empty<LogLine>();
            }
        }
    }
}

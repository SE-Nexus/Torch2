using System;
using System.Collections.Generic;
using Torch2API.DTOs.Logs;

namespace InstanceUtils.Logging
{
    /// <summary>
    /// Thread-safe circular buffer that captures NLog entries and broadcasts them to live subscribers.
    /// Static singleton so NLog targets can reach it without DI.
    /// </summary>
    public sealed class LogBuffer
    {
        public static readonly LogBuffer Instance = new LogBuffer();

        private const int MaxCapacity = 1000;
        private readonly Queue<LogLine> _ring = new Queue<LogLine>(MaxCapacity);
        private readonly object _lock = new object();

        public event Action<LogLine> OnLog;

        private LogBuffer() { }

        public void Add(LogLine entry)
        {
            lock (_lock)
            {
                _ring.Enqueue(entry);
                if (_ring.Count > MaxCapacity)
                    _ring.Dequeue();
            }

            OnLog?.Invoke(entry);
        }

        public LogLine[] GetHistory()
        {
            lock (_lock)
                return _ring.ToArray();
        }
    }
}

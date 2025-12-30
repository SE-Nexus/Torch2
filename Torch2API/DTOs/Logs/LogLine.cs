using System;
using System.Collections.Generic;
using System.Text;

namespace Torch2API.DTOs.Logs
{
    public class LogLine
    {
        public string InstanceName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

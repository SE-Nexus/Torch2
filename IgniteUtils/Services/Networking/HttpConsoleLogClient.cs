using NLog;
using NLog.Common;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.DTOs.Logs;
using VRage.Scripting;

namespace InstanceUtils.Services.Networking
{
    [Target("HttpIntercept")]
    public class HttpConsoleLogClient : Target
    {
        private CancellationTokenSource _cts;
        private Task _workerTask;




        private readonly HttpClient _http;

        public HttpConsoleLogClient(HttpClient http)
        {
            
            _http = http;
        }

        private void InitializeHttpClient()
        {
            // Set up HttpClient if needed (e.g., default headers, base address)
            _cts = new CancellationTokenSource();
            
        }


        public async Task SendLogLineAsync(string line)
        {
            var log = new LogLine
            {
                InstanceName = "MyNewInstance",
                Level = "INFO",
                Message = line,
                Timestamp = DateTime.UtcNow
            };

            await _http.PostAsJsonAsync("/api/instance/logstream", log);
        }

        protected override void Write(LogEventInfo logEvent)
        {

            base.Write(logEvent);
        }


    }
}

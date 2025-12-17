using NLog;
using NLog.Common;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IgniteUtils.Services.Networking
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
            var content = new StringContent(line, Encoding.UTF8, "text/plain");
            await _http.PostAsync("/api/console/log", content);
        }

        protected override void Write(LogEventInfo logEvent)
        {

            base.Write(logEvent);
        }


    }
}

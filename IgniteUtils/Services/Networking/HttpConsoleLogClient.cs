using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IgniteUtils.Services.Networking
{
    internal class HttpConsoleLogClient
    {
        private readonly HttpClient _http;

        public HttpConsoleLogClient(HttpClient http)
        {
            _http = http;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace IgniteUtils.Services.WebPanel
{
    public class PanelHTTPClient
    {
        public HttpClient Http { get; }

        public PanelHTTPClient(HttpClient http)
        {
            Http = http;
        }

    }
}

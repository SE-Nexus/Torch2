using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelHTTPClient
    {
        const int DefaultTimeoutSeconds = 5;

        public HttpClient Http { get; }

        public PanelHTTPClient(HttpClient http)
        {
            Http = http;
        }

        public async Task<bool> PostAsync<T>(string path,T payload,CancellationToken ct = default, TimeSpan? timeout = null)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(DefaultTimeoutSeconds));

            try
            {
                var response = await Http.PostAsJsonAsync(
                    path,
                    payload,
                    cts.Token);

                return response.IsSuccessStatusCode;
            }
            catch (OperationCanceledException)
            {
                // timeout or shutdown
                return false;
            }
            catch (HttpRequestException)
            {
                // panel unreachable
                return false;
            }
        }

    }
}

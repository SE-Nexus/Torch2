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

                if (!response.IsSuccessStatusCode)
                {
                    string body;
                    try
                    {
                        // Read the error body to see problem details from the server
                        body = await response.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        body = $"<failed to read response body: {ex.Message}>";
                    }

                    // Minimal logging so the calling process (and the debugger/console) can see the server error
                    Console.WriteLine($"PanelHTTPClient POST {path} returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");

                    return false;
                }

                return true;
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

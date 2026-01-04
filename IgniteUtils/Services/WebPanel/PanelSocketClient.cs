using InstanceUtils.Models.Server;
using Sandbox.Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Constants;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelSocketClient
    {
        private TaskCompletionSource<bool> _disconnected =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly IConfigService _ConfigService;
        private readonly IPanelCoreService _PanelCore;
        private ClientWebSocket? _socket;

        public PanelSocketClient(IConfigService config, IPanelCoreService panelCore)
        {
            _ConfigService = config;
            _PanelCore = panelCore;
        }

        // 🔁 Public entry point — call this ONCE at startup
        public async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _disconnected = new(TaskCreationOptions.RunContinuationsAsynchronously);

                try
                {
                    await ConnectAsync(ct);

                    // Wait here until listener signals disconnect
                    await Task.WhenAny(_disconnected.Task, Task.Delay(Timeout.Infinite, ct));
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch(WebSocketException ex)
                {
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket error: {ex}");
                }
                finally
                {
                    CleanupSocket();
                }

                // ⏳ backoff before retry
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }

        private async Task ConnectAsync(CancellationToken ct)
        {
            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader(
                TorchConstants.InstanceIdHeader,
                _ConfigService.Identification.GetInstanceID().ToString());

            var panelUrl = _ConfigService.TargetWebPanel;

            var wsUri = new UriBuilder(panelUrl)
            {
                Scheme = panelUrl.Scheme switch
                {
                    "https" => "wss",
                    "http" => "ws",
                    _ => throw new InvalidOperationException(
                        $"Unsupported scheme: {panelUrl.Scheme}")
                },
                Path = "/ws/instance"
            }.Uri;

            await _socket.ConnectAsync(wsUri, ct);

            _ = ListenAsync(ct); // fire-and-forget
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            try
            {
                var buffer = new byte[4096];
                var segment = new ArraySegment<byte>(buffer);

                while (_socket?.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await _socket.ReceiveAsync(segment, ct);

                        if (result.MessageType == WebSocketMessageType.Close)
                            return;

                        ms.Write(buffer, 0, result.Count);

                    } while (!result.EndOfMessage);

                    await _PanelCore.RunWSCommand(Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            catch (OperationCanceledException)
            {
                // normal shutdown
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Listen failed: {ex}");
            }
            finally
            {
                _disconnected.TrySetResult(true);
            }
        }

        private void CleanupSocket()
        {
            try
            {
                _socket?.Abort();
                _socket?.Dispose();
            }
            catch
            {
                // ignore cleanup failures
            }

            _socket = null;
        }
    }
}
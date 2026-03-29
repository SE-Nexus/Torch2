using InstanceUtils.Logging;
using InstanceUtils.Models.Server;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Torch2API.Constants;
using Torch2API.DTOs.Logs;
using Torch2API.DTOs.WebSockets;

namespace InstanceUtils.Services.WebPanel
{
    public class PanelSocketClient
    {
        private TaskCompletionSource<bool> _disconnected =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly IConfigService _ConfigService;
        private readonly IPanelCoreService _PanelCore;
        private ClientWebSocket? _socket;

        private ConcurrentQueue<byte[]> _sendQueue = new ConcurrentQueue<byte[]>();
        private SemaphoreSlim _sendSignal = new SemaphoreSlim(0);
        private CancellationTokenSource? _connectionCts;
        private Action<LogLine>? _logHandler;

        private bool _IsConnected = false;



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
                    //Console.WriteLine("Couldnt Connect");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WebSocket error: {ex}");
                }
                finally
                {
                    _IsConnected = false;
                    UnsubscribeLog();
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

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _socket.ConnectAsync(wsUri, cancellationTokenSource.Token);

            // Fresh send queue and connection-scoped cancellation for this session
            _sendQueue = new ConcurrentQueue<byte[]>();
            _sendSignal = new SemaphoreSlim(0);
            _connectionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _ = SendLoopAsync(_connectionCts.Token);
            _ = ListenAsync(ct);

            // Subscribe to live log events and immediately flush buffered history
            _logHandler = line => EnqueueEnvelope(TorchConstants.WsLog, line);
            LogBuffer.Instance.OnLog += _logHandler;
            EnqueueEnvelope(TorchConstants.WsLogHistory, LogBuffer.Instance.GetHistory());
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            try
            {
                var buffer = new byte[4096];
                var segment = new ArraySegment<byte>(buffer);

                while (_socket?.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    _IsConnected = true;
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await _socket.ReceiveAsync(segment, ct);

                        if (result.MessageType == WebSocketMessageType.Close)
                            return;

                        ms.Write(buffer, 0, result.Count);

                    } while (!result.EndOfMessage);


                    var json = Encoding.UTF8.GetString(ms.ToArray());

                    SocketMsgEnvelope envelope;
                    try
                    {
                        envelope = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions)
                            ?? throw new JsonException("Envelope was null");
                    }
                    catch (Exception ex)
                    {
                        // Bad payload — log and ignore
                        Console.WriteLine($"Invalid WS message: {ex}");
                        continue;
                    }

                    await _PanelCore.RunWSCommand(envelope);
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
                _IsConnected = false;
                _disconnected.TrySetResult(true);
            }
        }

        public async Task ShutdownAsync(CancellationToken ct = default)
        {
            var socket = _socket;

            if (socket == null)
                return;

            try
            {
                // Optional but STRONGLY recommended:
                // Tell the panel this is intentional
                var shutdownEnvelope = new SocketMsgEnvelope("instance.shutdown");

                var json = JsonSerializer.Serialize(shutdownEnvelope, TorchConstants.JsonOptions);
                var bytes = Encoding.UTF8.GetBytes(json);

                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(bytes),WebSocketMessageType.Text,true,ct);
                }
            }
            catch
            {
                // Ignore — shutdown should never fail because of messaging
            }

            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Instance shutting down",
                        ct);
                }
            }
            catch
            {
                // Ignore close failures
            }
            finally
            {
                _disconnected.TrySetResult(true);
                CleanupSocket();
            }
        }

        private void EnqueueEnvelope<T>(string command, T payload)
        {
            var envelope = new SocketMsgEnvelope(command)
            {
                Args = JsonSerializer.SerializeToElement(payload, TorchConstants.JsonOptions)
            };

            var bytes = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(envelope, TorchConstants.JsonOptions));

            _sendQueue.Enqueue(bytes);
            try { _sendSignal.Release(); } catch { }
        }

        private async Task SendLoopAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    await _sendSignal.WaitAsync(ct);

                    while (_sendQueue.TryDequeue(out var bytes))
                    {
                        if (_socket?.State != WebSocketState.Open) return;

                        await _socket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            ct);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Send loop error: {ex}");
            }
        }

        private void UnsubscribeLog()
        {
            if (_logHandler != null)
            {
                LogBuffer.Instance.OnLog -= _logHandler;
                _logHandler = null;
            }

            _connectionCts?.Cancel();
            _connectionCts?.Dispose();
            _connectionCts = null;
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
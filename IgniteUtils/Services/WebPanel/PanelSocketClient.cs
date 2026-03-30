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
        private const int ReceiveBufferSize = 4096;
        private const int ConnectionTimeoutSeconds = 10;
        private const int ReconnectionDelaySeconds = 5;
        private const string ShutdownCommandName = "instance.shutdown";
        private const string ShutdownMessage = "Instance shutting down";
        private const string WebSocketPath = "/ws/instance";

        private TaskCompletionSource<bool> _disconnected =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly IConfigService _configService;
        private readonly IPanelCoreService _panelCore;
        private ClientWebSocket? _socket;

        private ConcurrentQueue<byte[]> _sendQueue = new();
        private SemaphoreSlim _sendSignal = new(0);
        private CancellationTokenSource? _connectionCts;
        private Action<LogLine>? _logHandler;

        private bool _isConnected;

        public PanelSocketClient(IConfigService config, IPanelCoreService panelCore)
        {
            _configService = config;
            _panelCore = panelCore;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                _disconnected = new(TaskCreationOptions.RunContinuationsAsynchronously);

                try
                {
                    await ConnectAsync(ct);
                    await Task.WhenAny(_disconnected.Task, Task.Delay(Timeout.Infinite, ct));
                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (WebSocketException ex)
                {
                    //Connection failed, will retry
                }
                catch (Exception ex)
                {
                    //Unexpected error, will retry
                }
                finally
                {
                    _isConnected = false;
                    UnsubscribeLog();
                    CleanupSocket();
                }

                if (!ct.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(ReconnectionDelaySeconds), ct);
                }
            }
        }

        private async Task ConnectAsync(CancellationToken ct)
        {
            _socket = new ClientWebSocket();
            _socket.Options.SetRequestHeader(
                TorchConstants.InstanceIdHeader,
                _configService.Identification.GetInstanceID().ToString());

            var panelUrl = _configService.TargetWebPanel;

            var wsUri = new UriBuilder(panelUrl)
            {
                Scheme = panelUrl.Scheme switch
                {
                    "https" => "wss",
                    "http" => "ws",
                    _ => throw new InvalidOperationException(
                        $"Unsupported scheme: {panelUrl.Scheme}")
                },
                Path = WebSocketPath
            }.Uri;

            var connectionTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(ConnectionTimeoutSeconds));

            try
            {
                await _socket.ConnectAsync(wsUri, connectionTimeout.Token);
            }
            finally
            {
                connectionTimeout.Dispose();
            }

            _sendQueue = new();
            _sendSignal = new(0);
            _connectionCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _ = SendLoopAsync(_connectionCts.Token);
            _ = ListenAsync(ct);

            _logHandler = line => EnqueueEnvelope(TorchConstants.WsLog, line);
            LogBuffer.Instance.OnLog += _logHandler;
            EnqueueEnvelope(TorchConstants.WsLogHistory, LogBuffer.Instance.GetHistory());
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            try
            {
                var buffer = new byte[ReceiveBufferSize];
                var segment = new ArraySegment<byte>(buffer);

                while (_socket?.State == WebSocketState.Open && !ct.IsCancellationRequested)
                {
                    _isConnected = true;

                    var message = await ReceiveMessageAsync(buffer, segment, ct);
                    if (message is null)
                        break;

                    await HandleMessageAsync(message);
                }
            }
            catch (OperationCanceledException)
            {
                //Normal shutdown
            }
            catch (Exception ex)
            {
                //Listen failed
            }
            finally
            {
                _isConnected = false;
                _disconnected.TrySetResult(true);
            }
        }

        private async Task<string?> ReceiveMessageAsync(byte[] buffer, ArraySegment<byte> segment, CancellationToken ct)
        {
            try
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await _socket!.ReceiveAsync(segment, ct);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return null;
                    }

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task HandleMessageAsync(string json)
        {
            SocketMsgEnvelope? envelope;

            try
            {
                envelope = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions)
                    ?? throw new JsonException("Deserialized envelope was null");
            }
            catch (Exception ex)
            {
                return;
            }

            try
            {
                await _panelCore.RunWSCommand(envelope);
            }
            catch (Exception ex)
            {
                //Error processing command
            }
        }

        public async Task ShutdownAsync(CancellationToken ct = default)
        {
            var socket = _socket;

            if (socket == null)
                return;

            try
            {
                var shutdownEnvelope = new SocketMsgEnvelope(ShutdownCommandName);
                await SendEnvelopeAsync(socket, shutdownEnvelope, ct);
            }
            catch (Exception ex)
            {
                //Error sending shutdown message
            }

            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        ShutdownMessage,
                        ct);
                }
            }
            catch (Exception ex)
            {
                //Error closing socket
            }
            finally
            {
                _disconnected.TrySetResult(true);
                CleanupSocket();
            }
        }

        private async Task SendEnvelopeAsync(ClientWebSocket socket, SocketMsgEnvelope envelope, CancellationToken ct)
        {
            var bytes = SerializeEnvelope(envelope);

            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    ct);
            }
        }

        private void EnqueueEnvelope<T>(string command, T payload)
        {
            var envelope = new SocketMsgEnvelope(command)
            {
                Args = JsonSerializer.SerializeToElement(payload, TorchConstants.JsonOptions)
            };

            var bytes = SerializeEnvelope(envelope);
            _sendQueue.Enqueue(bytes);

            try
            {
                _sendSignal.Release();
            }
            catch (ObjectDisposedException)
            {
                //SendSignal was disposed
            }
        }

        private static byte[] SerializeEnvelope(SocketMsgEnvelope envelope)
        {
            var json = JsonSerializer.Serialize(envelope, TorchConstants.JsonOptions);
            return Encoding.UTF8.GetBytes(json);
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
                        if (_socket?.State != WebSocketState.Open)
                            return;

                        await _socket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //SendLoop cancelled
            }
            catch (Exception ex)
            {
                //SendLoop error
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

            DisposeSendSignal();
        }

        private void DisposeSendSignal()
        {
            try
            {
                _sendSignal?.Dispose();
            }
            catch (Exception ex)
            {
                //Error disposing SendSignal
            }
        }

        private void CleanupSocket()
        {
            try
            {
                _socket?.Abort();
                _socket?.Dispose();
            }
            catch (Exception ex)
            {
                //Error cleaning up socket
            }

            _socket = null;
        }
    }
}
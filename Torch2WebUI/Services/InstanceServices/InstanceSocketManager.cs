using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Torch2API.Constants;
using Torch2API.DTOs.Logs;
using Torch2API.DTOs.WebSockets;

namespace Torch2WebUI.Services.InstanceServices
{
    public class InstanceSocketManager
    {
        private const int ReceiveBufferSize = 4096;
        private const string MissingInstanceIdMessage = "Missing Instance-Id header";
        private const string NormalClosureMessage = "Closing";

        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly InstanceLogService _logService;
        private readonly ILogger<InstanceSocketManager> _logger;

        public InstanceSocketManager(InstanceLogService logService, ILogger<InstanceSocketManager> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        public async Task HandleConnectionAsync(HttpContext context, WebSocket socket)
        {
            if (!context.Request.Headers.TryGetValue(TorchConstants.InstanceIdHeader, out var instanceId))
            {
                _logger.LogWarning("WebSocket connection rejected: missing Instance-Id header");
                await socket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    MissingInstanceIdMessage,
                    CancellationToken.None);
                return;
            }

            var id = instanceId.ToString();
            _sockets[id] = socket;
            _logger.LogInformation("WebSocket connection established for instance {InstanceId}", id);

            await ListenAsync(id, socket);
        }

        private async Task ListenAsync(string instanceId, WebSocket socket)
        {
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var message = await ReceiveMessageAsync(instanceId, socket);
                    if (message is null)
                        break;

                    HandleMessage(instanceId, message);
                }
            }
            finally
            {
                await CloseSocketAsync(instanceId, socket);
            }
        }

        private async Task<string?> ReceiveMessageAsync(string instanceId, WebSocket socket)
        {
            var buffer = new byte[ReceiveBufferSize];

            try
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation(
                            "Instance {InstanceId} closed gracefully: {CloseStatus} - {CloseStatusDescription}",
                            instanceId,
                            result.CloseStatus,
                            result.CloseStatusDescription);
                        return null;
                    }

                    ms.Write(buffer, 0, result.Count);
                }
                while (!result.EndOfMessage);

                return Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket exception for instance {InstanceId}", instanceId);
                return null;
            }
        }

        private async Task CloseSocketAsync(string instanceId, WebSocket socket)
        {
            _sockets.TryRemove(instanceId, out _);

            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        NormalClosureMessage,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing WebSocket for instance {InstanceId}", instanceId);
            }
        }

        private void HandleMessage(string instanceId, string json)
        {
            SocketMsgEnvelope? envelope;

            try
            {
                envelope = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions);
                if (envelope is null)
                {
                    _logger.LogWarning("Received null envelope for instance {InstanceId}", instanceId);
                    return;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize message for instance {InstanceId}", instanceId);
                return;
            }

            try
            {
                switch (envelope.Command)
                {
                    case TorchConstants.WsLog:
                        HandleLogMessage(instanceId, envelope);
                        break;

                    case TorchConstants.WsLogHistory:
                        HandleLogHistoryMessage(instanceId, envelope);
                        break;

                    default:
                        _logger.LogDebug("Unknown command for instance {InstanceId}: {Command}", instanceId, envelope.Command);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling message for instance {InstanceId}", instanceId);
            }
        }

        private void HandleLogMessage(string instanceId, SocketMsgEnvelope envelope)
        {
            var entry = envelope.Args.Deserialize<LogLine>(TorchConstants.JsonOptions);
            if (entry is not null)
                _logService.Append(instanceId, entry);
        }

        private void HandleLogHistoryMessage(string instanceId, SocketMsgEnvelope envelope)
        {
            var history = envelope.Args.Deserialize<LogLine[]>(TorchConstants.JsonOptions);
            if (history is not null)
                _logService.AppendHistory(instanceId, history);
        }

        public async Task SendCommandAsync(string instanceId, string rawcommand, object args)
        {
            if (!_sockets.TryGetValue(instanceId, out var socket))
            {
                _logger.LogWarning("Socket not found for instance {InstanceId}", instanceId);
                return;
            }

            try
            {
                string normalized = NormalizeCommand(rawcommand);
                var envelope = new SocketMsgEnvelope(normalized)
                {
                    Args = JsonSerializer.SerializeToElement(args, TorchConstants.JsonOptions)
                };

                string json = JsonSerializer.Serialize(envelope, TorchConstants.JsonOptions);
                byte[] data = Encoding.UTF8.GetBytes(json);

                await socket.SendAsync(
                    new ArraySegment<byte>(data),
                    WebSocketMessageType.Text,
                    endOfMessage: true,
                    CancellationToken.None);

                _logger.LogDebug("Command sent to instance {InstanceId}: {Command}", instanceId, normalized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command to instance {InstanceId}", instanceId);
            }
        }

        private static string NormalizeCommand(string rawcommand)
        {
            return rawcommand.Replace(' ', '.').Replace('/', '.');
        }
    }
}


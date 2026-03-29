using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Torch2API.Constants;
using Torch2API.DTOs.Logs;
using Torch2API.DTOs.WebSockets;

namespace Torch2WebUI.Services.InstanceServices
{
    public class InstanceSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly InstanceLogService _logService;

        public InstanceSocketManager(InstanceLogService logService)
        {
            _logService = logService;
        }

        public async Task HandleConnectionAsync(HttpContext context, WebSocket socket)
        {
            if (!context.Request.Headers.TryGetValue(TorchConstants.InstanceIdHeader, out var instanceId))
            {
                await socket.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Missing Instance-Id header",
                    CancellationToken.None);
                return;
            }

            var id = instanceId.ToString();
            _sockets[id] = socket;

            await Listen(id, socket);
        }

        private async Task Listen(string instanceId, WebSocket socket)
        {
            var buffer = new byte[4096];
            bool gracefulClose = false;

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    using var ms = new MemoryStream();
                    WebSocketReceiveResult result;

                    try
                    {
                        do
                        {
                            result = await socket.ReceiveAsync(
                                new ArraySegment<byte>(buffer),
                                CancellationToken.None);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                gracefulClose = true;
                                Console.WriteLine($"Instance {instanceId} closed gracefully: {result.CloseStatus} - {result.CloseStatusDescription}");
                                return;
                            }

                            ms.Write(buffer, 0, result.Count);
                        }
                        while (!result.EndOfMessage);
                    }
                    catch (WebSocketException ex)
                    {
                        Console.WriteLine($"WebSocketException for instance {instanceId}: {ex.Message}");
                        return;
                    }

                    HandleMessage(instanceId, Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            finally
            {
                _sockets.TryRemove(instanceId, out _);

                try
                {
                    if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    {
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Closing",
                            CancellationToken.None);
                    }
                }
                catch { }
            }
        }

        private void HandleMessage(string instanceId, string json)
        {
            SocketMsgEnvelope? envelope;
            try
            {
                envelope = JsonSerializer.Deserialize<SocketMsgEnvelope>(json, TorchConstants.JsonOptions);
                if (envelope is null) return;
            }
            catch
            {
                return;
            }

            switch (envelope.Command)
            {
                case TorchConstants.WsLog:
                    var entry = envelope.Args.Deserialize<LogLine>(TorchConstants.JsonOptions);
                    if (entry is not null)
                        _logService.Append(instanceId, entry);
                    break;

                case TorchConstants.WsLogHistory:
                    var history = envelope.Args.Deserialize<LogLine[]>(TorchConstants.JsonOptions);
                    if (history is not null)
                        _logService.AppendHistory(instanceId, history);
                    break;
            }
        }

        public async Task SendCommandAsync(string instanceId, string rawcommand, object args)
        {
            if (!_sockets.TryGetValue(instanceId, out var socket))
                return;

            string normalized = rawcommand.Replace(' ', '.').Replace('/', '.');
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
        }
    }
}


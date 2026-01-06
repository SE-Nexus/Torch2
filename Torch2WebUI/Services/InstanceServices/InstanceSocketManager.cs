using SQLitePCL;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Torch2API.Constants;
using Torch2API.DTOs.WebSockets;

namespace Torch2WebUI.Services.InstanceServices
{
    public class InstanceSocketManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();

        public async Task HandleConnectionAsync(HttpContext context, WebSocket socket)
        {
            // instance identifies itself
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
            var buffer = new byte[1024];
            bool gracefulClose = false;

            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;

                    try
                    {
                        result = await socket.ReceiveAsync(
                            new ArraySegment<byte>(buffer),
                            CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        // Ungraceful termination (crash, kill, network failure)
                        Console.WriteLine($"WebSocketException for instance {instanceId}: {ex.Message}");
                        //LogCrash(instanceId, ex);
                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        gracefulClose = true;
                        //LogGracefulShutdown(instanceId, result.CloseStatus);
                        Console.WriteLine($"Instance {instanceId} closed the connection gracefully: {result.CloseStatus} - {result.CloseStatusDescription}");
                        return;
                    }

                    // Normal message handling here
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
                catch
                {
                    // Ignore shutdown failures
                }

                if (!gracefulClose)
                {
                    // Last chance detection
                    //(instanceId);
                }
            }
        }

        public async Task SendCommandAsync(string instanceId, string rawcommand)
        {
            if (!_sockets.TryGetValue(instanceId, out var socket))
                return;


            string normalized = rawcommand.Replace(' ', '.').Replace('/', '.');
            var envelope = new SocketMsgEnvelope(normalized);


            string json = JsonSerializer.Serialize(envelope);
            byte[] data = Encoding.UTF8.GetBytes(json);

            await socket.SendAsync(
                new ArraySegment<byte>(data),
                WebSocketMessageType.Text,
                endOfMessage: true,
                CancellationToken.None);
        }

    }
}

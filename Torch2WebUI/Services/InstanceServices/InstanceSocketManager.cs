using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Torch2API.Constants;

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

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _sockets.TryRemove(instanceId, out _);
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closed",
                        CancellationToken.None);
                }
            }
        }

        public async Task SendCommandAsync(string instanceId, string command)
        {
            if (_sockets.TryGetValue(instanceId, out var socket))
            {
                var data = Encoding.UTF8.GetBytes(command);
                await socket.SendAsync(
                    data,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

    }
}

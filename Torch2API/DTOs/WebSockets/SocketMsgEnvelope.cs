using System;
using System.Text.Json;

namespace Torch2API.DTOs.WebSockets
{
    public sealed class SocketMsgEnvelope
    {
        public string Command { get; set; }

        public string? RequestId { get; set; }

        public string? UserID { get; set; }

        // Named arguments object: { "worldname": "...", "template": "..." }
        public JsonElement Args { get; set; }

        public SocketMsgEnvelope(string command)
        {
            Command = command;
            Args = default;
        }
    }
}
